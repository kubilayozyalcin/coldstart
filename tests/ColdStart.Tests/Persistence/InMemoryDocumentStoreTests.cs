using ColdStart.Core.Entities;
using ColdStart.Persistence.InMemory;

namespace ColdStart.Tests.Persistence;

public sealed class InMemoryDocumentStoreTests
{
    [Fact]
    public async Task Upsert_increments_count()
    {
        InMemoryDocumentStore store = new();

        await store.UpsertAsync(new Document { Id = "1", Content = "alpha" });
        await store.UpsertAsync(new Document { Id = "2", Content = "beta" });

        Assert.Equal(2, await store.CountAsync());
    }

    [Fact]
    public async Task Upsert_with_same_id_replaces_document()
    {
        InMemoryDocumentStore store = new();
        await store.UpsertAsync(new Document { Id = "1", Content = "alpha" });
        await store.UpsertAsync(new Document { Id = "1", Content = "beta" });

        Document? document = await store.GetAsync("1");
        Assert.NotNull(document);
        Assert.Equal("beta", document!.Content);
        Assert.Equal(1, await store.CountAsync());
    }

    [Fact]
    public async Task Delete_returns_false_when_missing()
    {
        InMemoryDocumentStore store = new();
        bool deleted = await store.DeleteAsync("missing");
        Assert.False(deleted);
    }
}
