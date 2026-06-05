namespace ColdStart.Core.Results;

/// <summary>
/// Bir işlemin sonucunu — başarı/başarısızlık + hata bilgisini — taşıyan temel sınıf.
/// İş katmanı exception fırlatmak yerine bu nesneyi döner. Sunum katmanı
/// <c>ToActionResult()</c> uzantısı ile HTTP cevabına çevirir.
/// </summary>
public class Result
{
    /// <summary>İşlemin başarılı tamamlanıp tamamlanmadığını belirtir.</summary>
    public bool IsSuccess { get; }

    /// <summary><see cref="IsSuccess"/> değerinin tersi; kod okunabilirliği için.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Başarısızlık durumunda hatayı, başarı durumunda <see cref="Error.None"/>'u döner.</summary>
    public Error Error { get; }

    /// <summary>
    /// Türev sınıfların kullanması için korumalı kurucu. Geçersiz kombinasyonlarda
    /// <see cref="InvalidOperationException"/> fırlatır.
    /// </summary>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Başarılı sonuç üretiminde hata nesnesi None olmalıdır.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Başarısız sonuç üretiminde hata nesnesi None olamaz.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Başarılı, değersiz bir sonuç üretir (komut türü işlemler için).</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Verilen <paramref name="error"/> ile başarısız bir sonuç üretir.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Verilen değerle başarılı bir <see cref="Result{T}"/> üretir.</summary>
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);

    /// <summary>Verilen hata ile başarısız bir <see cref="Result{T}"/> üretir.</summary>
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}
