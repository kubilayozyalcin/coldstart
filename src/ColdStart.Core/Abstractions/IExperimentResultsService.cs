using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Offline batch deney sonuçlarını (CSV) okuyup UI için özetleyen servis
/// soyutlaması. Sonuç dosyası bulunamazsa boş özet döner; bu bir hata değildir.
/// </summary>
public interface IExperimentResultsService
{
    /// <summary>En güncel activation deneyinin strateji bazlı özetini döner.</summary>
    Task<Result<ExperimentSummaryResponse>> GetActivationSummaryAsync(CancellationToken cancellationToken = default);
}
