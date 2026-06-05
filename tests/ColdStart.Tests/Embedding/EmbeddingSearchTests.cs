using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Embedding;
using ColdStart.Persistence.InMemory;
using ColdStart.Tests.Fakes;

namespace ColdStart.Tests.Layer2;

public sealed class EmbeddingSearchTests
{
    [Fact]
    public async Task Embeds_missing_documents_lazily_and_writes_back()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "a", Content = "alpha alpha alpha" });
        await store.UpsertAsync(new Document { Id = "b", Content = "beta beta beta" });

        var layer = new EmbeddingSearch(store, new FakeEmbeddingService(dimension: 16));

        var first = await layer.SearchAsync(new SearchRequest { Query = "alpha", TopK = 1 });
        Assert.True(first.IsSuccess);

        var docs = await store.GetAllAsync();
        Assert.All(docs, d => Assert.NotNull(d.Embedding));
    }

    [Fact]
    public async Task Returns_most_similar_document()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "alpha", Content = "alpha alpha alpha" });
        await store.UpsertAsync(new Document { Id = "beta", Content = "beta beta beta" });

        var layer = new EmbeddingSearch(store, new FakeEmbeddingService(dimension: 16));

        var result = await layer.SearchAsync(new SearchRequest { Query = "alpha alpha", TopK = 1 });

        Assert.True(result.IsSuccess);
        Assert.Equal("alpha", result.Value.Sources[0].DocumentId);
    }
}
