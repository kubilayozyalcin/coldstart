using ColdStart.Core.Abstractions;
using ColdStart.Persistence.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Persistence.Extensions;

/// <summary>
/// Persistence katmanının servis kayıtları.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// In-memory <see cref="IDocumentStore"/> uygulamasını singleton olarak kaydeder.
    /// </summary>
    public static IServiceCollection AddPersistenceInMemory(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentStore, InMemoryDocumentStore>();
        return services;
    }
}
