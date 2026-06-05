namespace ColdStart.Core.Entities;

/// <summary>
/// Bir belgenin vektör indeksine yazılan tek bir parçası (chunk). Layer 3'te
/// belgeler sabit boyutlu parçalara bölünür; retrieval chunk granülaritesinde
/// yapılır, cevap üretiminde chunk içerikleri bağlam olarak LLM'e verilir.
/// </summary>
public sealed record DocumentChunk
{
    /// <summary>
    /// Chunk'ın deterministik kimliği: <c>{DocumentId}#{Index}</c>. Aynı belge
    /// yeniden indekslendiğinde aynı kimlikler üretilir; Qdrant'ta upsert
    /// çakışma yaratmaz.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>Chunk'ın ait olduğu belgenin kimliği.</summary>
    public required string DocumentId { get; init; }

    /// <summary>Chunk'ın belge içindeki sıra numarası (0 tabanlı).</summary>
    public required int Index { get; init; }

    /// <summary>Chunk'ın metin içeriği.</summary>
    public required string Content { get; init; }

    /// <summary>
    /// Kaynak belgenin içerik özeti (hash). Belge güncellendiğinde hash değişir;
    /// vektör indeksindeki kayıt ile karşılaştırılarak yeniden indeksleme
    /// gerektiği anlaşılır.
    /// </summary>
    public required string ContentHash { get; init; }

    /// <summary>Chunk'ın vektörel temsili. İndekslenmeden önce hesaplanır; aksi halde <c>null</c>.</summary>
    public float[]? Embedding { get; init; }
}
