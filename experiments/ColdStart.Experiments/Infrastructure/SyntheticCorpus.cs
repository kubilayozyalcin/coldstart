using System.Text.Json;
using ColdStart.Core.Entities;

namespace ColdStart.Experiments.Infrastructure;

/// <summary>
/// Deney corpus'u üreticisi. Çekirdek, <c>data/synthetic/documents_seed.json</c>'daki
/// 15 el yazımı belgedir; daha büyük corpus gerektiğinde konu şablonlarından
/// deterministik filler belgeler türetilir (sabit seed — her koşuda aynı corpus,
/// reproducibility şartı).
/// </summary>
public static class SyntheticCorpus
{
    private static readonly string[] Topics =
    {
        "kod inceleme süreçleri ve pull request disiplini",
        "birim test ve test piramidi stratejisi",
        "sürüm yönetimi ve semantic versioning",
        "gözlemlenebilirlik: log, metrik ve trace üçlüsü",
        "bulut maliyet optimizasyonu ve kaynak etiketleme",
        "API tasarımında geriye dönük uyumluluk",
        "veri tabanı indeksleme ve sorgu planı analizi",
        "önbellekleme katmanları ve cache invalidation",
        "kimlik doğrulama ve OAuth 2.0 akışları",
        "konteyner orkestrasyonu ve sağlık kontrolleri",
        "olay güdümlü mimari ve mesaj kuyrukları",
        "statik kod analizi ve teknik kalite kapıları"
    };

    /// <summary>Seed dosyasındaki el yazımı belgeleri okur.</summary>
    public static IReadOnlyList<Document> LoadSeed(string repoRoot)
    {
        string path = Path.Combine(repoRoot, "data", "synthetic", "documents_seed.json");
        using FileStream stream = File.OpenRead(path);
        List<SeedDocument> raw = JsonSerializer.Deserialize<List<SeedDocument>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        return raw
            .Select(d => new Document { Id = d.Id, Content = d.Content, Metadata = d.Metadata })
            .ToArray();
    }

    /// <summary>
    /// Seed + deterministik filler ile istenen boyutta corpus üretir.
    /// Filler içerikleri konu şablonlarının varyasyonlarıdır; aynı <paramref name="size"/>
    /// her zaman aynı belgeleri verir.
    /// </summary>
    public static IReadOnlyList<Document> Generate(string repoRoot, int size)
    {
        var documents = new List<Document>(LoadSeed(repoRoot));
        int fillerIndex = 0;
        while (documents.Count < size)
        {
            string topic = Topics[fillerIndex % Topics.Length];
            int variant = fillerIndex / Topics.Length + 1;
            documents.Add(new Document
            {
                Id = $"synth-{fillerIndex + 1:D3}",
                Content = $"Bu belge {topic} konusunu ele alır (varyant {variant}). " +
                          $"Modern yazılım ekipleri bu pratiği sürdürülebilir teslimatın ön koşulu olarak görür; " +
                          $"ölçüm yapılmadan iyileştirme yapılamaz ilkesi burada da geçerlidir. " +
                          $"Varyant {variant}, konunun farklı bir bağlamda tekrarını temsil eder.",
                Metadata = new Dictionary<string, string> { ["category"] = "synthetic", ["source"] = "generator" }
            });
            fillerIndex++;
        }
        return documents.Take(size).ToArray();
    }

    private sealed record SeedDocument(string Id, string Content, Dictionary<string, string>? Metadata);
}
