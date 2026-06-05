using System.Diagnostics;
using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.Experiments.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Experiments.Experiments;

/// <summary>
/// Activation Time deneyi: corpus belge belge büyürken sistem altın sorguya
/// kaç belgede doğru kaynağı getirebiliyor? Birincil metrik deterministik
/// <c>hit@K</c> (beklenen belge kaynaklarda mı); ikincil metrik (opsiyonel,
/// <c>--with-llm</c>) hakem relevancy skorudur. Üç strateji karşılaştırılır:
/// adaptif pipeline (cold start tezi), yalnız-embedding ve yalnız-RAG baseline'ları.
/// </summary>
public static class ActivationTimeExperiment
{
    private static readonly int[] CorpusSizes = { 1, 3, 5, 10, 15, 20, 25, 30 };

    private static readonly (string Name, Dictionary<string, string?> Overrides)[] Strategies =
    {
        ("adaptive", new Dictionary<string, string?>
        {
            ["Pipeline:Layer2Threshold"] = "50",
            ["Pipeline:Layer3Threshold"] = "200"
        }),
        ("embedding-only", new Dictionary<string, string?>
        {
            ["Pipeline:Layer2Threshold"] = "0",
            ["Pipeline:Layer3Threshold"] = int.MaxValue.ToString()
        }),
        ("rag-only", new Dictionary<string, string?>
        {
            ["Pipeline:Layer2Threshold"] = "0",
            ["Pipeline:Layer3Threshold"] = "0",
            ["Qdrant:CollectionName"] = "coldstart_chunks_exp"
        })
    };

    /// <summary>Deneyi koşar ve CSV dosya yolunu döner.</summary>
    public static async Task<string> RunAsync(string repoRoot, bool withLlm)
    {
        var rows = new List<string>();

        foreach ((string strategy, Dictionary<string, string?> overrides) in Strategies)
        {
            // embedding-only ve rag-only stratejileri her boyutta OpenAI çağrısı yapar;
            // LLM izni yoksa yalnızca adaptif (Layer 1 bölgesi, ücretsiz) koşulur.
            if (!withLlm && strategy != "adaptive")
            {
                Console.WriteLine($"[{strategy}] atlandı (--with-llm gerekli).");
                continue;
            }

            await using var provider = ExperimentHost.Build(overrides);
            var store = provider.GetRequiredService<IDocumentStore>();
            var pipeline = provider.GetRequiredService<ISearchPipeline>();
            var evaluator = provider.GetRequiredService<IRagEvaluator>();

            var activation = GoldenQueries.All.ToDictionary(q => q.Query, _ => (int?)null);

            foreach (int size in CorpusSizes)
            {
                await store.ClearAsync();
                foreach (Document document in SyntheticCorpus.Generate(repoRoot, size))
                    await store.UpsertAsync(document);

                foreach (GoldenQuery golden in GoldenQueries.All)
                {
                    bool expectedPresent = (await store.GetAsync(golden.ExpectedDocumentId)) is not null;

                    long start = Stopwatch.GetTimestamp();
                    Result<SearchResponse> result = await pipeline.SearchAsync(
                        new SearchRequest { Query = golden.Query, TopK = 3 });
                    long durationMs = (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;

                    bool hit = result.IsSuccess &&
                               result.Value.Sources.Any(s => s.DocumentId == golden.ExpectedDocumentId);
                    if (hit && expectedPresent)
                        activation[golden.Query] ??= size;

                    string relevancy = "";
                    string faithfulness = "";
                    if (withLlm && result.IsSuccess)
                    {
                        Result<RagEvaluation> judged = await evaluator.EvaluateAsync(
                            golden.Query, result.Value.Answer, result.Value.Sources);
                        if (judged.IsSuccess)
                        {
                            relevancy = CsvWriter.F(judged.Value.AnswerRelevancy);
                            faithfulness = CsvWriter.F(judged.Value.Faithfulness);
                        }
                    }

                    int activeLayer = result.IsSuccess ? result.Value.ActiveLayer : 0;
                    rows.Add($"{strategy},{size},\"{golden.Query}\",{golden.ExpectedDocumentId}," +
                             $"{(expectedPresent ? 1 : 0)},{(hit ? 1 : 0)},{activeLayer},{durationMs},{faithfulness},{relevancy}");
                }
            }

            foreach ((string query, int? at) in activation)
                Console.WriteLine($"[{strategy}] aktivasyon — \"{query[..Math.Min(40, query.Length)]}...\": " +
                                  (at is null ? "ölçüm aralığında isabet yok" : $"{at} belge"));
        }

        return CsvWriter.Write(repoRoot, "activation-time",
            "strategy,corpusSize,query,expectedDocId,expectedPresent,sourceHit,activeLayer,durationMs,faithfulness,relevancy",
            rows);
    }
}
