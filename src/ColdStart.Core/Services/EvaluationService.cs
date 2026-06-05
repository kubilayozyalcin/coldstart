using System.Diagnostics;
using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Results;

namespace ColdStart.Core.Services;

/// <summary>
/// <see cref="IEvaluationService"/>'in varsayılan uygulaması. Aramayı
/// <see cref="ISearchPipeline"/>'a, puanlamayı <see cref="IRagEvaluator"/>'a
/// devreder; iki sonucu tek cevapta birleştirir.
/// </summary>
public sealed class EvaluationService : IEvaluationService
{
    private readonly ISearchPipeline _pipeline;
    private readonly IRagEvaluator _evaluator;

    /// <summary>DI üzerinden bağımlılıkları alır.</summary>
    public EvaluationService(ISearchPipeline pipeline, IRagEvaluator evaluator)
    {
        _pipeline = pipeline;
        _evaluator = evaluator;
    }

    /// <inheritdoc />
    public async Task<Result<EvaluateResponse>> EvaluateAsync(
        EvaluateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Error.Validation("Sorgu metni boş olamaz.");

        long start = Stopwatch.GetTimestamp();
        Result<SearchResponse> search = await _pipeline.SearchAsync(
            new SearchRequest { Query = request.Query, TopK = request.TopK },
            cancellationToken);
        long searchDurationMs = (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        if (search.IsFailure)
            return Result.Failure<EvaluateResponse>(search.Error);

        Result<RagEvaluation> evaluation = await _evaluator.EvaluateAsync(
            request.Query,
            search.Value.Answer,
            search.Value.Sources,
            cancellationToken);

        if (evaluation.IsFailure)
            return Result.Failure<EvaluateResponse>(evaluation.Error);

        return new EvaluateResponse
        {
            Search = search.Value,
            Evaluation = evaluation.Value,
            SearchDurationMs = searchDurationMs
        };
    }
}
