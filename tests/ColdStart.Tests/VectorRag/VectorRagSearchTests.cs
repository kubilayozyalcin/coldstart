using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.Persistence.InMemory;
using ColdStart.Tests.Fakes;
using ColdStart.VectorRag.Chunking;
using ColdStart.VectorRag.Options;

namespace ColdStart.Tests.VectorRag;

public sealed class VectorRagSearchTests
{
    private static ColdStart.VectorRag.VectorRagSearch CreateLayer(
        InMemoryDocumentStore store,
        FakeVectorStore vectorStore,
        FakeAnswerGenerator generator) =>
        new(
            store,
            new FakeEmbeddingService(dimension: 16),
            vectorStore,
            new FixedSizeChunker(Microsoft.Extensions.Options.Options.Create(new VectorRagOptions
            {
                ChunkSize = 200,
                ChunkOverlap = 20
            })),
            generator);

    [Fact]
    public async Task Empty_store_returns_informative_answer_without_llm_call()
    {
        var generator = new FakeAnswerGenerator();
        var layer = CreateLayer(new InMemoryDocumentStore(), new FakeVectorStore(), generator);

        var result = await layer.SearchAsync(new SearchRequest { Query = "soru" });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Sources);
        Assert.Null(generator.LastQuery); // LLM hiç çağrılmamalı.
    }

    [Fact]
    public async Task Missing_documents_are_chunked_embedded_and_indexed_lazily()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "a", Content = "alpha alpha alpha" });
        await store.UpsertAsync(new Document { Id = "b", Content = "beta beta beta" });
        var vectorStore = new FakeVectorStore();
        var layer = CreateLayer(store, vectorStore, new FakeAnswerGenerator());

        var result = await layer.SearchAsync(new SearchRequest { Query = "alpha", TopK = 2 });

        Assert.True(result.IsSuccess);
        Assert.True(vectorStore.Ready);
        Assert.Equal(2, vectorStore.Chunks.Select(c => c.DocumentId).Distinct().Count());
        Assert.All(vectorStore.Chunks, c => Assert.NotNull(c.Embedding));
    }

    [Fact]
    public async Task Answer_comes_from_generator_with_retrieved_context()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "a", Content = "alpha alpha alpha" });
        var generator = new FakeAnswerGenerator();
        var layer = CreateLayer(store, new FakeVectorStore(), generator);

        var result = await layer.SearchAsync(new SearchRequest { Query = "alpha", TopK = 3 });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.ActiveLayer);
        Assert.StartsWith("Cevap(", result.Value.Answer);
        Assert.Equal("alpha", generator.LastQuery);
        Assert.NotEmpty(generator.LastContext);
        Assert.Single(result.Value.Sources);
        Assert.Equal("a", result.Value.Sources[0].DocumentId);
    }

    [Fact]
    public async Task Deleted_documents_are_removed_from_index_on_next_search()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "a", Content = "alpha alpha alpha" });
        await store.UpsertAsync(new Document { Id = "b", Content = "beta beta beta" });
        var vectorStore = new FakeVectorStore();
        var layer = CreateLayer(store, vectorStore, new FakeAnswerGenerator());

        await layer.SearchAsync(new SearchRequest { Query = "alpha" });
        await store.DeleteAsync("b");
        await layer.SearchAsync(new SearchRequest { Query = "alpha" });

        Assert.DoesNotContain(vectorStore.Chunks, c => c.DocumentId == "b");
    }

    [Fact]
    public async Task Updated_document_is_reindexed_with_new_content()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "a", Content = "eski içerik" });
        var vectorStore = new FakeVectorStore();
        var layer = CreateLayer(store, vectorStore, new FakeAnswerGenerator());

        await layer.SearchAsync(new SearchRequest { Query = "içerik" });
        await store.UpsertAsync(new Document { Id = "a", Content = "yeni içerik" });
        await layer.SearchAsync(new SearchRequest { Query = "içerik" });

        Assert.All(vectorStore.Chunks.Where(c => c.DocumentId == "a"),
            c => Assert.Equal("yeni içerik", c.Content));
    }

    [Fact]
    public async Task Generator_failure_propagates_as_result_error()
    {
        var store = new InMemoryDocumentStore();
        await store.UpsertAsync(new Document { Id = "a", Content = "alpha alpha alpha" });
        var generator = new FakeAnswerGenerator
        {
            FailWith = Error.External("LLM erişilemez.", code: "rag.generation_failed")
        };
        var layer = CreateLayer(store, new FakeVectorStore(), generator);

        var result = await layer.SearchAsync(new SearchRequest { Query = "alpha" });

        Assert.True(result.IsFailure);
        Assert.Equal("rag.generation_failed", result.Error.Code);
    }
}
