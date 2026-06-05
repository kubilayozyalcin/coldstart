using ColdStart.Core.Abstractions;
using ColdStart.Core.Pipeline;
using ColdStart.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Core.Extensions;

/// <summary>
/// Core katmanının servis kayıtlarını sunan <see cref="IServiceCollection"/>
/// uzantıları. <c>Program.cs</c> doğrudan bu metotları çağırır.
/// </summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Pipeline yönlendirici, ingest servisi, metrik altyapısı ve
    /// <see cref="PipelineOptions"/> bağlamasını kaydeder.
    /// <see cref="ISearchPipeline"/>, metrik kaydı için
    /// <see cref="MetricsRecordingPipeline"/> decorator'ı ile sarılır.
    /// </summary>
    public static IServiceCollection AddColdStartCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PipelineOptions>(configuration.GetSection(PipelineOptions.SectionName));

        services.AddScoped<PipelineRouter>();
        services.AddScoped<ISearchPipeline>(sp => new MetricsRecordingPipeline(
            sp.GetRequiredService<PipelineRouter>(),
            sp.GetRequiredService<ISearchMetricsRecorder>()));

        services.AddSingleton<ISearchMetricsRecorder, InMemorySearchMetricsRecorder>();
        services.AddSingleton<IMetricsQueryService, MetricsQueryService>();

        services.AddScoped<IDocumentIngestService, DocumentIngestService>();
        services.AddScoped<IDocumentMaintenanceService, DocumentMaintenanceService>();
        services.AddScoped<IDocumentQueryService, DocumentQueryService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        return services;
    }
}
