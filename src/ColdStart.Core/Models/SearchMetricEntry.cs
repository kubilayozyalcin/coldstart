namespace ColdStart.Core.Models;

/// <summary>
/// Tek bir arama isteğinin runtime metrik kaydı. Layer transition analizinin
/// ham verisidir: belge sayısı, aktif katman ve süre her aramada kaydedilir.
/// </summary>
public sealed record SearchMetricEntry
{
    /// <summary>Aramanın yapıldığı UTC zaman damgası.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Aramayı cevaplayan katmanın numarası (başarısız aramada 0).</summary>
    public int ActiveLayer { get; init; }

    /// <summary>Katmanın okunabilir adı.</summary>
    public required string LayerName { get; init; }

    /// <summary>Arama anındaki toplam belge sayısı.</summary>
    public int DocumentCount { get; init; }

    /// <summary>Aramanın süresi (milisaniye).</summary>
    public long DurationMs { get; init; }

    /// <summary>Arama başarılı sonuçlandı mı?</summary>
    public bool Success { get; init; }

    /// <summary>Başarısız aramada hata kodu; başarılıda <c>null</c>.</summary>
    public string? ErrorCode { get; init; }
}
