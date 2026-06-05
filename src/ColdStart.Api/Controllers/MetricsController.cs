using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ColdStart.Api.Controllers;

/// <summary>
/// Runtime metrik paneli endpoint'i: arama kayıtları ve katman geçişleri.
/// İş mantığı <see cref="IMetricsQueryService"/>'tedir.
/// </summary>
public sealed class MetricsController : ApiController
{
    private readonly IMetricsQueryService _metricsService;

    /// <summary>DI üzerinden metrik okuma servisini alır.</summary>
    public MetricsController(IMetricsQueryService metricsService)
    {
        _metricsService = metricsService;
    }

    /// <summary>Son aramaların metrik kayıtlarını ve gözlenen katman geçişlerini döner.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(MetricsResponse), StatusCodes.Status200OK)]
    public IActionResult GetMetrics() => Ok(_metricsService.GetMetrics());
}
