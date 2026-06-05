namespace ColdStart.Core.Results;

/// <summary>
/// Domain düzeyinde oluşan hataları kategorize eden enum. HTTP cevap kodu çevrimi
/// <c>ResultExtensions</c> üzerinden bu değere göre yapılır.
/// </summary>
public enum ErrorType
{
    /// <summary>Hata bulunmaması durumu (başarılı sonuçlar için).</summary>
    None = 0,

    /// <summary>Genel iş kuralı ihlali ya da beklenmedik durum (HTTP 500 / 400 — bağlama göre).</summary>
    Failure = 1,

    /// <summary>İstenen kaynak mevcut değil (HTTP 404).</summary>
    NotFound = 2,

    /// <summary>Doğrulama hatası: gelen istek geçersiz alan içeriyor (HTTP 400).</summary>
    Validation = 3,

    /// <summary>Yetkilendirme hatası: kimlik doğrulanmış değil ya da yetki yok (HTTP 401 / 403).</summary>
    Unauthorized = 4,

    /// <summary>Çakışma: mevcut durumla istek arasında çelişki (HTTP 409).</summary>
    Conflict = 5,

    /// <summary>Dış servis hatası: OpenAI, Qdrant gibi bağımlı bir bileşen başarısız oldu (HTTP 502).</summary>
    External = 6
}
