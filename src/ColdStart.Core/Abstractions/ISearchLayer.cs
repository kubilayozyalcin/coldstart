using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Adaptif RAG hattındaki bir arama katmanının soyutlaması. Üç katman da
/// (BM25, Lightweight Embedding, Vector RAG) bu aynı kontratı uygular;
/// böylece <see cref="ISearchPipeline"/> katmanlar arası şeffaf biçimde geçiş yapar.
/// </summary>
public interface ISearchLayer
{
    /// <summary>Katmanın hat içindeki sıra numarası (1, 2 veya 3).</summary>
    int LayerNumber { get; }

    /// <summary>Katmanın açıklayıcı insan-okur adı (örn. "Keyword (BM25)").</summary>
    string LayerName { get; }

    /// <summary>Verilen sorguyu bu katmanın stratejisiyle çalıştırır.</summary>
    Task<Result<SearchResponse>> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default);
}
