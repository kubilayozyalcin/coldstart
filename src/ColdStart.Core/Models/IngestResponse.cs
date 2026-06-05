namespace ColdStart.Core.Models;

/// <summary>
/// Belge ekleme işleminin sonucu.
/// </summary>
public sealed class IngestResponse
{
    /// <summary>Eklenen veya güncellenen belgenin kimliği (üretildiyse GUID).</summary>
    public required string DocumentId { get; init; }

    /// <summary>İşlem başarıyla tamamlandı mı?</summary>
    public bool Success { get; init; }

    /// <summary>İşlem sonrası toplam belge sayısı.</summary>
    public int DocumentCount { get; init; }

    /// <summary>İşlem sonrası aktif katman numarası (eşik geçilirse otomatik güncellenir).</summary>
    public int ActiveLayer { get; init; }
}
