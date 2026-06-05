using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Belge yaşam döngüsünün yıkıcı operasyonları (silme, tümünü temizleme).
/// Ingest'ten ayrı tutulur; sorumluluk ayrımı için. Demo akışında jüri,
/// store'u temizleyip belgeleri tek tek ekleyerek katman geçişini canlı izler.
/// </summary>
public interface IDocumentMaintenanceService
{
    /// <summary>Verilen kimliğe sahip belgeyi siler. Bulunamazsa <see cref="ErrorType.NotFound"/> döner.</summary>
    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Store'daki tüm belgeleri siler. Silinen kayıt adedini taşıyan sonuç döner.</summary>
    Task<Result<int>> ClearAsync(CancellationToken cancellationToken = default);
}
