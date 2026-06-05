namespace ColdStart.Core.Models;

/// <summary>
/// Belgeyi listelemek için kullanılan özet model. Tam <c>Content</c> ve
/// embedding vektörü dışarı verilmez — sadece kısa önizleme, embedding
/// hazır mı bilgisi ve metaveri taşınır.
/// </summary>
public sealed class DocumentSummary
{
    /// <summary>Belgeyi tanımlayan kimlik.</summary>
    public required string Id { get; init; }

    /// <summary>İçeriğin ilk N karakterlik önizlemesi.</summary>
    public required string ContentPreview { get; init; }

    /// <summary>Toplam içerik karakter uzunluğu.</summary>
    public int ContentLength { get; init; }

    /// <summary>Belgenin embedding vektörü hazır mı?</summary>
    public bool HasEmbedding { get; init; }

    /// <summary>Belge metaverisi (varsa).</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>Belgenin store'a eklendiği UTC zaman damgası.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
