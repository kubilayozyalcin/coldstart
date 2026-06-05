using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Metin → vektör (embedding) dönüşümünün soyutlaması. Layer 2 ve Faz 3'teki
/// Layer 3 tarafından kullanılır. Faz 1 modunda hiçbir çağrı yapılmaz —
/// sadece kayıt tutulur. Implementasyon Layer 2.Lightweight projesindedir.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>Embedding üretiminde kullanılan modelin adı (ör. <c>text-embedding-3-small</c>).</summary>
    string ModelName { get; }

    /// <summary>Embedding vektörünün boyutu (text-embedding-3-small için 1536).</summary>
    int Dimension { get; }

    /// <summary>Tek bir metni embedding'e çevirir.</summary>
    Task<Result<float[]>> EmbedAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Birden fazla metni tek bir batch isteğinde embedding'e çevirir (maliyet/verim için tercih edilir).</summary>
    Task<Result<IReadOnlyList<float[]>>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);
}
