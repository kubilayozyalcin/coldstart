using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.Core.Services;

namespace ColdStart.Tests.Metrics;

/// <summary>Test amaçlı sahte hakem: sabit skor döner, çağrı argümanlarını kaydeder.</summary>
internal sealed class FakeRagEvaluator : IRagEvaluator
{
    public Error? FailWith { get; set; }
    public string? LastAnswer { get; private set; }

    public Task<Result<RagEvaluation>> EvaluateAsync(
        string query,
        string answer,
        IReadOnlyList<SearchSource> sources,
        CancellationToken cancellationToken = default)
    {
        LastAnswer = answer;
        Result<RagEvaluation> result = FailWith is null
            ? new RagEvaluation
            {
                Faithfulness = 0.9,
                AnswerRelevancy = 0.8,
                Rationale = "test",
                JudgeModel = "fake-judge"
            }
            : FailWith;
        return Task.FromResult(result);
    }
}

public sealed class EvaluationServiceTests
{
    private static FakeSearchPipeline SuccessfulPipeline() => new()
    {
        NextResult = new SearchResponse
        {
            Answer = "üretilen cevap",
            ActiveLayer = 2,
            LayerName = "Lightweight Embedding",
            DocumentCount = 60,
            Sources = Array.Empty<SearchSource>()
        }
    };

    [Fact]
    public async Task Combines_search_result_with_judge_scores()
    {
        var evaluator = new FakeRagEvaluator();
        var service = new EvaluationService(SuccessfulPipeline(), evaluator);

        var result = await service.EvaluateAsync(new EvaluateRequest { Query = "soru" });

        Assert.True(result.IsSuccess);
        Assert.Equal("üretilen cevap", result.Value.Search.Answer);
        Assert.Equal(0.9, result.Value.Evaluation.Faithfulness);
        Assert.Equal("üretilen cevap", evaluator.LastAnswer);
    }

    [Fact]
    public async Task Search_failure_short_circuits_without_judge_call()
    {
        var evaluator = new FakeRagEvaluator();
        var pipeline = new FakeSearchPipeline { NextResult = Error.Failure("arama hatası", code: "search.failed") };
        var service = new EvaluationService(pipeline, evaluator);

        var result = await service.EvaluateAsync(new EvaluateRequest { Query = "soru" });

        Assert.True(result.IsFailure);
        Assert.Equal("search.failed", result.Error.Code);
        Assert.Null(evaluator.LastAnswer);
    }

    [Fact]
    public async Task Judge_failure_propagates_as_result_error()
    {
        var evaluator = new FakeRagEvaluator
        {
            FailWith = Error.External("hakem erişilemez", code: "evaluation.failed")
        };
        var service = new EvaluationService(SuccessfulPipeline(), evaluator);

        var result = await service.EvaluateAsync(new EvaluateRequest { Query = "soru" });

        Assert.True(result.IsFailure);
        Assert.Equal("evaluation.failed", result.Error.Code);
    }
}
