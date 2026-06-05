using System.Net.Sockets;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Persistence.InMemory;
using ColdStart.Tests.Fakes;
using ColdStart.VectorRag.Chunking;
using ColdStart.VectorRag.Options;
using ColdStart.VectorRag.Qdrant;
using Microsoft.Extensions.Logging.Abstractions;
using Qdrant.Client;

namespace ColdStart.Tests.VectorRag;

/// <summary>
/// Canlı Qdrant container'ına karşı koşan integration testleri. OpenAI çağrısı
/// yapılmaz (fake embedding + fake generator); yalnızca Qdrant CRUD ve Layer 3
/// akışı gerçek vektör DB ile doğrulanır. Qdrant erişilebilir değilse testler
/// skip edilir — CI'da container şartı dayatılmaz (<c>docker compose up -d</c>).
/// </summary>
public sealed class QdrantIntegrationTests : IAsyncLifetime
{
    private const string TestCollection = "coldstart_chunks_itest";
    private const int Dimension = 16;

    private static readonly bool QdrantAvailable = ProbeQdrant();

    private readonly QdrantVectorStore _store;

    public QdrantIntegrationTests()
    {
        _store = new QdrantVectorStore(
            Microsoft.Extensions.Options.Options.Create(new QdrantOptions { CollectionName = TestCollection }),
            NullLogger<QdrantVectorStore>.Instance);
    }

    /// <summary>Her test sınıfı örneği temiz koleksiyonla başlar.</summary>
    public async Task InitializeAsync()
    {
        if (!QdrantAvailable) return;
        await DropTestCollectionAsync();
    }

    /// <summary>Test koleksiyonu geride bırakılmaz.</summary>
    public async Task DisposeAsync()
    {
        if (!QdrantAvailable) return;
        await DropTestCollectionAsync();
    }

    [SkippableFact]
    public async Task Upsert_and_search_roundtrip_preserves_payload()
    {
        Skip.IfNot(QdrantAvailable, "Qdrant erişilebilir değil (docker compose up -d).");

        var ready = await _store.EnsureReadyAsync(Dimension);
        Assert.True(ready.IsSuccess, ready.IsFailure ? ready.Error.Message : null);

        var chunkA = MakeChunk("doc-a", 0, "alpha içerik", VectorOf(1f));
        var chunkB = MakeChunk("doc-b", 0, "beta içerik", VectorOf(-1f));
        var upsert = await _store.UpsertChunksAsync(new[] { chunkA, chunkB });
        Assert.True(upsert.IsSuccess, upsert.IsFailure ? upsert.Error.Message : null);

        var hits = await _store.SearchAsync(VectorOf(1f), topK: 1);
        Assert.True(hits.IsSuccess);
        var hit = Assert.Single(hits.Value);
        Assert.Equal("doc-a", hit.Chunk.DocumentId);
        Assert.Equal("alpha içerik", hit.Chunk.Content);
        Assert.Equal(chunkA.ContentHash, hit.Chunk.ContentHash);
        Assert.True(hit.Score > 0.99, $"Cosine skoru ~1 beklenirdi, gelen: {hit.Score}");
    }

    [SkippableFact]
    public async Task Indexed_documents_are_listed_and_deletable_per_document()
    {
        Skip.IfNot(QdrantAvailable, "Qdrant erişilebilir değil (docker compose up -d).");

        await _store.EnsureReadyAsync(Dimension);
        await _store.UpsertChunksAsync(new[]
        {
            MakeChunk("doc-a", 0, "a0", VectorOf(1f)),
            MakeChunk("doc-a", 1, "a1", VectorOf(0.5f)),
            MakeChunk("doc-b", 0, "b0", VectorOf(-1f))
        });

        var indexed = await _store.GetIndexedDocumentsAsync();
        Assert.True(indexed.IsSuccess);
        Assert.Equal(new[] { "doc-a", "doc-b" }, indexed.Value.Keys.OrderBy(k => k));

        var delete = await _store.DeleteDocumentAsync("doc-a");
        Assert.True(delete.IsSuccess);

        var after = await _store.GetIndexedDocumentsAsync();
        Assert.True(after.IsSuccess);
        Assert.Equal(new[] { "doc-b" }, after.Value.Keys.ToArray());
    }

    [SkippableFact]
    public async Task VectorRagSearch_end_to_end_against_live_qdrant()
    {
        Skip.IfNot(QdrantAvailable, "Qdrant erişilebilir değil (docker compose up -d).");

        var documentStore = new InMemoryDocumentStore();
        await documentStore.UpsertAsync(new Document { Id = "a", Content = "alpha alpha alpha" });
        await documentStore.UpsertAsync(new Document { Id = "b", Content = "beta beta beta" });

        var generator = new FakeAnswerGenerator();
        var layer = new ColdStart.VectorRag.VectorRagSearch(
            documentStore,
            new FakeEmbeddingService(Dimension),
            _store,
            new FixedSizeChunker(Microsoft.Extensions.Options.Options.Create(new VectorRagOptions())),
            generator);

        var first = await layer.SearchAsync(new SearchRequest { Query = "alpha", TopK = 1 });
        Assert.True(first.IsSuccess, first.IsFailure ? first.Error.Message : null);
        Assert.Equal(3, first.Value.ActiveLayer);
        Assert.Equal("a", first.Value.Sources[0].DocumentId);
        Assert.NotEmpty(generator.LastContext);

        // Belge güncellenince content-hash uyuşmaz → canlı Qdrant'ta yeniden indekslenir.
        await documentStore.UpsertAsync(new Document { Id = "a", Content = "gamma gamma gamma" });
        var second = await layer.SearchAsync(new SearchRequest { Query = "gamma", TopK = 1 });
        Assert.True(second.IsSuccess);
        Assert.Contains("gamma", second.Value.Sources[0].Snippet);
    }

    private static DocumentChunk MakeChunk(string documentId, int index, string content, float[] embedding) => new()
    {
        Id = $"{documentId}#{index}",
        DocumentId = documentId,
        Index = index,
        Content = content,
        ContentHash = ContentHasher.Compute(content),
        Embedding = embedding
    };

    /// <summary>İlk bileşeni işaretli, kalanı sabit dolgu olan basit test vektörü üretir.</summary>
    private static float[] VectorOf(float head)
    {
        float[] vector = new float[Dimension];
        vector[0] = head;
        for (int i = 1; i < Dimension; i++) vector[i] = 0.01f;
        return vector;
    }

    private static async Task DropTestCollectionAsync()
    {
        var client = new QdrantClient("localhost", 6334);
        if (await client.CollectionExistsAsync(TestCollection))
            await client.DeleteCollectionAsync(TestCollection);
    }

    /// <summary>Qdrant gRPC portuna kısa timeout'lu TCP probe atar; container kapalıysa testler skip edilir.</summary>
    private static bool ProbeQdrant()
    {
        try
        {
            using var tcp = new TcpClient();
            return tcp.ConnectAsync("localhost", 6334).Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            return false;
        }
    }
}
