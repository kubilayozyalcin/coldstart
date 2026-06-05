using System.Diagnostics;
using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Pipeline;

/// <summary>
/// <see cref="ISearchPipeline"/>'ı saran metrik decorator'ı. Her aramanın
/// süresini, aktif katmanını ve belge sayısını <see cref="ISearchMetricsRecorder"/>'a
/// kaydeder; yönlendirme davranışına dokunmaz. Decorator pattern sayesinde
/// <see cref="PipelineRouter"/> tek sorumluluğunu (routing) korur.
/// </summary>
public sealed class MetricsRecordingPipeline : ISearchPipeline
{
    private readonly ISearchPipeline _inner;
    private readonly ISearchMetricsRecorder _recorder;

    /// <summary>Sarılacak pipeline ve metrik kaydediciyi alır.</summary>
    public MetricsRecordingPipeline(ISearchPipeline inner, ISearchMetricsRecorder recorder)
    {
        _inner = inner;
        _recorder = recorder;
    }

    /// <inheritdoc />
    public async Task<Result<SearchResponse>> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        long start = Stopwatch.GetTimestamp();
        Result<SearchResponse> result = await _inner.SearchAsync(request, cancellationToken);
        long durationMs = (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        _recorder.Record(result.IsSuccess
            ? new SearchMetricEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                ActiveLayer = result.Value.ActiveLayer,
                LayerName = result.Value.LayerName,
                DocumentCount = result.Value.DocumentCount,
                DurationMs = durationMs,
                Success = true
            }
            : new SearchMetricEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                ActiveLayer = 0,
                LayerName = "Yok",
                DocumentCount = 0,
                DurationMs = durationMs,
                Success = false,
                ErrorCode = result.Error.Code
            });

        return result;
    }

    /// <inheritdoc />
    public Task<StatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
        => _inner.GetStatusAsync(cancellationToken);
}
