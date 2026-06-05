namespace ColdStart.Core.Entities;

/// <summary>
/// Bilgi tabanındaki bir belgeyi temsil eden değişmez (immutable) varlık.
/// Layer 1'de yalnızca <see cref="Content"/> kullanılır; Layer 2 ve sonrasında
/// <see cref="Embedding"/> alanı hesaplanır ve aynı kayda yazılır.
/// </summary>
public sealed record Document
{
    /// <summary>Belgeyi benzersiz biçimde tanımlayan kimlik (GUID veya kullanıcı tarafından verilen anahtar).</summary>
    public required string Id { get; init; }

    /// <summary>Belgenin tam metni. Tüm arama katmanlarının üzerinde çalıştığı temel alan.</summary>
    public required string Content { get; init; }

    /// <summary>Belgeye iliştirilen serbest metaveri (kaynak, etiket, tarih vb.). İsteğe bağlıdır.</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Belgenin vektörel temsili. Layer 2 / Layer 3 için OpenAI text-embedding-3-small
    /// ile üretilir (1536 boyut). Layer 1 modunda <c>null</c> olabilir.
    /// </summary>
    public float[]? Embedding { get; init; }

    /// <summary>Belgenin store'a eklendiği UTC zaman damgası.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
