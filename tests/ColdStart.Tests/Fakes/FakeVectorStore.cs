using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.Embedding.Similarity;

namespace ColdStart.Tests.Fakes;

/// <summary>
/// Test amaçlı in-memory <see cref="IVectorStore"/>. Qdrant'a gitmez;
/// chunk'ları bellekte tutar ve cosine similarity ile arar. Layer 3'ün
/// senkronizasyon ve retrieval davranışlarını ağ bağımlılığı olmadan
/// doğrulamak için kullanılır.
/// </summary>
public sealed class FakeVectorStore : IVectorStore
{
    private readonly Dictionary<string, DocumentChunk> _chunks = new();

    /// <summary>EnsureReadyAsync'in çağrılıp çağrılmadığı (koleksiyon hazırlık adımı).</summary>
    public bool Ready { get; private set; }

    /// <summary>İndeksteki tüm chunk'ların kopyası (assert'ler için).</summary>
    public IReadOnlyList<DocumentChunk> Chunks => _chunks.Values.ToArray();

    public Task<Result> EnsureReadyAsync(int dimension, CancellationToken cancellationToken = default)
    {
        Ready = true;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> UpsertChunksAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        foreach (DocumentChunk chunk in chunks)
            _chunks[chunk.Id] = chunk;
        return Task.FromResult(Result.Success());
    }

    public Task<Result<IReadOnlyList<ChunkSearchHit>>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ChunkSearchHit> hits = _chunks.Values
            .Where(c => c.Embedding is { Length: > 0 })
            .Select(c => new ChunkSearchHit
            {
                Chunk = c,
                Score = CosineSimilarity.Compute(queryEmbedding, c.Embedding!)
            })
            .OrderByDescending(h => h.Score)
            .Take(topK)
            .ToArray();

        return Task.FromResult(Result.Success(hits));
    }

    public Task<Result<IReadOnlyDictionary<string, string>>> GetIndexedDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<string, string> documents = _chunks.Values
            .GroupBy(c => c.DocumentId)
            .ToDictionary(g => g.Key, g => g.First().ContentHash);

        return Task.FromResult(Result.Success(documents));
    }

    public Task<Result> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        foreach (string key in _chunks.Where(kv => kv.Value.DocumentId == documentId).Select(kv => kv.Key).ToArray())
            _chunks.Remove(key);
        return Task.FromResult(Result.Success());
    }
}
