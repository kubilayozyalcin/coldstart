using System.Text.Json;
using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;

namespace ColdStart.Api.Hosting;

/// <summary>
/// Uygulama açılışında <c>data/synthetic/documents_seed.json</c> dosyasını
/// okuyup, store boşsa belgeleri yükleyen hosted service. Sentetik veri
/// üzerinden demo akışını kolaylaştırır. Var olan kayıtlar üzerine yazmaz.
/// </summary>
public sealed class DocumentSeedHostedService : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<DocumentSeedHostedService> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>DI üzerinden gerekli bağımlılıkları alır.</summary>
    public DocumentSeedHostedService(
        IServiceProvider provider,
        IHostEnvironment environment,
        ILogger<DocumentSeedHostedService> logger)
    {
        _provider = provider;
        _environment = environment;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string seedPath = Path.Combine(_environment.ContentRootPath,
            "..", "..", "data", "synthetic", "documents_seed.json");
        seedPath = Path.GetFullPath(seedPath);

        if (!File.Exists(seedPath))
        {
            _logger.LogInformation("Seed dosyası bulunamadı: {Path} — atlanıyor.", seedPath);
            return;
        }

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IDocumentStore store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        IDocumentIngestService ingest = scope.ServiceProvider.GetRequiredService<IDocumentIngestService>();

        int existing = await store.CountAsync(cancellationToken);
        if (existing > 0)
        {
            _logger.LogInformation("Store'da {Count} belge mevcut; seed çalıştırılmıyor.", existing);
            return;
        }

        await using FileStream stream = File.OpenRead(seedPath);
        SeedDocument[]? seeds = await JsonSerializer.DeserializeAsync<SeedDocument[]>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken);

        if (seeds is null || seeds.Length == 0)
        {
            _logger.LogWarning("Seed dosyası boş veya geçersiz.");
            return;
        }

        int loaded = 0;
        foreach (SeedDocument seed in seeds)
        {
            IngestRequest request = new()
            {
                Id = seed.Id,
                Content = seed.Content,
                Metadata = seed.Metadata
            };
            var result = await ingest.IngestAsync(request, cancellationToken);
            if (result.IsSuccess) loaded++;
        }
        _logger.LogInformation("Seed tamamlandı: {Loaded} belge yüklendi.", loaded);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private sealed class SeedDocument
    {
        public string? Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public Dictionary<string, string>? Metadata { get; init; }
    }
}
