namespace ColdStart.Core.Models;

/// <summary>
/// Bir arama cevabının LLM-as-a-judge ile ölçülmüş kalite skorları.
/// RAGAS çerçevesindeki tanımlar esas alınır: faithfulness cevabın yalnızca
/// kaynaklara dayanma derecesi, answer relevancy cevabın soruyla alaka derecesidir.
/// </summary>
public sealed record RagEvaluation
{
    /// <summary>Cevap, kaynak belgelerle ne kadar tutarlı? 0 (tamamen uydurma) – 1 (tamamen kaynaklara dayalı).</summary>
    public double Faithfulness { get; init; }

    /// <summary>Cevap, sorulan soruyla ne kadar alakalı? 0 (alakasız) – 1 (tam isabet).</summary>
    public double AnswerRelevancy { get; init; }

    /// <summary>Hakem modelin skorlara dair kısa gerekçesi.</summary>
    public required string Rationale { get; init; }

    /// <summary>Değerlendirmeyi yapan hakem modelin adı (ör. <c>gpt-4o-mini</c>).</summary>
    public required string JudgeModel { get; init; }
}
