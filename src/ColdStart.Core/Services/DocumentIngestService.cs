using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Services;

/// <summary>
/// <see cref="IDocumentIngestService"/>'in varsayılan uygulaması. Belgeyi
/// store'a yazar, durum bilgisini pipeline'dan okur ve yanıtı derler.
/// Embedding üretimi bu aşamada yapılmaz — Layer 2 ihtiyaç duyduğunda
/// lazy biçimde hesaplanır.
/// </summary>
public sealed class DocumentIngestService : IDocumentIngestService
{
    private readonly IDocumentStore _store;
    private readonly ISearchPipeline _pipeline;

    /// <summary>DI üzerinden bağımlılıkları alır.</summary>
    public DocumentIngestService(IDocumentStore store, ISearchPipeline pipeline)
    {
        _store = store;
        _pipeline = pipeline;
    }

    /// <summary>İçerik için üst sınır karakter sayısı; bu sınırı aşan ingest istekleri reddedilir.</summary>
    public const int MaxContentLength = 10_000;

    /// <inheritdoc />
    public async Task<Result<IngestResponse>> IngestAsync(
        IngestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return Error.Validation("Belge içeriği boş olamaz.");

        if (request.Content.Length > MaxContentLength)
            return Error.Validation(
                $"Belge içeriği {MaxContentLength:N0} karakteri aşamaz (gelen: {request.Content.Length:N0}).",
                code: "document.content_too_long");

        string id = string.IsNullOrWhiteSpace(request.Id)
            ? Guid.NewGuid().ToString("N")
            : request.Id!.Trim();

        Document document = new()
        {
            Id = id,
            Content = request.Content.Trim(),
            Metadata = request.Metadata
        };

        await _store.UpsertAsync(document, cancellationToken);
        StatusResponse status = await _pipeline.GetStatusAsync(cancellationToken);

        return new IngestResponse
        {
            DocumentId = id,
            Success = true,
            DocumentCount = status.DocumentCount,
            ActiveLayer = status.ActiveLayer
        };
    }
}
