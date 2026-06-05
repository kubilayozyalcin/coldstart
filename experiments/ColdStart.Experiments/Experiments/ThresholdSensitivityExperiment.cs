using System.Diagnostics;
using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.Experiments.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Experiments.Experiments;

/// <summary>
/// Eşik hassasiyet (sensitivity) deneyi: farklı corpus boyutlarında her üç
/// katman <em>doğrudan</em> (router atlanarak) sorgulanır ve kaliteleri
/// karşılaştırılır. "Hangi belge sayısında hangi katman daha iyi?" sorusunun
/// cevabı, eşik değerlerinin (50/200) ampirik gerekçesidir. LLM'siz modda
/// yalnızca hit@K ve süre; <c>--with-llm</c> ile hakem skorları da ölçülür.
/// </summary>
public static class ThresholdSensitivityExperiment
{
    private static readonly int[] CorpusSizes = { 10, 25, 50, 100 };

    /// <summary>Deneyi koşar ve CSV dosya yolunu döner.</summary>
    public static async Task<string> RunAsync(string repoRoot, bool withLlm)
    {
        var rows = new List<string>();

        await using var provider = ExperimentHost.Build(new Dictionary<string, string?>
        {
            ["Qdrant:CollectionName"] = "coldstart_chunks_exp"
        });

        var store = provider.GetRequiredService<IDocumentStore>();
        var layers = provider.GetServices<ISearchLayer>().OrderBy(l => l.LayerNumber).ToArray();
        var evaluator = provider.GetRequiredService<IRagEvaluator>();

        foreach (int size in CorpusSizes)
        {
            await store.ClearAsync();
            foreach (Document document in SyntheticCorpus.Generate(repoRoot, size))
                await store.UpsertAsync(document);

            foreach (ISearchLayer layer in layers)
            {
                // Layer 2/3 her sorguda OpenAI'a gider; LLM izni yoksa yalnız Layer 1 ölçülür.
                if (!withLlm && layer.LayerNumber != 1)
                    continue;

                foreach (GoldenQuery golden in GoldenQueries.All)
                {
                    long start = Stopwatch.GetTimestamp();
                    Result<SearchResponse> result = await layer.SearchAsync(
                        new SearchRequest { Query = golden.Query, TopK = 3 });
                    long durationMs = (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;

                    if (result.IsFailure)
                    {
                        rows.Add($"{size},{layer.LayerNumber},\"{golden.Query}\",0,,,{durationMs},{result.Error.Code}");
                        continue;
                    }

                    bool hit = result.Value.Sources.Any(s => s.DocumentId == golden.ExpectedDocumentId);

                    string relevancy = "";
                    string faithfulness = "";
                    if (withLlm)
                    {
                        Result<RagEvaluation> judged = await evaluator.EvaluateAsync(
                            golden.Query, result.Value.Answer, result.Value.Sources);
                        if (judged.IsSuccess)
                        {
                            relevancy = CsvWriter.F(judged.Value.AnswerRelevancy);
                            faithfulness = CsvWriter.F(judged.Value.Faithfulness);
                        }
                    }

                    rows.Add($"{size},{layer.LayerNumber},\"{golden.Query}\",{(hit ? 1 : 0)},{faithfulness},{relevancy},{durationMs},");
                }
            }

            Console.WriteLine($"Corpus {size} belge: {(withLlm ? "3 katman" : "yalnız Layer 1")} ölçüldü.");
        }

        return CsvWriter.Write(repoRoot, "threshold-sensitivity",
            "corpusSize,layer,query,sourceHit,faithfulness,relevancy,durationMs,error",
            rows);
    }
}
