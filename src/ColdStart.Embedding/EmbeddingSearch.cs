using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.Core.Services;
using ColdStart.Embedding.Similarity;

namespace ColdStart.Embedding;

/// <summary>
/// Orta veri yoğunluğu için hafif parametrik katman. OpenAI
/// <c>text-embedding-3-small</c> ile sorgu ve belge embedding'lerini üretir,
/// cosine similarity ile sıralar. LLM completion yapılmaz; cevap, göreli skor
/// eşiğini geçen top belgelerin alıntısıdır. Eksik embedding'ler lazy biçimde
/// bu katmanın ilk çağrısında batch olarak hesaplanır ve store'a geri yazılır.
/// </summary>
public sealed class EmbeddingSearch : ISearchLayer
{
    private static readonly SemaphoreSlim EmbeddingLock = new(1, 1);

    private readonly IDocumentStore _store;
    private readonly IEmbeddingService _embeddings;

    /// <summary>DI üzerinden bağımlılıkları alır.</summary>
    public EmbeddingSearch(IDocumentStore store, IEmbeddingService embeddings)
    {
        _store = store;
        _embeddings = embeddings;
    }

    /// <inheritdoc />
    public int LayerNumber => 2;

    /// <inheritdoc />
    public string LayerName => "Lightweight Embedding";

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

        Result ensure = await EnsureEmbeddingsAsync(documents, cancellationToken);
        if (ensure.IsFailure)
            return Result.Failure<SearchResponse>(ensure.Error);

        IReadOnlyList<Document> embedded = await _store.GetAllAsync(cancellationToken);
        Document[] withVectors = embedded.Where(d => d.Embedding is { Length: > 0 }).ToArray();
        if (withVectors.Length == 0)
            return EmptyResponse("Embedding üretilebilen belge bulunamadı.", embedded.Count);

        Result<float[]> queryEmbedding = await _embeddings.EmbedAsync(request.Query, cancellationToken);
        if (queryEmbedding.IsFailure)
            return Result.Failure<SearchResponse>(queryEmbedding.Error);

        int topK = Math.Clamp(request.TopK, 1, 50);
        var scored = withVectors
            .Select(d => new
            {
                Document = d,
                Score = CosineSimilarity.Compute(queryEmbedding.Value, d.Embedding!)
            })
            .OrderByDescending(s => s.Score)
            .Take(topK)
            .ToArray();

        IReadOnlyList<SearchSource> sources = scored
            .Select(s => new SearchSource
            {
                DocumentId = s.Document.Id,
                Snippet = ExtractiveAnswerComposer.MakeSnippet(s.Document.Content),
                Score = s.Score
            })
            .ToArray();

        return new SearchResponse
        {
            Answer = ExtractiveAnswerComposer.Compose(
                scored.Select(s => (s.Document.Content, s.Score)).ToArray()),
            ActiveLayer = LayerNumber,
            LayerName = LayerName,
            DocumentCount = embedded.Count,
            Sources = sources
        };
    }

    private async Task<Result> EnsureEmbeddingsAsync(
        IReadOnlyList<Document> documents,
        CancellationToken cancellationToken)
    {
        Document[] missing = documents.Where(d => d.Embedding is null || d.Embedding.Length == 0).ToArray();
        if (missing.Length == 0) return Result.Success();

        await EmbeddingLock.WaitAsync(cancellationToken);
        try
        {
            // İkinci kontrol: lock'a girene kadar başka bir istek embedding üretmiş olabilir.
            IReadOnlyList<Document> latest = await _store.GetAllAsync(cancellationToken);
            Document[] stillMissing = latest.Where(d => d.Embedding is null || d.Embedding.Length == 0).ToArray();
            if (stillMissing.Length == 0) return Result.Success();

            string[] texts = stillMissing.Select(d => d.Content).ToArray();
            Result<IReadOnlyList<float[]>> batch = await _embeddings.EmbedBatchAsync(texts, cancellationToken);
            if (batch.IsFailure)
                return Result.Failure(batch.Error);

            for (int i = 0; i < stillMissing.Length; i++)
            {
                Document updated = stillMissing[i] with { Embedding = batch.Value[i] };
                await _store.UpsertAsync(updated, cancellationToken);
            }
            return Result.Success();
        }
        finally
        {
            EmbeddingLock.Release();
        }
    }

    private static SearchResponse EmptyResponse(string message, int documentCount) => new()
    {
        Answer = message,
        ActiveLayer = 2,
        LayerName = "Lightweight Embedding",
        DocumentCount = documentCount,
        Sources = Array.Empty<SearchSource>()
    };

}
