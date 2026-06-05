using ColdStart.Api.Extensions;
using ColdStart.Api.Hosting;
using ColdStart.Core.Extensions;
using ColdStart.Keyword.Extensions;
using ColdStart.Embedding.Extensions;
using ColdStart.VectorRag.Extensions;
using ColdStart.Persistence.Extensions;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddColdStartCore(builder.Configuration)
    .AddPersistenceInMemory()
    .AddKeywordSearch(builder.Configuration)
    .AddEmbeddingSearch(builder.Configuration)
    .AddVectorRag(builder.Configuration)
    .AddApiPresentation()
    .AddDocumentSeed()
    .AddExperimentResults();

var app = builder.Build();

app.UseApiPipeline();

app.Run();
