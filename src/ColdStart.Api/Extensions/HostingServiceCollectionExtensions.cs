using ColdStart.Api.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Api.Extensions;

/// <summary>
/// Uygulama başlangıç davranışlarını (hosted servisler) bağlayan uzantı.
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
}
