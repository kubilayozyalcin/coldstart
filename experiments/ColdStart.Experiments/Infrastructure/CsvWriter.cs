using System.Globalization;
using System.Text;

namespace ColdStart.Experiments.Infrastructure;

/// <summary>
/// Deney sonuçlarını <c>data/results/</c> altına CSV olarak yazar. Ondalıklar
/// invariant culture ile yazılır (tez analizinde Excel/pandas uyumu).
/// </summary>
public static class CsvWriter
{
    /// <summary>Satırları başlıkla birlikte zaman damgalı CSV dosyasına yazar; dosya yolunu döner.</summary>
    public static string Write(string repoRoot, string experimentName, string header, IEnumerable<string> rows)
    {
        string dir = Path.Combine(repoRoot, "data", "results");
        Directory.CreateDirectory(dir);

        string stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        string path = Path.Combine(dir, $"{experimentName}-{stamp}.csv");

        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (string row in rows)
            sb.AppendLine(row);

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        return path;
    }

    /// <summary>Ondalık değeri CSV için invariant biçimde yazar.</summary>
    public static string F(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
