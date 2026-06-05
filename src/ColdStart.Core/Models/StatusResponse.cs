namespace ColdStart.Core.Models;

/// <summary>
/// Hattın o anki durumunu raporlayan model. Hangi katmanın aktif olduğu,
/// kaç belge var, bir sonraki geçiş eşiğine ne kadar kaldığını gösterir.
/// </summary>
public sealed class StatusResponse
{
    /// <summary>Toplam belge sayısı.</summary>
    public int DocumentCount { get; init; }

    /// <summary>Aktif katman numarası (1, 2 veya 3). 0 ise hiçbir katman kayıtlı değildir.</summary>
    public int ActiveLayer { get; init; }

    /// <summary>Aktif katmanın okunabilir adı.</summary>
    public required string LayerName { get; init; }

    /// <summary>
    /// Bir sonraki katmana geçiş eşiği. Layer 3'teyse <c>null</c> döner.
    /// </summary>
    public int? NextLayerThreshold { get; init; }

    /// <summary>Bir sonraki eşiğe kadar eklenmesi gereken belge adedi. Layer 3'teyse <c>null</c>.</summary>
    public int? DocumentsUntilNextLayer { get; init; }
}
