using System.Collections.Concurrent;
using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;

namespace ColdStart.Persistence.InMemory;

/// <summary>
/// <see cref="IDocumentStore"/>'un bellekte (in-memory) çalışan thread-safe uygulaması.
/// Faz 1 ve Faz 2 boyunca tek başına yeterlidir; Faz 3'te Qdrant ile birlikte
/// çalışır (bu store local cache, Qdrant ise dayanıklı backing store olur).
/// Restart sonrası veri kaybı söz konusudur — akademik prototip için kabul edilebilir.
/// </summary>
public sealed class InMemoryDocumentStore : IDocumentStore
{
    private readonly ConcurrentDictionary<string, Document> _store = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_store.Count);

    /// <inheritdoc />
    public Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Document> snapshot = _store.Values.ToArray();
        return Task.FromResult(snapshot);
    }

    /// <inheritdoc />
    public Task<Document?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var document);
        return Task.FromResult(document);
    }

    /// <inheritdoc />
    public Task UpsertAsync(Document document, CancellationToken cancellationToken = default)
    {
        _store[document.Id] = document;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryRemove(id, out _));

    /// <inheritdoc />
    public Task<int> ClearAsync(CancellationToken cancellationToken = default)
    {
        int removed = _store.Count;
        _store.Clear();
        return Task.FromResult(removed);
    }
}
