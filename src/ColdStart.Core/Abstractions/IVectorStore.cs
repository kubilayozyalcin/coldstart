using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Vektör veritabanının soyutlaması. Layer 3'te Qdrant uygulaması kullanılır;
/// testlerde in-memory fake ile değiştirilir. Chunk granülaritesinde çalışır:
/// belgeler <see cref="DocumentChunk"/>'lara bölünmüş halde yazılır ve aranır.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Koleksiyonun var olduğundan ve verilen vektör boyutuyla yapılandırıldığından
    /// emin olur; yoksa oluşturur. İlk indekslemeden önce çağrılır.
    /// </summary>
    Task<Result> EnsureReadyAsync(int dimension, CancellationToken cancellationToken = default);

    /// <summary>Embedding'i hesaplanmış chunk'ları ekler ya da aynı kimlikle değiştirir.</summary>
    Task<Result> UpsertChunksAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default);

    /// <summary>Sorgu vektörüne en benzer <paramref name="topK"/> chunk'ı döner.</summary>
    Task<Result<IReadOnlyList<ChunkSearchHit>>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// İndeksteki belge kimliklerini ve her belgenin indekslenme anındaki içerik
    /// hash'ini döner. Store ile karşılaştırılarak eksik / bayat / silinmiş
    /// belgeler tespit edilir (lazy senkronizasyon).
    /// </summary>
    Task<Result<IReadOnlyDictionary<string, string>>> GetIndexedDocumentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Verilen belgeye ait tüm chunk'ları indeksten siler.</summary>
    Task<Result> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);
}
