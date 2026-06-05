using ColdStart.Core.Entities;
using ColdStart.VectorRag.Chunking;
using ColdStart.VectorRag.Options;
using Microsoft.Extensions.Options;

namespace ColdStart.Tests.VectorRag;

public sealed class FixedSizeChunkerTests
{
    private static FixedSizeChunker CreateChunker(int chunkSize = 100, int overlap = 20) =>
        new(Microsoft.Extensions.Options.Options.Create(new VectorRagOptions
        {
            ChunkSize = chunkSize,
            ChunkOverlap = overlap
        }));

    [Fact]
    public void Short_document_yields_single_chunk()
    {
        var document = new Document { Id = "d1", Content = "kısa içerik" };

        var chunks = CreateChunker(chunkSize: 100).Chunk(document);

        Assert.Single(chunks);
        Assert.Equal("d1#0", chunks[0].Id);
        Assert.Equal("kısa içerik", chunks[0].Content);
    }

    [Fact]
    public void Long_document_is_split_with_overlap()
    {
        string content = string.Join(' ', Enumerable.Repeat("kelime", 100)); // ~700 karakter
        var document = new Document { Id = "d1", Content = content };

        var chunks = CreateChunker(chunkSize: 200, overlap: 50).Chunk(document);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.True(c.Content.Length <= 200));
        // Sıra numaraları ardışık ve kimlikler deterministik olmalı.
        Assert.Equal(Enumerable.Range(0, chunks.Count), chunks.Select(c => c.Index));
        Assert.All(chunks, c => Assert.Equal($"d1#{c.Index}", c.Id));
    }

    [Fact]
    public void Chunks_do_not_cut_words_in_half()
    {
        string content = string.Join(' ', Enumerable.Repeat("belgelerimizden", 50));
        var document = new Document { Id = "d1", Content = content };

        var chunks = CreateChunker(chunkSize: 100, overlap: 10).Chunk(document);

        Assert.All(chunks, c =>
            Assert.All(c.Content.Split(' '), word => Assert.Equal("belgelerimizden", word)));
    }

    [Fact]
    public void Same_content_produces_same_hash_different_content_does_not()
    {
        var chunker = CreateChunker();
        var first = chunker.Chunk(new Document { Id = "d1", Content = "aynı içerik" });
        var second = chunker.Chunk(new Document { Id = "d1", Content = "aynı içerik" });
        var changed = chunker.Chunk(new Document { Id = "d1", Content = "farklı içerik" });

        Assert.Equal(first[0].ContentHash, second[0].ContentHash);
        Assert.NotEqual(first[0].ContentHash, changed[0].ContentHash);
    }
}
