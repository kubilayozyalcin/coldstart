using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Değerlendirmeli arama akışının soyutlaması: sorguyu pipeline'dan geçirir,
/// ardından cevabı LLM-as-a-judge ile puanlar. Controller yalnızca bu servisi çağırır.
/// </summary>
public interface IEvaluationService
{
    /// <summary>Arama + değerlendirme akışını çalıştırır.</summary>
    Task<Result<EvaluateResponse>> EvaluateAsync(
        EvaluateRequest request,
        CancellationToken cancellationToken = default);
}
