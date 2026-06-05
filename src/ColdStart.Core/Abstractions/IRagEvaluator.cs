using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Bir arama cevabını LLM-as-a-judge yaklaşımıyla puanlayan bileşenin
/// soyutlaması. Faithfulness ve answer relevancy metriklerini üretir;
/// uygulaması Layer 3 projesindedir (GPT-4o-mini hakem olarak kullanılır).
/// </summary>
public interface IRagEvaluator
{
    /// <summary>
    /// Verilen sorgu–cevap çiftini, cevabın dayandığı kaynak snippet'leriyle
    /// birlikte hakem modele gönderir ve kalite skorlarını döner.
    /// </summary>
    Task<Result<RagEvaluation>> EvaluateAsync(
        string query,
        string answer,
        IReadOnlyList<SearchSource> sources,
        CancellationToken cancellationToken = default);
}
