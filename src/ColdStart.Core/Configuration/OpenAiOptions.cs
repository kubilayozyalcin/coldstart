namespace ColdStart.Core.Configuration;

/// <summary>
/// OpenAI sağlayıcısına ait konfigürasyon. <c>OpenAi</c> bölümünden okunur.
/// API anahtarı kesinlikle kod içine gömülmez; environment variable
/// (<c>OPENAI_API_KEY</c>) veya user-secrets üzerinden geçilir.
/// Layer 2 (embedding) ve Layer 3 (chat completion) tarafından ortak kullanılır;
/// bu yüzden Core'da yaşar.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>Konfigürasyon bölümünün adı (<c>appsettings.json</c>).</summary>
    public const string SectionName = "OpenAi";

    /// <summary>OpenAI API anahtarı. <c>OPENAI_API_KEY</c> environment variable'ı tercih edilen kaynaktır.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Kullanılacak embedding modeli. Varsayılan: <c>text-embedding-3-small</c>.</summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>Kullanılacak completion modeli (Layer 3 cevap üretimi). Varsayılan: <c>gpt-4o-mini</c>.</summary>
    public string ChatModel { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Embedding boyutu. <c>text-embedding-3-small</c> için 1536; başka model
    /// kullanılırsa burada güncellenir.
    /// </summary>
    public int EmbeddingDimension { get; set; } = 1536;
}
