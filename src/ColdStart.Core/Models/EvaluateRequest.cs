namespace ColdStart.Core.Models;

/// <summary>
/// Değerlendirmeli arama isteği: sorgu normal pipeline'dan geçirilir,
/// ardından cevap LLM-as-a-judge ile puanlanır.
/// </summary>
public sealed class EvaluateRequest
{
    /// <summary>Değerlendirilecek kullanıcı sorgusu.</summary>
    public required string Query { get; init; }

    /// <summary>Aramada dönecek en alakalı belge adedi. 1–50 arası.</summary>
    public int TopK { get; init; } = 5;
}
