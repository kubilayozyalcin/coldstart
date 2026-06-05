using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Keyword.BM25;
using ColdStart.Keyword.Tokenization;
using ColdStart.Persistence.InMemory;
using Microsoft.Extensions.Options;

namespace ColdStart.Tests.Layer1;

public sealed class BM25SearchLayerTests
{
    [Fact]
    public async Task Returns_top_matching_document_for_keyword()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "agile", Content = "Çevik yazılım geliştirme sprint temposu üzerine kuruludur." });
        await store.UpsertAsync(new Document { Id = "ci", Content = "Sürekli entegrasyon ve sürekli teslimat hattı kaliteyi artırır." });
        await store.UpsertAsync(new Document { Id = "ddd", Content = "Domain Driven Design bounded context kavramını öne çıkarır." });

        var layer = new BM25SearchLayer(
            store,
            new TurkishTokenizer(),
            Options.Create(new BM25Parameters()));

        var result = await layer.SearchAsync(new SearchRequest { Query = "sprint çevik", TopK = 2 });

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Sources);
        Assert.Equal("agile", result.Value.Sources[0].DocumentId);
    }

    [Fact]
    public async Task Empty_store_returns_informative_message()
    {
        var layer = new BM25SearchLayer(
            new InMemoryDocumentStore(),
            new TurkishTokenizer(),
            Options.Create(new BM25Parameters()));

        var result = await layer.SearchAsync(new SearchRequest { Query = "test" });
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Sources);
        Assert.Equal(0, result.Value.DocumentCount);
    }

    [Fact]
    public async Task Empty_query_returns_validation_error()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "1", Content = "alpha beta" });

        var layer = new BM25SearchLayer(
            store,
            new TurkishTokenizer(),
            Options.Create(new BM25Parameters()));

        var result = await layer.SearchAsync(new SearchRequest { Query = "" });
        Assert.True(result.IsFailure);
    }
}
