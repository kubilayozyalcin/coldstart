using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.VectorRag.Chunking;

namespace ColdStart.VectorRag;

/// <summary>
/// Layer 3 — tam vektörel RAG. Akış: (1) in-memory store ile Qdrant indeksi
/// lazy senkronize edilir (eksik/bayat belgeler chunk'lanıp embed edilir,
/// silinmiş belgeler indeksten düşürülür), (2) sorgu embed edilip Qdrant'ta
/// chunk granülaritesinde aranır, (3) retrieve edilen bağlamla GPT-4o-mini
/// cevap üretir. Senkronizasyon, Layer 2'deki lazy embedding pattern'inin
/// chunk granülaritesine genelleştirilmiş halidir.
/// </summary>
public sealed class VectorRagSearch : ISearchLayer
{
    private const int SnippetLength = 240;
    private static readonly SemaphoreSlim IndexLock = new(1, 1);

    private readonly IDocumentStore _store;
    private readonly IEmbeddingService _embeddings;
    private readonly IVectorStore _vectorStore;
    private readonly IDocumentChunker _chunker;
    private readonly IAnswerGenerator _answerGenerator;

    /// <summary>DI üzerinden bağımlılıkları alır.</summary>
    public VectorRagSearch(
        IDocumentStore store,
        IEmbeddingService embeddings,
        IVectorStore vectorStore,
        IDocumentChunker chunker,
        IAnswerGenerator answerGenerator)
    {
        _store = store;
        _embeddings = embeddings;
        _vectorStore = vectorStore;
        _chunker = chunker;
        _answerGenerator = answerGenerator;
    }

    /// <inheritdoc />
    public int LayerNumber => 3;

    /// <inheritdoc />
    public string LayerName => "Vector RAG (Qdrant + GPT-4o-mini)";

    /// <inheritdoc />
    public async Task<Result<SearchResponse>> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Error.Validation("Sorgu metni boş olamaz.");

        IReadOnlyList<Document> documents = await _store.GetAllAsync(cancellationToken);
        if (documents.Count == 0)
            return EmptyResponse("Sistemde henüz belge bulunmuyor.", documentCount: 0);

        Result sync = await EnsureIndexSyncedAsync(documents, cancellationToken);
        if (sync.IsFailure)
            return Result.Failure<SearchResponse>(sync.Error);

        Result<float[]> queryEmbedding = await _embeddings.EmbedAsync(request.Query, cancellationToken);
        if (queryEmbedding.IsFailure)
            return Result.Failure<SearchResponse>(queryEmbedding.Error);

        int topK = Math.Clamp(request.TopK, 1, 50);
        Result<IReadOnlyList<ChunkSearchHit>> hits =
            await _vectorStore.SearchAsync(queryEmbedding.Value, topK, cancellationToken);
        if (hits.IsFailure)
            return Result.Failure<SearchResponse>(hits.Error);

        if (hits.Value.Count == 0)
            return EmptyResponse("Sorguyla eşleşen içerik bulunamadı.", documents.Count);

        IReadOnlyList<DocumentChunk> contextChunks = hits.Value.Select(h => h.Chunk).ToArray();
        Result<string> answer = await _answerGenerator.GenerateAsync(request.Query, contextChunks, cancellationToken);
        if (answer.IsFailure)
            return Result.Failure<SearchResponse>(answer.Error);

