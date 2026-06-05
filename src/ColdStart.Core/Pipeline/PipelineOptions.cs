namespace ColdStart.Core.Pipeline;

/// <summary>
/// Pipeline geçiş eşiklerini taşıyan konfigürasyon nesnesi.
/// <c>appsettings.json</c> içindeki <c>Pipeline</c> bölümünden bağlanır.
/// </summary>
public sealed class PipelineOptions
{
    /// <summary>Konfigürasyon bölümünün adı.</summary>
    public const string SectionName = "Pipeline";

    /// <summary>Layer 1'den Layer 2'ye geçiş eşiği. <c>documentCount ≥ Layer2Threshold</c> olduğunda Layer 2 aktif olur.</summary>
    public int Layer2Threshold { get; set; } = 50;

    /// <summary>Layer 2'den Layer 3'e geçiş eşiği. <c>documentCount ≥ Layer3Threshold</c> olduğunda Layer 3 aktif olur.</summary>
    public int Layer3Threshold { get; set; } = 200;
}
