using ColdStart.Core.Entities;

namespace ColdStart.Core.Models;

/// <summary>
/// Vektör araması sonucunda dönen tek bir chunk ve benzerlik skoru.
/// Skor, Qdrant'ın cosine similarity değeridir (1.0'a yakın = daha benzer).
/// </summary>
public sealed record ChunkSearchHit
{
    /// <summary>Eşleşen chunk.</summary>
    public required DocumentChunk Chunk { get; init; }

    /// <summary>Sorgu vektörü ile chunk vektörü arasındaki benzerlik skoru.</summary>
    public double Score { get; init; }
}