        return new SearchResponse
        {
            Answer = answer.Value,
            ActiveLayer = LayerNumber,
            LayerName = LayerName,
            DocumentCount = documents.Count,
            Sources = BuildSources(hits.Value)
        };
    }

    /// <summary>
    /// Qdrant indeksini store ile tutarlı hale getirir: eksik veya içeriği
    /// değişmiş (hash uyuşmayan) belgeler chunk'lanıp embed edilerek upsert
    /// edilir; store'dan silinmiş belgeler indeksten düşürülür. Eşzamanlı
    /// isteklerde çifte indekslemeyi önlemek için kilit altında çalışır.
    /// </summary>
    private async Task<Result> EnsureIndexSyncedAsync(
        IReadOnlyList<Document> documents,
        CancellationToken cancellationToken)
    {
        await IndexLock.WaitAsync(cancellationToken);
        try
        {
            Result ready = await _vectorStore.EnsureReadyAsync(_embeddings.Dimension, cancellationToken);
            if (ready.IsFailure)
                return ready;

            Result<IReadOnlyDictionary<string, string>> indexed =
                await _vectorStore.GetIndexedDocumentsAsync(cancellationToken);
            if (indexed.IsFailure)
                return Result.Failure(indexed.Error);

            // Store'da olmayan belgeler indeksten temizlenir (silme senaryosu).
            var storeIds = documents.Select(d => d.Id).ToHashSet();
            foreach (string orphanId in indexed.Value.Keys.Where(id => !storeIds.Contains(id)))
            {
                Result delete = await _vectorStore.DeleteDocumentAsync(orphanId, cancellationToken);
                if (delete.IsFailure)
                    return delete;
            }

            // Eksik veya içeriği değişmiş belgeler yeniden indekslenir.
            Document[] stale = documents
                .Where(d => !indexed.Value.TryGetValue(d.Id, out string? hash)
                            || hash != ContentHasher.Compute(d.Content))
                .ToArray();

            foreach (Document document in stale)
            {
                Result reindex = await IndexDocumentAsync(document, cancellationToken);
                if (reindex.IsFailure)
                    return reindex;
            }

            return Result.Success();
        }
        finally
        {
            IndexLock.Release();
        }
    }

    /// <summary>Tek bir belgeyi chunk'lar, embedding'lerini batch üretir ve Qdrant'a yazar.</summary>
    private async Task<Result> IndexDocumentAsync(Document document, CancellationToken cancellationToken)
    {
        // Güncellenen belgenin eski chunk'ları kalmasın (chunk sayısı azalmış olabilir).
        Result cleanup = await _vectorStore.DeleteDocumentAsync(document.Id, cancellationToken);
        if (cleanup.IsFailure)
            return cleanup;

        IReadOnlyList<DocumentChunk> chunks = _chunker.Chunk(document);
        if (chunks.Count == 0)
            return Result.Success();

        string[] texts = chunks.Select(c => c.Content).ToArray();
        Result<IReadOnlyList<float[]>> embeddings = await _embeddings.EmbedBatchAsync(texts, cancellationToken);
        if (embeddings.IsFailure)
            return Result.Failure(embeddings.Error);

        DocumentChunk[] withVectors = chunks
            .Select((chunk, i) => chunk with { Embedding = embeddings.Value[i] })
            .ToArray();

        return await _vectorStore.UpsertChunksAsync(withVectors, cancellationToken);
    }

    /// <summary>
    /// Chunk isabetlerini belge bazında gruplar: her belge için en yüksek skorlu
    /// chunk'ın skoru ve snippet'i kaynak olarak döner.
    /// </summary>
    private static IReadOnlyList<SearchSource> BuildSources(IReadOnlyList<ChunkSearchHit> hits) => hits
        .GroupBy(h => h.Chunk.DocumentId)
        .Select(g => g.OrderByDescending(h => h.Score).First())
        .OrderByDescending(h => h.Score)
        .Select(h => new SearchSource
        {
            DocumentId = h.Chunk.DocumentId,
            Snippet = MakeSnippet(h.Chunk.Content),
            Score = h.Score
        })
        .ToArray();

    private SearchResponse EmptyResponse(string message, int documentCount) => new()
    {
        Answer = message,
        ActiveLayer = LayerNumber,
        LayerName = LayerName,
        DocumentCount = documentCount,
        Sources = Array.Empty<SearchSource>()
    };

    private static string MakeSnippet(string content) =>
        content.Length <= SnippetLength ? content : content[..SnippetLength] + "...";
}
