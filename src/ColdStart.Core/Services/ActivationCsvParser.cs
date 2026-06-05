using ColdStart.Core.Models;

namespace ColdStart.Core.Services;

/// <summary>
/// Activation deneyinin CSV çıktısını (<c>strategy,corpusSize,query,...</c>)
/// parse edip strateji × corpus boyutu bazında ortalama metriklere indirger.
/// Saf fonksiyondur; dosya erişimi çağırana aittir. Sorgu alanı tırnaklı ve
/// virgül içerebileceği için satırlar alıntı-duyarlı bölünür.
/// </summary>
public static class ActivationCsvParser
{
    /// <summary>
    /// CSV satırlarını (başlık dahil) özet noktalara çevirir. Başlıksız veya
    /// boş girdi boş liste döner; sayıya çevrilemeyen satırlar atlanır.
    /// </summary>
    public static IReadOnlyList<ExperimentPoint> Parse(IReadOnlyList<string> lines)
    {
        if (lines.Count < 2) return Array.Empty<ExperimentPoint>();

        string[] header = SplitCsvLine(lines[0]);
        int strategyIx = Array.IndexOf(header, "strategy");
        int corpusIx = Array.IndexOf(header, "corpusSize");
        int durationIx = Array.IndexOf(header, "durationMs");
        int faithIx = Array.IndexOf(header, "faithfulness");
        int relevancyIx = Array.IndexOf(header, "relevancy");
        if (strategyIx < 0 || corpusIx < 0 || durationIx < 0 || faithIx < 0 || relevancyIx < 0)
            return Array.Empty<ExperimentPoint>();

        Dictionary<(string Strategy, int Corpus), List<(double Rel, double Faith, double Dur)>> groups = new();
        foreach (string line in lines.Skip(1))
        {
            string[] fields = SplitCsvLine(line);
            if (fields.Length <= Math.Max(relevancyIx, Math.Max(durationIx, faithIx))) continue;
            if (!int.TryParse(fields[corpusIx], out int corpus)) continue;
            if (!double.TryParse(fields[relevancyIx], System.Globalization.CultureInfo.InvariantCulture, out double rel)) continue;
            if (!double.TryParse(fields[faithIx], System.Globalization.CultureInfo.InvariantCulture, out double faith)) continue;
            if (!double.TryParse(fields[durationIx], System.Globalization.CultureInfo.InvariantCulture, out double dur)) continue;

            (string, int) key = (fields[strategyIx], corpus);
            if (!groups.TryGetValue(key, out var list))
                groups[key] = list = new List<(double, double, double)>();
            list.Add((rel, faith, dur));
        }

        return groups
            .OrderBy(g => g.Key.Strategy, StringComparer.Ordinal)
            .ThenBy(g => g.Key.Corpus)
            .Select(g => new ExperimentPoint
            {
                Strategy = g.Key.Strategy,
                CorpusSize = g.Key.Corpus,
                Relevancy = g.Value.Average(v => v.Rel),
                Faithfulness = g.Value.Average(v => v.Faith),
                DurationMs = g.Value.Average(v => v.Dur)
            })
            .ToArray();
    }

    private static string[] SplitCsvLine(string line)
    {
        List<string> fields = new();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();
        foreach (char c in line.TrimStart('﻿'))
        {
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { fields.Add(current.ToString()); current.Clear(); continue; }
            current.Append(c);
        }
        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
