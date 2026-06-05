using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ColdStart.Api.Middleware;

/// <summary>
/// İşlenmemiş tüm exception'ları yakalar, log'lar ve standart
/// <see cref="ProblemDetails"/> formatında 500 cevabı üretir. İş katmanı
/// <see cref="Core.Results.Result"/> döndüğü için buraya yalnızca beklenmedik
/// hatalar düşmelidir.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>Pipeline'daki bir sonraki adıma referans alır.</summary>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>HTTP isteğini bir sonraki middleware'e devreder; exception yakalar.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "İşlenmemiş hata yakalandı: {Message}", exception.Message);
            await WriteProblemAsync(context, exception);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "internal_error",
            Detail = "Beklenmedik bir hata oluştu. Lütfen tekrar deneyin.",
            Type = "https://coldstart.local/errors/internal_error",
            Extensions = { ["exceptionType"] = exception.GetType().Name }
        };

        context.Response.StatusCode = problem.Status!.Value;
        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(context.Response.Body, problem);
    }
}
