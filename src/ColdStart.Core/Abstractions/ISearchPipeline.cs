using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Adaptif RAG hattının dış cephesi. <c>documentCount</c>'a göre runtime'da
/// hangi <see cref="ISearchLayer"/>'ın kullanılacağını belirler ve isteği
/// uygun katmana yönlendirir. Durum sorgusu da bu arayüz üzerinden yapılır.
/// </summary>
public interface ISearchPipeline
{
    /// <summary>Sorguyu, mevcut belge sayısına göre uygun katmana yönlendirir.</summary>
    Task<Result<SearchResponse>> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>O anki aktif katmanı, belge sayısını ve bir sonraki eşiği döner.</summary>
    Task<StatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);
}
