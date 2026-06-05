using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace ColdStart.Api.Controllers;

/// <summary>
/// Adaptif RAG hattının dış cephesi. Gelen sorguyu <see cref="ISearchPipeline"/>'a
/// devreder; iş mantığı bu controller'da bulunmaz.
/// </summary>
public sealed class SearchController : ApiController
{
    private readonly ISearchPipeline _pipeline;

    /// <summary>DI üzerinden pipeline bağımlılığını alır.</summary>
    public SearchController(ISearchPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>
    /// Verilen sorguyu çalıştırır ve aktif katmana göre cevap üretir.
    /// </summary>
    /// <param name="request">Sorgu ve dönecek belge adedi.</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    [HttpPost]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Search(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        Result<SearchResponse> result = await _pipeline.SearchAsync(request, cancellationToken);
        return ToActionResult(result);
    }
}
