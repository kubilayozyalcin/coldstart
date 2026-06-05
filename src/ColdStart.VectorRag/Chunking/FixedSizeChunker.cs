using ColdStart.Core.Entities;
using ColdStart.VectorRag.Options;
using Microsoft.Extensions.Options;

namespace ColdStart.VectorRag.Chunking;

/// <summary>
/// Sabit boyutlu, örtüşmeli (overlapping) chunking stratejisi. Kelime ortasından
/// kesmemek için pencere sonundaki son boşlukta kırpar; ardışık pencereler
/// <see cref="VectorRagOptions.ChunkOverlap"/> kadar örtüşür. Deterministiktir:
/// aynı içerik her zaman aynı kimlikli chunk'ları üretir.
/// </summary>
public sealed class FixedSizeChunker : IDocumentChunker
{
    private readonly VectorRagOptions _options;

    /// <summary>DI üzerinden chunking ayarlarını alır.</summary>
    public FixedSizeChunker(IOptions<VectorRagOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public IReadOnlyList<DocumentChunk> Chunk(Document document)
    {
        string content = document.Content;
        string contentHash = ContentHasher.Compute(content);
        int size = Math.Max(1, _options.ChunkSize);
        int overlap = Math.Clamp(_options.ChunkOverlap, 0, size - 1);

        if (content.Length <= size)
        {
            return new[] { CreateChunk(document.Id, 0, content, contentHash) };
        }

        var chunks = new List<DocumentChunk>();
        int position = 0;
        int index = 0;

        while (position < content.Length)
        {
            int windowEnd = Math.Min(position + size, content.Length);

            // Kelimeyi ortadan bölmemek için pencere içindeki son boşlukta kes;
            // boşluk yoksa (tek uzun token) pencereyi olduğu gibi kullan.
            if (windowEnd < content.Length)
            {
                int lastSpace = content.LastIndexOf(' ', windowEnd - 1, windowEnd - position);
                if (lastSpace > position)
                    windowEnd = lastSpace;
            }

            string piece = content[position..windowEnd].Trim();
            if (piece.Length > 0)
                chunks.Add(CreateChunk(document.Id, index++, piece, contentHash));

            if (windowEnd >= content.Length)
                break;

            // Örtüşme kadar geri giderek devam et; ilerleme garantisi için en az 1 karakter.
            int next = windowEnd - overlap;

            // Örtüşme başlangıcı kelime ortasına denk geldiyse bir sonraki kelime
            // başına kaydır — chunk'lar yarım kelimeyle başlamasın. Boşluk
            // bulunamazsa (tek dev token) içerik kaybetmemek için pencere
            // sonundan devam edilir.
            if (next > 0 && content[next - 1] != ' ')
            {
                int nextSpace = content.IndexOf(' ', next);
                next = nextSpace >= 0 ? Math.Min(nextSpace + 1, windowEnd) : windowEnd;
            }

            position = Math.Max(next, position + 1);
        }

        return chunks;
    }

    private static DocumentChunk CreateChunk(string documentId, int index, string content, string contentHash) => new()
    {
        Id = $"{documentId}#{index}",
        DocumentId = documentId,
        Index = index,
        Content = content,
        ContentHash = contentHash
    };
}
