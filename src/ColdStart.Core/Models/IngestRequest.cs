namespace ColdStart.Core.Models;

/// <summary>
/// Sisteme yeni bir belge eklemek için kullanılan istek modeli.
/// </summary>
public sealed class IngestRequest
{
    /// <summary>
    /// Belgenin benzersiz kimliği. Boş bırakılırsa servis GUID üretir.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>Belgenin tam metni. Boş veya yalnızca whitespace içeren değerler reddedilir.</summary>
    public required string Content { get; init; }

    /// <summary>İsteğe bağlı metaveri (kaynak, etiket vs.).</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
