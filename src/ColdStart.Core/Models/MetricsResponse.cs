namespace ColdStart.Core.Models;

/// <summary>
/// Runtime metrik panelinin cevabı: ham arama kayıtları ve bunlardan türetilen
/// katman geçiş (layer transition) olayları.
/// </summary>
public sealed class MetricsResponse
{
    /// <summary>Son aramaların metrik kayıtları (eskiden yeniye sıralı).</summary>
    public required IReadOnlyList<SearchMetricEntry> Entries { get; init; }

    /// <summary>Ardışık başarılı aramalar arasında gözlenen katman geçişleri.</summary>
    public required IReadOnlyList<LayerTransition> Transitions { get; init; }
}

/// <summary>
/// İki ardışık arama arasında aktif katmanın değiştiği anın kaydı.
/// Layer Transition Accuracy analizinin gözlem birimidir.
/// </summary>
public sealed record LayerTransition
{
    /// <summary>Geçişin gözlendiği UTC zaman damgası.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Önceki aramayı cevaplayan katman.</summary>
    public int FromLayer { get; init; }

    /// <summary>Bu aramayı cevaplayan katman.</summary>
    public int ToLayer { get; init; }

    /// <summary>Geçiş anındaki belge sayısı (eşik doğrulaması için).</summary>
    public int DocumentCount { get; init; }
}
