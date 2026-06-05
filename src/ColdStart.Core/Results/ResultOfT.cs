namespace ColdStart.Core.Results;

/// <summary>
/// Bir değer taşıyan başarı/başarısızlık sonucu. Servisler iş çıktısını bu tip
/// üzerinden döner. <see cref="Value"/>'ya yalnızca <see cref="Result.IsSuccess"/>
/// doğruyken erişilmelidir; aksi halde <see cref="InvalidOperationException"/>
/// fırlatılır.
/// </summary>
/// <typeparam name="T">Sonuç değerinin tipi.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// Başarılı sonucun taşıdığı değer. Başarısız sonuçta erişim girişimi
    /// <see cref="InvalidOperationException"/> fırlatır.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Başarısız bir sonuç üzerinden Value alınamaz.");

    /// <summary>İç kurucu; <see cref="Result"/> fabrikaları üzerinden çağrılır.</summary>
    internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>Bir <typeparamref name="T"/> değerini örtük olarak başarılı sonuca dönüştürür.</summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>Bir <see cref="Error"/>'ı örtük olarak başarısız sonuca dönüştürür.</summary>
    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}
