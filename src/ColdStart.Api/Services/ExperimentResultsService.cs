using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.Core.Services;

namespace ColdStart.Api.Services;

/// <summary>
/// <see cref="IExperimentResultsService"/> uygulaması. <c>data/results</c>
/// klasöründeki en güncel <c>activation-time-*.csv</c> dosyasını bulur ve
/// <see cref="ActivationCsvParser"/> ile özetler. Klasör veya dosya yoksa boş
/// özet döner — deney koşulmamış olması bir hata durumu değildir. Klasör yolu,
/// seed verisiyle aynı kuralla çözülür (content root'tan iki üst dizin; Docker
/// imajında <c>/data</c>).
/// </summary>
public sealed class ExperimentResultsService : IExperimentResultsService
{
    private readonly IHostEnvironment _environment;

    /// <summary>DI üzerinden host ortamını alır.</summary>
    public ExperimentResultsService(IHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <inheritdoc />
    public async Task<Result<ExperimentSummaryResponse>> GetActivationSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        string resultsDir = Path.GetFullPath(Path.Combine(
            _environment.ContentRootPath, "..", "..", "data", "results"));

        string? latest = Directory.Exists(resultsDir)
            ? Directory.EnumerateFiles(resultsDir, "activation-time-*.csv")
                .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
                .FirstOrDefault()
            : null;

        if (latest is null)
        {
            return new ExperimentSummaryResponse
            {
                FileName = null,
                Points = Array.Empty<ExperimentPoint>()
            };
        }

        string[] lines = await File.ReadAllLinesAsync(latest, cancellationToken);
        return new ExperimentSummaryResponse
        {
            FileName = Path.GetFileName(latest),
            Points = ActivationCsvParser.Parse(lines)
        };
    }
}
