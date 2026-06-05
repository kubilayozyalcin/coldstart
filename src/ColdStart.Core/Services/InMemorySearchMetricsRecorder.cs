using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;

namespace ColdStart.Core.Services;

/// <summary>
/// <see cref="ISearchMetricsRecorder"/>'ın thread-safe in-memory uygulaması.
/// Sabit kapasiteli ring buffer: kapasite dolduğunda en eski kayıt düşer,
/// bellek kullanımı sınırlı kalır.
/// </summary>
public sealed class InMemorySearchMetricsRecorder : ISearchMetricsRecorder
{
    private const int Capacity = 500;

    private readonly object _gate = new();
    private readonly Queue<SearchMetricEntry> _entries = new();

    /// <inheritdoc />
    public void Record(SearchMetricEntry entry)
    {
        lock (_gate)
        {
            _entries.Enqueue(entry);
            while (_entries.Count > Capacity)
                _entries.Dequeue();
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<SearchMetricEntry> GetAll()
    {
        lock (_gate)
        {
            return _entries.ToArray();
        }
    }
}
