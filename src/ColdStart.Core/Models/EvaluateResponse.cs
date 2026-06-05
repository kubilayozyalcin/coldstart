namespace ColdStart.Core.Models;

/// <summary>
/// Değerlendirmeli arama cevabı: pipeline'ın ürettiği arama sonucu ile
/// hakem modelin kalite skorlarını birlikte taşır.
/// </summary>
public sealed class EvaluateResponse
{
    /// <summary>Pipeline'ın ürettiği arama sonucu (aktif katman, cevap, kaynaklar).</summary>
    public required SearchResponse Search { get; init; }

    /// <summary>LLM-as-a-judge skorları (faithfulness, answer relevancy).</summary>
    public required RagEvaluation Evaluation { get; init; }

    /// <summary>Aramanın süresi (milisaniye); değerlendirme süresi dahil değildir.</summary>
    public long SearchDurationMs { get; init; }
}
