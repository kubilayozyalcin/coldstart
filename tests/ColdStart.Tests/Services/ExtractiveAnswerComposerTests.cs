using ColdStart.Core.Services;

namespace ColdStart.Tests.Services;

public sealed class ExtractiveAnswerComposerTests
{
    [Fact]
    public void Excludes_documents_below_relative_score_threshold()
    {
        // Demo'da gözlenen senaryo: tek alakalı belge (0.480), kalanlar cosine
        // baseline gürültüsü (0.220-0.262). Eşik = 0.480 * 0.6 = 0.288.
        var ranked = new[]
        {
            ("RAG, LLM cevaplarını dış bilgi tabanıyla zenginleştirir.", 0.480),
            ("ADR yaklaşımı önerilir.", 0.262),
            ("Teknik borç faiziyle birikir.", 0.255),
            ("PR'lar küçük tutulmalıdır.", 0.224),
            ("Git dağıtık sürüm kontrol sistemidir.", 0.220)
        };

        string answer = ExtractiveAnswerComposer.Compose(ranked);

        Assert.Contains("RAG", answer);
        Assert.DoesNotContain("ADR", answer);
        Assert.DoesNotContain("Teknik borç", answer);
        Assert.DoesNotContain("PR", answer);
        Assert.DoesNotContain("Git", answer);
    }

    [Fact]
    public void Includes_all_documents_above_threshold()
    {
        var ranked = new[]
        {
            ("Birinci belge.", 0.50),
            ("İkinci belge.", 0.45),
            ("Üçüncü belge.", 0.31)
        };

        string answer = ExtractiveAnswerComposer.Compose(ranked);

        Assert.Contains("Birinci", answer);
        Assert.Contains("İkinci", answer);
        Assert.Contains("Üçüncü", answer);
    }

    [Fact]
    public void Always_includes_top_document()
    {
        var ranked = new[] { ("Tek belge.", 0.05) };

        string answer = ExtractiveAnswerComposer.Compose(ranked);

        Assert.Contains("Tek belge.", answer);
    }

    [Fact]
    public void Returns_empty_string_for_empty_list()
    {
        string answer = ExtractiveAnswerComposer.Compose(Array.Empty<(string, double)>());

        Assert.Equal(string.Empty, answer);
    }

    [Fact]
    public void Separates_bullets_with_blank_line()
    {
        var ranked = new[]
        {
            ("Birinci belge.", 1.0),
            ("İkinci belge.", 0.9)
        };

        string answer = ExtractiveAnswerComposer.Compose(ranked);

        Assert.Equal("• Birinci belge.\n\n• İkinci belge.", answer);
    }

    [Fact]
    public void Snippet_truncates_long_content_with_ellipsis()
    {
        string content = new('a', ExtractiveAnswerComposer.SnippetLength + 10);

        string snippet = ExtractiveAnswerComposer.MakeSnippet(content);

        Assert.Equal(ExtractiveAnswerComposer.SnippetLength + 3, snippet.Length);
        Assert.EndsWith("...", snippet);
    }

    [Fact]
    public void Snippet_keeps_short_content_intact()
    {
        string snippet = ExtractiveAnswerComposer.MakeSnippet("Kısa içerik.");

        Assert.Equal("Kısa içerik.", snippet);
    }
}
