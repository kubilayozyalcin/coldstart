using ColdStart.Api.Hosting;
using ColdStart.Api.Services;
using ColdStart.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Api.Extensions;

/// <summary>
/// Uygulama başlangıç davranışlarını (hosted servisler) ve dosya tabanlı
/// yardımcı servisleri bağlayan uzantı.
/// </summary>
public static class HostingServiceCollectionExtensions
{
    /// <summary>
    /// Sentetik veri seed servisini ekler. Store boşsa ve dosya varsa
    /// belgeleri yükler.
    /// </summary>
    public static IServiceCollection AddDocumentSeed(this IServiceCollection services)
    {
        services.AddHostedService<DocumentSeedHostedService>();
        return services;
    }

    /// <summary>
    /// Offline batch deney sonuçlarını (data/results CSV'leri) UI'ya özetleyen
    /// servisi ekler.
    /// </summary>
    public static IServiceCollection AddExperimentResults(this IServiceCollection services)
    {
        services.AddSingleton<IExperimentResultsService, ExperimentResultsService>();
        return services;
    }
}
