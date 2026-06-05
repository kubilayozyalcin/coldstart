namespace ColdStart.Core.Results;

/// <summary>
/// İş katmanından dönen hata bilgisini taşıyan değer nesnesi. Exception fırlatmak
/// yerine <see cref="Result"/> üzerinden bu nesneyi taşır; sunum katmanı bunu
/// kullanıcıya uygun HTTP cevabına dönüştürür.
/// </summary>
/// <param name="Code">Programatik karşılaştırma için kısa kod (örn. <c>document.not_found</c>).</param>
/// <param name="Message">Kullanıcıya gösterilebilir açıklama metni (Türkçe).</param>
/// <param name="Type">Hatanın türü; HTTP kodu çevrimi bu değere göre yapılır.</param>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    /// <summary>
    /// Hatasız durumu temsil eden sabit nesne. <see cref="Result.Success"/> içinde kullanılır.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>Genel iş hatası üretir.</summary>
    public static Error Failure(string message, string code = "failure")
        => new(code, message, ErrorType.Failure);

    /// <summary>Kaynak bulunamadı hatası üretir.</summary>
    public static Error NotFound(string message, string code = "not_found")
        => new(code, message, ErrorType.NotFound);

    /// <summary>Doğrulama hatası üretir.</summary>
    public static Error Validation(string message, string code = "validation")
        => new(code, message, ErrorType.Validation);

    /// <summary>Yetkilendirme hatası üretir.</summary>
    public static Error Unauthorized(string message, string code = "unauthorized")
        => new(code, message, ErrorType.Unauthorized);

    /// <summary>Çakışma hatası üretir.</summary>
    public static Error Conflict(string message, string code = "conflict")
        => new(code, message, ErrorType.Conflict);

    /// <summary>Dış servis (OpenAI, Qdrant vb.) kaynaklı hata üretir.</summary>
    public static Error External(string message, string code = "external")
        => new(code, message, ErrorType.External);
}
