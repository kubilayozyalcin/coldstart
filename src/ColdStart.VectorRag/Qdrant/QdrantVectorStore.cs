using System.Security.Cryptography;
using System.Text;
using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.VectorRag.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace ColdStart.VectorRag.Qdrant;

/// <summary>
/// <see cref="IVectorStore"/>'un Qdrant uygulaması. gRPC client ile konuşur;
/// chunk'lar payload olarak belge kimliği, sıra, içerik ve içerik hash'i taşır.
/// Tüm dış çağrılar <see cref="Result"/>'a sarılır — Qdrant erişilemezse
/// pipeline'a exception değil açıklayıcı hata döner.
/// </summary>
public sealed class QdrantVectorStore : IVectorStore
{
    private const string DocumentIdField = "documentId";
    private const string ChunkIndexField = "chunkIndex";
    private const string ContentField = "content";
    private const string ContentHashField = "contentHash";

    private readonly QdrantOptions _options;
    private readonly ILogger<QdrantVectorStore> _logger;
    private readonly Lazy<QdrantClient> _client;

    /// <summary>DI üzerinden bağlantı ayarlarını alır. Client lazy kurulur — Qdrant kapalıyken uygulama yine de açılabilir; hata yalnızca Layer 3 fiilen kullanıldığında döner.</summary>
    public QdrantVectorStore(IOptions<QdrantOptions> options, ILogger<QdrantVectorStore> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new Lazy<QdrantClient>(() =>
            new QdrantClient(_options.Host, _options.GrpcPort, https: _options.UseTls));
    }

    /// <inheritdoc />
    public async Task<Result> EnsureReadyAsync(int dimension, CancellationToken cancellationToken = default)
    {
        try
        {
            bool exists = await _client.Value.CollectionExistsAsync(_options.CollectionName, cancellationToken);
            if (!exists)
            {
                await _client.Value.CreateCollectionAsync(
                    _options.CollectionName,
                    new VectorParams { Size = (ulong)dimension, Distance = Distance.Cosine },
                    cancellationToken: cancellationToken);
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant koleksiyonu hazırlanamadı.");
            return Result.Failure(Error.External($"Qdrant koleksiyonu hazırlanamadı: {ex.Message}", code: "qdrant.unavailable"));
        }
    }

    /// <inheritdoc />
    public async Task<Result> UpsertChunksAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        if (chunks.Count == 0)
            return Result.Success();

        if (chunks.Any(c => c.Embedding is null || c.Embedding.Length == 0))
            return Result.Failure(Error.Validation("Embedding'i hesaplanmamış chunk indekslenemez.", code: "qdrant.missing_embedding"));

        try
        {
            var points = chunks.Select(chunk => new PointStruct
            {
                Id = new PointId { Uuid = DeterministicGuid(chunk.Id) },
                Vectors = chunk.Embedding!,
                Payload =
                {
                    [DocumentIdField] = chunk.DocumentId,
                    [ChunkIndexField] = chunk.Index,
                    [ContentField] = chunk.Content,
                    [ContentHashField] = chunk.ContentHash
                }
            }).ToList();

            await _client.Value.UpsertAsync(_options.CollectionName, points, cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant upsert başarısız.");
            return Result.Failure(Error.External($"Qdrant upsert başarısız: {ex.Message}", code: "qdrant.upsert_failed"));
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ChunkSearchHit>>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<ScoredPoint> points = await _client.Value.SearchAsync(
                _options.CollectionName,
                queryEmbedding,
                limit: (ulong)Math.Clamp(topK, 1, 50),
                cancellationToken: cancellationToken);

            IReadOnlyList<ChunkSearchHit> hits = points
                .Select(p => new ChunkSearchHit
                {
                    Chunk = new DocumentChunk
                    {
                        Id = $"{p.Payload[DocumentIdField].StringValue}#{p.Payload[ChunkIndexField].IntegerValue}",
                        DocumentId = p.Payload[DocumentIdField].StringValue,
                        Index = (int)p.Payload[ChunkIndexField].IntegerValue,
                        Content = p.Payload[ContentField].StringValue,
                        ContentHash = p.Payload[ContentHashField].StringValue
                    },
                    Score = p.Score
                })
                .ToArray();

            return Result.Success(hits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant araması başarısız.");
            return Error.External($"Qdrant araması başarısız: {ex.Message}", code: "qdrant.search_failed");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyDictionary<string, string>>> GetIndexedDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = new Dictionary<string, string>();
            PointId? offset = null;

            do
            {
                ScrollResponse page = await _client.Value.ScrollAsync(
                    _options.CollectionName,
                    limit: 512,
                    offset: offset,
                    payloadSelector: new WithPayloadSelector
                    {
                        Include = new PayloadIncludeSelector { Fields = { DocumentIdField, ContentHashField } }
                    },
                    cancellationToken: cancellationToken);

                foreach (RetrievedPoint point in page.Result)
                    documents[point.Payload[DocumentIdField].StringValue] = point.Payload[ContentHashField].StringValue;

                offset = page.NextPageOffset;
            }
            while (offset is not null && offset.PointIdOptionsCase != PointId.PointIdOptionsOneofCase.None);

            return Result.Success<IReadOnlyDictionary<string, string>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant indeks listesi okunamadı.");
            return Error.External($"Qdrant indeks listesi okunamadı: {ex.Message}", code: "qdrant.scroll_failed");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.Value.DeleteAsync(
                _options.CollectionName,
                Conditions.MatchKeyword(DocumentIdField, documentId),
                cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant belge silme başarısız.");
            return Result.Failure(Error.External($"Qdrant belge silme başarısız: {ex.Message}", code: "qdrant.delete_failed"));
        }
    }

    /// <summary>
    /// Chunk kimliğinden deterministik UUID üretir. Qdrant point id'si UUID
    /// bekler; aynı chunk her indekslemede aynı UUID'yi alır, böylece upsert
    /// duplikasyon yaratmaz.
    /// </summary>
    private static string DeterministicGuid(string chunkId)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(chunkId));
        return new Guid(hash.AsSpan(0, 16)).ToString();
    }
}
