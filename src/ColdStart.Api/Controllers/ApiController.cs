using ColdStart.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace ColdStart.Api.Controllers;

/// <summary>
/// Projedeki tüm controller'ların türediği temel sınıf. Ortak yönlendirme
/// (<c>api/[controller]</c>), JSON içerik türü ve <see cref="Result"/> →
/// <see cref="IActionResult"/> çevrim davranışı buradan gelir. Hiçbir
/// concrete controller doğrudan <see cref="ControllerBase"/>'ten türemez.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Bir <see cref="Result{T}"/>'i HTTP cevabına çevirir. Başarıyla biten istek
    /// <c>200 OK</c> ile değeri döner; başarısız sonuç hata tipine göre uygun
    /// HTTP koduyla <see cref="ProblemDetails"/> üretir.
    /// </summary>
    protected IActionResult ToActionResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : BuildProblem(result.Error);

    /// <summary>
    /// Bir <see cref="Result"/>'i HTTP cevabına çevirir (komut türü işlemler için).
    /// Başarıyla biten istek <c>204 No Content</c> döner.
    /// </summary>
    protected IActionResult ToActionResult(Result result)
        => result.IsSuccess ? NoContent() : BuildProblem(result.Error);

    private IActionResult BuildProblem(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.External => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetails problem = new()
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message,
            Type = $"https://coldstart.local/errors/{error.Code}"
        };
        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
