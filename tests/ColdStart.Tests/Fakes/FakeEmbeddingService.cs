using ColdStart.Core.Abstractions;
using ColdStart.Core.Results;

namespace ColdStart.Tests.Fakes;

/// <summary>
/// Test amaçlı deterministik embedding üreten sahte servis. OpenAI çağrısı
/// yapmaz; metnin karakter kodlarından sabit boyutlu bir vektör türetir.
/// Cosine similarity testleri için yeterlidir.
/// </summary>
public sealed class FakeEmbeddingService : IEmbeddingService
{
    public string ModelName => "fake-deterministic";
    public int Dimension { get; }

    public FakeEmbeddingService(int dimension = 16)
    {
        Dimension = dimension;
    }

    public Task<Result<float[]>> EmbedAsync(string text, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<float[]>>(BuildVector(text));

    public Task<Result<IReadOnlyList<float[]>>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<float[]> vectors = texts.Select(BuildVector).ToArray();
        return Task.FromResult(Result.Success(vectors));
    }

    private float[] BuildVector(string text)
    {
        float[] vector = new float[Dimension];
        if (string.IsNullOrEmpty(text)) return vector;
        for (int i = 0; i < text.Length; i++)
        {
            int slot = i % Dimension;
            vector[slot] += text[i] / 100f;
        }
        return vector;
    }
}
