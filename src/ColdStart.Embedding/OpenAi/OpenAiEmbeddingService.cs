using ColdStart.Core.Abstractions;
using ColdStart.Core.Configuration;
using ColdStart.Core.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;

namespace ColdStart.Embedding.OpenAi;

/// <summary>
/// <see cref="IEmbeddingService"/>'in OpenAI uygulaması. <c>text-embedding-3-small</c>
/// modeli ile metinleri 1536-boyutlu vektörlere dönüştürür. Batch endpoint
/// kullanarak maliyet ve gecikmeyi düşürür.
/// </summary>
public sealed class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiEmbeddingService> _logger;
    private readonly Lazy<EmbeddingClient> _client;

    /// <summary>DI üzerinden bağımlılıkları alır. Client lazy olarak ilk çağrıda kurulur — böylece API key yokken uygulama yine de açılabilir, hata yalnızca Layer 2 fiilen kullanıldığında <see cref="Result"/> olarak döner.</summary>
    public OpenAiEmbeddingService(
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiEmbeddingService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new Lazy<EmbeddingClient>(() =>
            new OpenAIClient(_options.ApiKey).GetEmbeddingClient(_options.EmbeddingModel));
    }

    /// <inheritdoc />
    public string ModelName => _options.EmbeddingModel;

    /// <inheritdoc />
    public int Dimension => _options.EmbeddingDimension;

    /// <inheritdoc />
    public async Task<Result<float[]>> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Error.Validation("Embedding üretilemez: metin boş.");

        Result keyCheck = EnsureKeyConfigured();
        if (keyCheck.IsFailure) return Result.Failure<float[]>(keyCheck.Error);

        try
        {
            OpenAIEmbedding embedding = await _client.Value.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
            return embedding.ToFloats().ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI embedding çağrısı başarısız.");
            return Error.External($"Embedding çağrısı başarısız: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<float[]>>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts is null || texts.Count == 0)
            return Result.Success<IReadOnlyList<float[]>>(Array.Empty<float[]>());

        if (texts.Any(string.IsNullOrWhiteSpace))
            return Error.Validation("Batch içinde boş metin bulundu.");

        Result keyCheck = EnsureKeyConfigured();
        if (keyCheck.IsFailure) return Result.Failure<IReadOnlyList<float[]>>(keyCheck.Error);

        try
        {
            OpenAIEmbeddingCollection collection = await _client.Value.GenerateEmbeddingsAsync(
                texts,
                cancellationToken: cancellationToken);

            float[][] vectors = collection
                .Select(e => e.ToFloats().ToArray())
                .ToArray();

            return Result.Success<IReadOnlyList<float[]>>(vectors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI batch embedding çağrısı başarısız.");
            return Error.External($"Batch embedding çağrısı başarısız: {ex.Message}");
        }
    }

    private Result EnsureKeyConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            return Result.Failure(Error.External(
                "OpenAI API anahtarı yapılandırılmamış. 'OpenAi:ApiKey' veya 'OPENAI_API_KEY' değişkenini ayarlayın.",
                code: "openai.api_key_missing"));
        return Result.Success();
    }
}
