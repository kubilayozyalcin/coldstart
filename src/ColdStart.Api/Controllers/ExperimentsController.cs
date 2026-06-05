using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ColdStart.Api.Controllers;

/// <summary>
/// Offline batch deney sonuçlarının UI'ya servis edildiği endpoint. İş mantığı
/// <see cref="IExperimentResultsService"/>'tedir.
/// </summary>
public sealed class ExperimentsController : ApiController
{
    private readonly IExperimentResultsService _experiments;

    /// <summary>DI üzerinden deney özet servisini alır.</summary>
    public ExperimentsController(IExperimentResultsService experiments)
    {
        _experiments = experiments;
    }

    /// <summary>
    /// En güncel activation deneyinin strateji × corpus boyutu bazlı özetini döner.
    /// Deney hiç koşulmamışsa boş nokta listesi döner.
    /// </summary>
    [HttpGet("activation")]
    [ProducesResponseType(typeof(ExperimentSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivation(CancellationToken cancellationToken)
        => ToActionResult(await _experiments.GetActivationSummaryAsync(cancellationToken));
}
