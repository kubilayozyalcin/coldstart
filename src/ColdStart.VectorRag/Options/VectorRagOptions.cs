namespace ColdStart.VectorRag.Options;

/// <summary>
/// Layer 3'ün chunking ve üretim davranışını belirleyen ayarlar.
/// <c>VectorRag</c> bölümünden okunur.
/// </summary>
public sealed class VectorRagOptions
{
    /// <summary>Konfigürasyon bölümünün adı (<c>appsettings.json</c>).</summary>
    public const string SectionName = "VectorRag";

    /// <summary>
    /// Bir chunk'ın hedef karakter uzunluğu. Sabit boyutlu chunking stratejisi
    /// kullanılır; kelime ortasından kesmemek için en yakın boşlukta kırpılır.
    /// </summary>
    public int ChunkSize { get; set; } = 800;

    /// <summary>
    /// Ardışık chunk'lar arasındaki örtüşme (karakter). Bağlamın chunk sınırında
    /// kopmasını yumuşatır; literatürde %10–15 örtüşme yaygındır.
    /// </summary>
    public int ChunkOverlap { get; set; } = 100;

    /// <summary>LLM cevabı için üst token sınırı (maliyet kontrolü).</summary>
    public int MaxAnswerTokens { get; set; } = 600;
}
