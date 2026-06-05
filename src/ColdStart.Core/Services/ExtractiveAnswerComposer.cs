namespace ColdStart.Core.Services;

/// <summary>
/// LLM kullanmayan katmanların (Layer 1 — BM25, Layer 2 — Embedding) extractive
/// cevap kompozisyonunu tek noktada toplar. Top-K retrieval her zaman K aday
/// döndürdüğü için cevaba yalnızca en yüksek skorun belirli bir oranını
/// (<see cref="DefaultScoreRatio"/>) aşan belgeler alınır; aksi hâlde sorguyla
/// ilgisiz belgeler cevapta yan yana dizilip tutarsız bir metin oluşturur.
/// Eşik göreli olduğu için sınırsız BM25 skorlarında da [-1, 1] aralığındaki
/// cosine similarity'de de ek ayar gerektirmeden çalışır. Kaynak listesi bu
/// filtreden etkilenmez; tüm top-K adaylar skorlarıyla birlikte gösterilir.
/// </summary>
public static class ExtractiveAnswerComposer
{
    /// <summary>Cevap ve kaynak snippet'lerinin maksimum karakter uzunluğu.</summary>
    public const int SnippetLength = 240;

    /// <summary>
    /// Bir belgenin cevaba girebilmesi için skorunun, en yüksek skora oranla
    /// taşıması gereken minimum değer.
    /// </summary>
    public const double DefaultScoreRatio = 0.6;

    /// <summary>
    /// Skora göre azalan sıralı belge listesinden extractive cevap üretir.
    /// En yüksek skorlu belge her zaman dahildir; kalanlar yalnızca skorları
    /// <paramref name="scoreRatio"/> × en yüksek skor eşiğini geçerse eklenir.
    /// Liste boşsa boş string döner.
    /// </summary>
    public static string Compose(
        IReadOnlyList<(string Content, double Score)> rankedDocuments,
        double scoreRatio = DefaultScoreRatio)
    {
        if (rankedDocuments.Count == 0)
            return string.Empty;

        double threshold = rankedDocuments[0].Score * scoreRatio;
        IEnumerable<string> bullets = rankedDocuments
            .Where((d, index) => index == 0 || d.Score >= threshold)
            .Select(d => $"• {MakeSnippet(d.Content)}");

        return string.Join("\n\n", bullets);
    }

    /// <summary>İçeriği <see cref="SnippetLength"/> karakterde kesip "..." ekler.</summary>
    public static string MakeSnippet(string content) =>
        content.Length <= SnippetLength ? content : content[..SnippetLength] + "...";
}
