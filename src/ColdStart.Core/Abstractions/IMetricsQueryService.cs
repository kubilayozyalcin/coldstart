using ColdStart.Core.Models;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Runtime metrik panelinin okuma soyutlaması: ham kayıtları ve türetilmiş
/// katman geçişlerini derler.
/// </summary>
public interface IMetricsQueryService
{
    /// <summary>Metrik kayıtlarını ve katman geçiş olaylarını döner.</summary>
    MetricsResponse GetMetrics();
}
