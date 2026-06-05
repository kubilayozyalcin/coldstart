namespace ColdStart.Experiments.Experiments;

/// <summary>
/// Deneylerde kullanılan altın (golden) sorgu seti: her sorgunun doğru cevabını
/// içeren seed belgesi bellidir. Retrieval isabeti (hit@K) bu eşleme üzerinden
/// deterministik ölçülür; cevap kalitesi LLM-as-a-judge ile ayrıca puanlanır.
/// </summary>
public sealed record GoldenQuery(string Query, string ExpectedDocumentId);

/// <summary>Altın sorgu setinin sabit tanımı.</summary>
public static class GoldenQueries
{
    /// <summary>Seed corpus'a karşı tanımlı üç altın sorgu.</summary>
    public static readonly IReadOnlyList<GoldenQuery> All = new[]
    {
        new GoldenQuery("Çevik yazılım geliştirmede sprint teslimatları nasıl işler?", "doc-001"),
        new GoldenQuery("Teknik borç nedir ve nasıl yönetilmelidir?", "doc-009"),
        new GoldenQuery("Mikroservis mimarisinde servisler arası iletişim nasıl olur?", "doc-006")
    };
}
