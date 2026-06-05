using ColdStart.Core.Abstractions;
using ColdStart.Keyword.BM25;
using ColdStart.Keyword.Tokenization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.Keyword.Extensions;

/// <summary>
/// Layer 1 (BM25) servis kayıtları.
/// </summary>
public static class KeywordServiceCollectionExtensions
{
    /// <summary>
    /// BM25 tabanlı keyword arama katmanını ve Türkçe tokenizer'ı kaydeder.
    /// </summary>
    public static IServiceCollection AddKeywordSearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BM25Parameters>(configuration.GetSection(BM25Parameters.SectionName));
        services.AddSingleton<TurkishTokenizer>();
        services.AddSingleton<ISearchLayer, BM25SearchLayer>();
        return services;
    }
}
