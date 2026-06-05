using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Pipeline;
using ColdStart.Core.Results;
using ColdStart.Core.Services;
using ColdStart.Persistence.InMemory;
using Microsoft.Extensions.Options;

namespace ColdStart.Tests.Services;

public sealed class DocumentIngestServiceTests
{
    [Fact]
    public async Task Rejects_content_exceeding_max_length()
    {
        var store = new InMemoryDocumentStore();
        var router = new PipelineRouter(
            store,
            Array.Empty<ISearchLayer>(),
            Options.Create(new PipelineOptions()));
        var ingest = new DocumentIngestService(store, router);

        string oversize = new('x', DocumentIngestService.MaxContentLength + 1);
        var result = await ingest.IngestAsync(new IngestRequest { Content = oversize });

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("document.content_too_long", result.Error.Code);
        Assert.Equal(0, await store.CountAsync());
    }

    [Fact]
    public async Task Accepts_content_at_max_length()
    {
        var store = new InMemoryDocumentStore();
        var router = new PipelineRouter(
            store,
            Array.Empty<ISearchLayer>(),
            Options.Create(new PipelineOptions()));
        var ingest = new DocumentIngestService(store, router);

        string content = new('x', DocumentIngestService.MaxContentLength);
        var result = await ingest.IngestAsync(new IngestRequest { Content = content });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, await store.CountAsync());
    }
}
