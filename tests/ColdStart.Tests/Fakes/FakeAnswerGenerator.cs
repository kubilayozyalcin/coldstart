using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Results;

namespace ColdStart.Tests.Fakes;

/// <summary>
/// Test amaçlı sahte cevap üretici. LLM çağrısı yapmaz; kendisine verilen
/// sorguyu ve bağlam chunk'larını kaydeder, sabit bir cevap döner. Başarısızlık
/// senaryosu test edilecekse <see cref="FailWith"/> set edilir.
/// </summary>
public sealed class FakeAnswerGenerator : IAnswerGenerator
{
    public string ModelName => "fake-llm";

    /// <summary>Set edilirse GenerateAsync bu hatayla başarısız döner.</summary>
    public Error? FailWith { get; set; }

    /// <summary>Son çağrıda gelen sorgu.</summary>
    public string? LastQuery { get; private set; }

    /// <summary>Son çağrıda gelen bağlam chunk'ları.</summary>
    public IReadOnlyList<DocumentChunk> LastContext { get; private set; } = Array.Empty<DocumentChunk>();

    public Task<Result<string>> GenerateAsync(
        string query,
        IReadOnlyList<DocumentChunk> contextChunks,
        CancellationToken cancellationToken = default)
    {
        LastQuery = query;
        LastContext = contextChunks;

        Result<string> result = FailWith is null
            ? $"Cevap({contextChunks.Count} chunk)"
            : FailWith;

        return Task.FromResult(result);
    }
}
