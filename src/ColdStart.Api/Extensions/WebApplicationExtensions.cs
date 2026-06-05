using ColdStart.Api.Middleware;

namespace ColdStart.Api.Extensions;

/// <summary>
/// HTTP pipeline (middleware) sıralamasını barındıran uzantılar.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Sunum katmanının middleware sırasını kurar: önce global exception
    /// yakalayıcı, sonra Swagger (yalnızca development), ardından controller
    /// eşlemesi.
    /// </summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseRateLimiter();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ColdStart API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "ColdStart API — Adaptif Hibrit RAG";
            });
        }

        app.MapControllers();
        return app;
    }
}
