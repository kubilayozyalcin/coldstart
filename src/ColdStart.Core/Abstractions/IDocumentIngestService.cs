using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Yeni belgelerin sisteme alınmasını yöneten servis. Controller doğrudan
/// store ile konuşmaz; bu soyutlama üzerinden geçer. Embedding üretimi şu
/// anda lazy (Layer 2 talep edildiğinde) yapılır; ingest yalnızca persist eder.
/// </summary>
public interface IDocumentIngestService
{
    /// <summary>Bir belgeyi store'a ekler (ya da aynı kimlikle var olanı günceller).</summary>
    Task<Result<IngestResponse>> IngestAsync(
        IngestRequest request,
        CancellationToken cancellationToken = default);
}
