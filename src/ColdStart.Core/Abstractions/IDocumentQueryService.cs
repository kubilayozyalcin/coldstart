using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Belgeleri okumaya yönelik servis. Embedding vektörü gibi büyük alanları
/// dışarı vermez; demo arayüzü ve dış istemciler bu özet üzerinden listeleme yapar.
/// </summary>
public interface IDocumentQueryService
{
    /// <summary>Tüm belgelerin özetini, en yeniden en eskiye sıralayarak döner.</summary>
    Task<Result<IReadOnlyList<DocumentSummary>>> ListAsync(CancellationToken cancellationToken = default);
}
