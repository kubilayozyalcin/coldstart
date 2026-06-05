using ColdStart.Core.Entities;

namespace ColdStart.VectorRag.Chunking;

/// <summary>
/// Belgeyi vektör indeksine yazılacak parçalara bölen stratejinin soyutlaması.
/// Faz 3'te sabit boyutlu uygulama kullanılır; sentence-aware splitter
/// Future Work kapsamındadır.
/// </summary>
public interface IDocumentChunker
{
    /// <summary>Belgeyi sıralı chunk listesine böler. Embedding alanları boş döner; indeksleme öncesi hesaplanır.</summary>
    IReadOnlyList<DocumentChunk> Chunk(Document document);
}
