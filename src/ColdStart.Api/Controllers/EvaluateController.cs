using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace ColdStart.Api.Controllers;

/// <summary>
/// Değerlendirmeli arama endpoint'i: sorgu pipeline'dan geçirilir, cevap
/// LLM-as-a-judge ile puanlanır. İş mantığı <see cref="IEvaluationService"/>'tedir.
/// </summary>
public sealed class EvaluateController : ApiController
{
    private readonly IEvaluationService _evaluationService;

    /// <summary>DI üzerinden değerlendirme servisini alır.</summary>
    public EvaluateController(IEvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    /// <summary>
    /// Sorguyu çalıştırır ve cevabı faithfulness / answer relevancy metrikleriyle puanlar.
    /// </summary>
    /// <param name="request">Değerlendirilecek sorgu ve top-K.</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    [HttpPost]
    [ProducesResponseType(typeof(EvaluateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Evaluate(
        [FromBody] EvaluateRequest request,
        CancellationToken cancellationToken)
    {
        Result<EvaluateResponse> result = await _evaluationService.EvaluateAsync(request, cancellationToken);
        return ToActionResult(result);
    }
}
