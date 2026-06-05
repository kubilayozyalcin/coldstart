using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Experiments.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Experiments.Experiments;

/// <summary>
/// Layer Transition Accuracy deneyi (LLM'siz, ücretsiz). Farklı eşik
/// konfigürasyonlarında, eşik sınırlarının etrafındaki belge sayıları için
/// router'ın seçtiği katman ile beklenen katman karşılaştırılır. Çıktı:
/// her örnekleme noktası için beklenen/gerçekleşen katman ve isabet.
/// </summary>
public static class TransitionAccuracyExperiment
{
    private static readonly (int Layer2, int Layer3)[] ThresholdConfigs =
    {
        (50, 200),  // üretim varsayılanı (appsettings.json)
        (5, 20),    // demo/dev eşikleri (appsettings.Development.json)
        (25, 100)   // sensitivity için ara konfigürasyon
    };

    /// <summary>Deneyi koşar ve CSV dosya yolunu döner.</summary>
    public static async Task<string> RunAsync(string repoRoot)
    {
        var rows = new List<string>();
        int total = 0, matches = 0;

        foreach ((int layer2, int layer3) in ThresholdConfigs)
        {
            // Eşik sınırlarının tam üstü/altı + uç noktalar: off-by-one hatalarını yakalar.
            int[] samplePoints =
            {
                0, 1, layer2 - 1, layer2, layer2 + 1,
                (layer2 + layer3) / 2, layer3 - 1, layer3, layer3 + 1, layer3 + 50
            };

            await using var provider = ExperimentHost.Build(new Dictionary<string, string?>
            {
                ["Pipeline:Layer2Threshold"] = layer2.ToString(),
                ["Pipeline:Layer3Threshold"] = layer3.ToString()
            });

            var store = provider.GetRequiredService<IDocumentStore>();
            var pipeline = provider.GetRequiredService<ISearchPipeline>();

            foreach (int count in samplePoints.Where(p => p >= 0).Distinct().OrderBy(p => p))
            {
                await store.ClearAsync();
                foreach (Document document in SyntheticCorpus.Generate(repoRoot, count))
                    await store.UpsertAsync(document);

                StatusResponse status = await pipeline.GetStatusAsync();
                int expected = count >= layer3 ? 3 : count >= layer2 ? 2 : 1;
                bool match = status.ActiveLayer == expected;

                total++;
                if (match) matches++;
                rows.Add($"{layer2}/{layer3},{count},{expected},{status.ActiveLayer},{(match ? 1 : 0)}");
            }
        }

        Console.WriteLine($"Transition accuracy: {matches}/{total} ({100.0 * matches / total:0.#}%)");
        return CsvWriter.Write(repoRoot, "transition-accuracy",
            "thresholds,documentCount,expectedLayer,actualLayer,match", rows);
    }
}
