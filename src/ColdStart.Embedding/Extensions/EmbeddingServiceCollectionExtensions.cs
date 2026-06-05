using ColdStart.Core.Abstractions;
using ColdStart.Core.Configuration;
using ColdStart.Embedding.OpenAi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Embedding.Extensions;

/// <summary>
/// Layer 2 (Lightweight Embedding) servis kayıtları.
/// </summary>
public static class EmbeddingServiceCollectionExtensions
{
    /// <summary>
    /// OpenAI ayarlarını bağlar, <see cref="IEmbeddingService"/> ve
    /// <see cref="EmbeddingSearch"/> kayıtlarını yapar.
    /// </summary>
    public static IServiceCollection AddEmbeddingSearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));

        // Environment variable override: OPENAI_API_KEY tek başına yeterli olsun.
        services.PostConfigure<OpenAiOptions>(options =>
        {
            string? envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
                options.ApiKey = envKey;
        });

        services.AddSingleton<IEmbeddingService, OpenAiEmbeddingService>();
        services.AddSingleton<ISearchLayer, EmbeddingSearch>();
        return services;
    }
}
