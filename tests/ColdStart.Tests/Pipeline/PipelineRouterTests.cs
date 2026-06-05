using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Pipeline;
using ColdStart.Core.Results;
using ColdStart.Persistence.InMemory;
using Microsoft.Extensions.Options;

namespace ColdStart.Tests.Pipeline;

public sealed class PipelineRouterTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(49, 1)]
    [InlineData(50, 2)]
    [InlineData(199, 2)]
    [InlineData(200, 3)]
    [InlineData(1000, 3)]
    public async Task Selects_correct_layer_by_document_count(int documentCount, int expectedLayer)
    {
        var store = new InMemoryDocumentStore();
        for (int i = 0; i < documentCount; i++)
            await store.UpsertAsync(new Document { Id = $"d-{i}", Content = $"belge {i}" });

        var layers = new ISearchLayer[]
        {
            new FakeLayer(1, "L1"),
            new FakeLayer(2, "L2"),
            new FakeLayer(3, "L3"),
        };
        var options = Options.Create(new PipelineOptions { Layer2Threshold = 50, Layer3Threshold = 200 });
        var router = new PipelineRouter(store, layers, options);

        var status = await router.GetStatusAsync();
        Assert.Equal(expectedLayer, status.ActiveLayer);
    }

    [Fact]
    public async Task Search_delegates_to_selected_layer()
    {
        var store = new InMemoryDocumentStore();
        var l1 = new FakeLayer(1, "L1");
        var l2 = new FakeLayer(2, "L2");
        var router = new PipelineRouter(
            store,
            new ISearchLayer[] { l1, l2 },
            Options.Create(new PipelineOptions { Layer2Threshold = 1, Layer3Threshold = 100 }));

        await store.UpsertAsync(new Document { Id = "1", Content = "x" });

        var result = await router.SearchAsync(new SearchRequest { Query = "q" });
        Assert.True(result.IsSuccess);
        Assert.Equal("L2", result.Value.LayerName);
        Assert.Equal(1, l2.CallCount);
        Assert.Equal(0, l1.CallCount);
    }

    [Fact]
    public async Task Search_returns_validation_error_for_empty_query()
    {
        var router = new PipelineRouter(
            new InMemoryDocumentStore(),
            new ISearchLayer[] { new FakeLayer(1, "L1") },
            Options.Create(new PipelineOptions()));

        var result = await router.SearchAsync(new SearchRequest { Query = "  " });
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    private sealed class FakeLayer : ISearchLayer
    {
        public int LayerNumber { get; }
        public string LayerName { get; }
        public int CallCount { get; private set; }

        public FakeLayer(int number, string name)
        {
            LayerNumber = number;
            LayerName = name;
        }

        public Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
        {
            CallCount++;
            var response = new SearchResponse
            {
                Answer = "ok",
                ActiveLayer = LayerNumber,
                LayerName = LayerName,
                DocumentCount = 0
            };
            return Task.FromResult<Result<SearchResponse>>(response);
        }
    }
}
