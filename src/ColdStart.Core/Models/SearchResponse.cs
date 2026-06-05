namespace ColdStart.Core.Models;

/// <summary>
/// Arama hattının kullanıcıya döndürdüğü cevap. Layer 1 ve Layer 2 için
/// <see cref="Answer"/> "retrieve edilen belgelerin kısa özeti" olur;
/// Layer 3'te (Faz 3) LLM tarafından üretilmiş gerçek cevap olur.
/// </summary>
public sealed class SearchResponse
{
    /// <summary>Üretilen veya derlenen cevap metni.</summary>
    public required string Answer { get; init; }

    /// <summary>Cevabı üreten katmanın numarası (1, 2 veya 3).</summary>
    public int ActiveLayer { get; init; }

    /// <summary>Cevabı üreten katmanın okunabilir adı.</summary>
    public required string LayerName { get; init; }

    /// <summary>Sorgu anındaki toplam belge sayısı (geçiş eşiklerinin saydamlığı için).</summary>
    public int DocumentCount { get; init; }

    /// <summary>Cevabı destekleyen kaynak belgeler ve skorları.</summary>
    public IReadOnlyList<SearchSource> Sources { get; init; } = Array.Empty<SearchSource>();
}

/// <summary>
/// Bir cevabın dayandığı tek bir kaynak belgenin özet bilgisi.
/// </summary>
public sealed class SearchSource
{
    /// <summary>Belgenin kimliği.</summary>
    public required string DocumentId { get; init; }

    /// <summary>Belgenin gösterilebilir kısa snippet'i (ilk N karakter).</summary>
    public required string Snippet { get; init; }

    /// <summary>Katmana özgü skor (BM25 puanı veya cosine similarity). 0–1 normalize edilmemiştir.</summary>
    public double Score { get; init; }
}
