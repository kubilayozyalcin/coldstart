using ColdStart.Core.Extensions;
using ColdStart.Embedding.Extensions;
using ColdStart.Keyword.Extensions;
using ColdStart.Persistence.Extensions;
using ColdStart.VectorRag.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ColdStart.Experiments.Infrastructure;

/// <summary>
/// Deneylerin kullandığı DI host'u. API ile birebir aynı servis kayıtlarını
/// (aynı extension metotları) kullanır — deney, production pipeline'ının
/// kendisini ölçer, kopyasını değil. Eşikler deney başına override edilebilir.
/// </summary>
public static class ExperimentHost
{
    /// <summary>
    /// Verilen konfigürasyon override'larıyla yeni bir service provider kurar.
    /// Her deney koşusu izole bir provider alır (in-memory store sıfırdan başlar).
    /// </summary>
    public static ServiceProvider Build(IReadOnlyDictionary<string, string?>? overrides = null)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets(typeof(ExperimentHost).Assembly, optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(overrides ?? new Dictionary<string, string?>())
            .Build();

        ServiceCollection services = new();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services
            .AddColdStartCore(configuration)
            .AddPersistenceInMemory()
            .AddKeywordSearch(configuration)
            .AddEmbeddingSearch(configuration)
            .AddVectorRag(configuration);

        return services.BuildServiceProvider();
    }
}
