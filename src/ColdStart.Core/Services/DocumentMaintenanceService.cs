using ColdStart.Core.Abstractions;
using ColdStart.Core.Results;

namespace ColdStart.Core.Services;

/// <summary>
/// <see cref="IDocumentMaintenanceService"/> varsayılan uygulaması. Store'a
/// devreder, sonuçları <see cref="Result"/> sözleşmesine çevirir.
/// </summary>
public sealed class DocumentMaintenanceService : IDocumentMaintenanceService
{
    private readonly IDocumentStore _store;

    /// <summary>DI üzerinden store bağımlılığını alır.</summary>
    public DocumentMaintenanceService(IDocumentStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure(Error.Validation("Belge kimliği boş olamaz."));

        bool removed = await _store.DeleteAsync(id, cancellationToken);
        return removed
            ? Result.Success()
            : Result.Failure(Error.NotFound($"'{id}' kimlikli belge bulunamadı.", code: "document.not_found"));
    }

    /// <inheritdoc />
    public async Task<Result<int>> ClearAsync(CancellationToken cancellationToken = default)
    {
        int removed = await _store.ClearAsync(cancellationToken);
        return Result.Success(removed);
    }
}
