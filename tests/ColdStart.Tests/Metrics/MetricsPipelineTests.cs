using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Pipeline;
using ColdStart.Core.Results;
using ColdStart.Core.Services;

namespace ColdStart.Tests.Metrics;

/// <summary>Test amaçlı sahte pipeline: önceden ayarlanmış sonucu döner.</summary>
internal sealed class FakeSearchPipeline : ISearchPipeline
{
    public Result<SearchResponse> NextResult { get; set; } = Error.Failure("ayarlanmadı");

    public Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(NextResult);

    public Task<StatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new StatusResponse
        {
            DocumentCount = 0,
            ActiveLayer = 1,
            LayerName = "Keyword (BM25)",
            NextLayerThreshold = null,
            DocumentsUntilNextLayer = null
        });
}

public sealed class MetricsPipelineTests
{
    private static SearchResponse Response(int layer, int documentCount) => new()
    {
        Answer = "cevap",
        ActiveLayer = layer,
        LayerName = $"Layer {layer}",
        DocumentCount = documentCount,
        Sources = Array.Empty<SearchSource>()
    };

    [Fact]
    public async Task Successful_search_is_recorded_with_layer_and_count()
    {
        var recorder = new InMemorySearchMetricsRecorder();
        var inner = new FakeSearchPipeline { NextResult = Response(layer: 2, documentCount: 60) };
        var pipeline = new MetricsRecordingPipeline(inner, recorder);

        await pipeline.SearchAsync(new SearchRequest { Query = "soru" });

        var entry = Assert.Single(recorder.GetAll());
        Assert.True(entry.Success);
        Assert.Equal(2, entry.ActiveLayer);
        Assert.Equal(60, entry.DocumentCount);
    }

    [Fact]
    public async Task Failed_search_is_recorded_with_error_code()
    {
        var recorder = new InMemorySearchMetricsRecorder();
        var inner = new FakeSearchPipeline
        {
            NextResult = Error.External("Qdrant kapalı.", code: "qdrant.unavailable")
        };
        var pipeline = new MetricsRecordingPipeline(inner, recorder);

        await pipeline.SearchAsync(new SearchRequest { Query = "soru" });

        var entry = Assert.Single(recorder.GetAll());
        Assert.False(entry.Success);
        Assert.Equal("qdrant.unavailable", entry.ErrorCode);
    }

    [Fact]
    public async Task Layer_transitions_are_derived_from_consecutive_successful_searches()
    {
        var recorder = new InMemorySearchMetricsRecorder();
        var inner = new FakeSearchPipeline();
        var pipeline = new MetricsRecordingPipeline(inner, recorder);
        var query = new SearchRequest { Query = "soru" };

        inner.NextResult = Response(layer: 1, documentCount: 40);
        await pipeline.SearchAsync(query);
        inner.NextResult = Error.Failure("geçici hata"); // başarısız kayıt geçiş sayılmamalı
        await pipeline.SearchAsync(query);
        inner.NextResult = Response(layer: 2, documentCount: 55);
        await pipeline.SearchAsync(query);
        inner.NextResult = Response(layer: 3, documentCount: 210);
        await pipeline.SearchAsync(query);

        var metrics = new MetricsQueryService(recorder).GetMetrics();

        Assert.Equal(4, metrics.Entries.Count);
        Assert.Equal(2, metrics.Transitions.Count);
        Assert.Equal((1, 2, 55), (metrics.Transitions[0].FromLayer, metrics.Transitions[0].ToLayer, metrics.Transitions[0].DocumentCount));
        Assert.Equal((2, 3, 210), (metrics.Transitions[1].FromLayer, metrics.Transitions[1].ToLayer, metrics.Transitions[1].DocumentCount));
    }
}
