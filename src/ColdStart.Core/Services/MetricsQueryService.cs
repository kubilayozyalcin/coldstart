using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;

namespace ColdStart.Core.Services;

/// <summary>
/// <see cref="IMetricsQueryService"/>'in varsayılan uygulaması. Kayıtlardan
/// katman geçişlerini türetir: ardışık iki başarılı aramada aktif katman
/// değiştiyse bir <see cref="LayerTransition"/> üretilir.
/// </summary>
public sealed class MetricsQueryService : IMetricsQueryService
{
    private readonly ISearchMetricsRecorder _recorder;

    /// <summary>DI üzerinden metrik kaydediciyi alır.</summary>
    public MetricsQueryService(ISearchMetricsRecorder recorder)
    {
        _recorder = recorder;
    }

    /// <inheritdoc />
    public MetricsResponse GetMetrics()
    {
        IReadOnlyList<SearchMetricEntry> entries = _recorder.GetAll();

        var transitions = new List<LayerTransition>();
        SearchMetricEntry? previous = null;

        foreach (SearchMetricEntry entry in entries.Where(e => e.Success))
        {
            if (previous is not null && previous.ActiveLayer != entry.ActiveLayer)
            {
                transitions.Add(new LayerTransition
                {
                    Timestamp = entry.Timestamp,
                    FromLayer = previous.ActiveLayer,
                    ToLayer = entry.ActiveLayer,
                    DocumentCount = entry.DocumentCount
                });
            }
            previous = entry;
        }

        return new MetricsResponse
        {
            Entries = entries,
            Transitions = transitions
        };
    }
}
