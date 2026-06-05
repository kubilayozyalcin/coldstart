using ColdStart.Core.Models;
using ColdStart.Core.Services;

namespace ColdStart.Tests.Services;

public sealed class ActivationCsvParserTests
{
    private const string Header =
        "strategy,corpusSize,query,expectedDocId,expectedPresent,sourceHit,activeLayer,durationMs,faithfulness,relevancy";

    [Fact]
    public void Averages_metrics_per_strategy_and_corpus_size()
    {
        string[] lines =
        {
            Header,
            "adaptive,10,\"Sorgu bir?\",doc-001,1,1,1,4,1,1",
            "adaptive,10,\"Sorgu iki?\",doc-002,1,1,1,2,1,0.5",
            "rag-only,10,\"Sorgu bir?\",doc-001,1,1,3,2000,1,1"
        };

        IReadOnlyList<ExperimentPoint> points = ActivationCsvParser.Parse(lines);

        Assert.Equal(2, points.Count);
        ExperimentPoint adaptive = Assert.Single(points, p => p.Strategy == "adaptive");
        Assert.Equal(10, adaptive.CorpusSize);
        Assert.Equal(0.75, adaptive.Relevancy, precision: 3);
        Assert.Equal(1.0, adaptive.Faithfulness, precision: 3);
        Assert.Equal(3.0, adaptive.DurationMs, precision: 3);
    }

    [Fact]
    public void Handles_quoted_query_containing_comma()
    {
        string[] lines =
        {
            Header,
            "adaptive,5,\"Sprint, retro ve planlama nasıl işler?\",doc-001,1,1,1,4,1,0.8"
        };

        IReadOnlyList<ExperimentPoint> points = ActivationCsvParser.Parse(lines);

        ExperimentPoint point = Assert.Single(points);
        Assert.Equal(0.8, point.Relevancy, precision: 3);
        Assert.Equal(5, point.CorpusSize);
    }

    [Fact]
    public void Skips_rows_with_invalid_numbers()
    {
        string[] lines =
        {
            Header,
            "adaptive,bozuk,\"Sorgu?\",doc-001,1,1,1,4,1,1",
            "adaptive,5,\"Sorgu?\",doc-001,1,1,1,4,1,0.6"
        };

        IReadOnlyList<ExperimentPoint> points = ActivationCsvParser.Parse(lines);

        ExperimentPoint point = Assert.Single(points);
        Assert.Equal(5, point.CorpusSize);
    }

    [Fact]
    public void Returns_empty_for_missing_or_headerless_input()
    {
        Assert.Empty(ActivationCsvParser.Parse(Array.Empty<string>()));
        Assert.Empty(ActivationCsvParser.Parse(new[] { "a,b,c", "1,2,3" }));
    }
}
