using ColdStart.Core.Models;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Arama metriklerinin kayıt ve okuma soyutlaması. Uygulama in-memory ring
/// buffer'dır; restart'ta sıfırlanır (akademik prototip için kabul edilebilir).
/// </summary>
public interface ISearchMetricsRecorder
{
    /// <summary>Bir arama kaydını ekler; kapasite aşıldığında en eski kayıt düşer.</summary>
    void Record(SearchMetricEntry entry);

    /// <summary>Tüm kayıtların eskiden yeniye sıralı kopyasını döner.</summary>
    IReadOnlyList<SearchMetricEntry> GetAll();
}
