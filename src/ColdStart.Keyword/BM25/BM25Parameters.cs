namespace ColdStart.Keyword.BM25;

/// <summary>
/// BM25 sıralama formülünün parametreleri. Robertson &amp; Zaragoza (2009)'da
/// önerilen varsayılan değerler kullanılır.
/// </summary>
public sealed class BM25Parameters
{
    /// <summary>Konfigürasyon bölümünün adı (<c>appsettings.json</c>).</summary>
    public const string SectionName = "BM25";

    /// <summary>Terim frekansının doygunluk eğrisini kontrol eden parametre. Literatür: 1.2–2.0.</summary>
    public double K1 { get; set; } = 1.5;

    /// <summary>Belge uzunluğu normalizasyonunun ağırlığı. Literatür: 0.75.</summary>
    public double B { get; set; } = 0.75;
}
