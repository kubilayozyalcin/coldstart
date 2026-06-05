using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ColdStart.Api.Controllers;

/// <summary>
/// Hattın durumunu raporlayan controller. Hangi katmanın aktif olduğunu,
/// kaç belge olduğunu ve bir sonraki eşiğe ne kadar kaldığını gösterir.
/// </summary>
public sealed class StatusController : ApiController
{
    private readonly ISearchPipeline _pipeline;

    /// <summary>DI üzerinden pipeline bağımlılığını alır.</summary>
    public StatusController(ISearchPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>O anki hat durumunu döner.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        StatusResponse status = await _pipeline.GetStatusAsync(cancellationToken);
        return Ok(status);
    }
}
