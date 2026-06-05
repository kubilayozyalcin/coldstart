using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Services;

/// <summary>
/// <see cref="IDocumentQueryService"/> varsayılan uygulaması. Store'dan
/// belgeleri okur, <see cref="DocumentSummary"/> projeksiyonuna çevirir.
/// </summary>
public sealed class DocumentQueryService : IDocumentQueryService
{
    private const int PreviewLength = 180;

    private readonly IDocumentStore _store;

    /// <summary>DI üzerinden store bağımlılığını alır.</summary>
    public DocumentQueryService(IDocumentStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<DocumentSummary>>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Document> documents = await _store.GetAllAsync(cancellationToken);

        IReadOnlyList<DocumentSummary> summaries = documents
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentSummary
            {
                Id = d.Id,
                ContentPreview = d.Content.Length <= PreviewLength
                    ? d.Content
                    : d.Content[..PreviewLength] + "...",
                ContentLength = d.Content.Length,
                HasEmbedding = d.Embedding is { Length: > 0 },
                Metadata = d.Metadata,
                CreatedAt = d.CreatedAt
            })
            .ToArray();

        return Result.Success(summaries);
    }
}
