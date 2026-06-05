namespace ColdStart.Core.Models;

/// <summary>
/// Offline batch deneylerinin (activation) UI'da görselleştirilmek üzere
/// özetlenmiş hali. Kayıtlar strateji × corpus boyutu bazında, altın sorgular
/// üzerinden ortalaması alınarak üretilir.
/// </summary>
public sealed class ExperimentSummaryResponse
{
    /// <summary>Özetin üretildiği sonuç dosyasının adı; veri yoksa null.</summary>
    public string? FileName { get; init; }

    /// <summary>Strateji × corpus boyutu başına ortalama metrik noktaları.</summary>
    public required IReadOnlyList<ExperimentPoint> Points { get; init; }
}

/// <summary>Tek bir strateji × corpus boyutu kombinasyonunun ortalama metrikleri.</summary>
public sealed class ExperimentPoint
{
    /// <summary>Deney stratejisi: <c>adaptive</c>, <c>embedding-only</c> veya <c>rag-only</c>.</summary>
    public required string Strategy { get; init; }

    /// <summary>Deney sırasındaki corpus büyüklüğü (belge sayısı).</summary>
    public required int CorpusSize { get; init; }

    /// <summary>Altın sorgular üzerinden ortalama answer relevancy (0-1).</summary>
    public required double Relevancy { get; init; }

    /// <summary>Altın sorgular üzerinden ortalama faithfulness (0-1).</summary>
    public required double Faithfulness { get; init; }

    /// <summary>Altın sorgular üzerinden ortalama arama süresi (milisaniye).</summary>
    public required double DurationMs { get; init; }
}
