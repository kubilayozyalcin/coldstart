using ColdStart.Api.Extensions;
using ColdStart.Core.Extensions;
using ColdStart.Keyword.Extensions;
using ColdStart.Embedding.Extensions;
using ColdStart.VectorRag.Extensions;
using ColdStart.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddColdStartCore(builder.Configuration)
    .AddPersistenceInMemory()
    .AddKeywordSearch(builder.Configuration)
    .AddEmbeddingSearch(builder.Configuration)
    .AddVectorRag(builder.Configuration)
    .AddApiPresentation()
    .AddDocumentSeed();

var app = builder.Build();

app.UseApiPipeline();

app.Run();
