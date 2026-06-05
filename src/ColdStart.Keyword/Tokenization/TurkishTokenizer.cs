using System.Globalization;
using System.Text.RegularExpressions;

namespace ColdStart.Keyword.Tokenization;

/// <summary>
/// Türkçe metinler için sade tokenizer. Türkçe lowercase kurallarını
/// (örn. "İ" → "i", "I" → "ı") doğru uygular, tek karakterli token'ları
/// ve stop-word listesindeki kelimeleri eler. BM25 sıralamasında doğrudan
/// kullanılır.
/// </summary>
public sealed partial class TurkishTokenizer
{
    private static readonly CultureInfo TurkishCulture = new("tr-TR");

    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        "ve", "veya", "ile", "bir", "bu", "şu", "o", "ki",
        "mi", "mı", "mu", "mü", "için", "ama", "fakat", "ancak",
        "de", "da", "ne", "ya", "ise", "her", "hep", "çok",
        "az", "daha", "en", "gibi", "kadar", "olarak", "olan",
        "olduğu", "üzere", "tüm", "bazı", "kendi", "şey"
    };

    [GeneratedRegex(@"[^\p{L}\p{N}]+", RegexOptions.Compiled)]
    private static partial Regex SeparatorRegex();

    /// <summary>
    /// Metni Türkçe normalize edip token dizisine ayırır. Boş veya yalnız stop-word
    /// içeren girdilerde boş dizi döner.
    /// </summary>
    public IReadOnlyList<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        string lower = text.ToLower(TurkishCulture);
        string[] raw = SeparatorRegex().Split(lower);

        List<string> tokens = new(capacity: raw.Length);
        foreach (string token in raw)
        {
            if (token.Length < 2) continue;
            if (StopWords.Contains(token)) continue;
            tokens.Add(token);
        }
        return tokens;
    }
}
