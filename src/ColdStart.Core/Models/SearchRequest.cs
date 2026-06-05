namespace ColdStart.Core.Models;

/// <summary>
/// Kullanıcının arama isteği. <see cref="Query"/> zorunlu; <see cref="TopK"/>
/// döndürülecek belge adedidir (varsayılan 5).
/// </summary>
public sealed class SearchRequest
{
    /// <summary>Kullanıcı sorgusu (doğal dil veya anahtar kelime).</summary>
    public required string Query { get; init; }

    /// <summary>Sonuçta dönecek en alakalı belge adedi. 1–50 arası.</summary>
    public int TopK { get; init; } = 5;
}
