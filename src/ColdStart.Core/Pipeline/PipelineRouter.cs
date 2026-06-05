using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using Microsoft.Extensions.Options;

namespace ColdStart.Core.Pipeline;

/// <summary>
/// Adaptif RAG hattının kalbi. <see cref="IDocumentStore"/>'dan belge sayısını okur,
/// <see cref="PipelineOptions"/>'taki eşiklerle karşılaştırır ve isteği uygun
/// <see cref="ISearchLayer"/>'a yönlendirir.
/// </summary>
public sealed class PipelineRouter : ISearchPipeline
{
    private readonly IDocumentStore _store;
    private readonly IReadOnlyDictionary<int, ISearchLayer> _layersByNumber;
    private readonly PipelineOptions _options;

    /// <summary>DI üzerinden tüm kayıtlı <see cref="ISearchLayer"/>'ları ve eşik ayarlarını alır.</summary>
    public PipelineRouter(
        IDocumentStore store,
        IEnumerable<ISearchLayer> layers,
        IOptions<PipelineOptions> options)
    {
        _store = store;
        _layersByNumber = layers
            .GroupBy(l => l.LayerNumber)
            .ToDictionary(g => g.Key, g => g.First());
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result<SearchResponse>> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Error.Validation("Sorgu metni boş olamaz.");

        int documentCount = await _store.CountAsync(cancellationToken);
        ISearchLayer? layer = SelectLayer(documentCount);
        if (layer is null)
            return Error.Failure("Aktif edilebilecek bir arama katmanı bulunamadı.");

        return await layer.SearchAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<StatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        int documentCount = await _store.CountAsync(cancellationToken);
        ISearchLayer? active = SelectLayer(documentCount);

        (int? nextThreshold, int? untilNext) = active?.LayerNumber switch
        {
            1 => (_options.Layer2Threshold, Math.Max(0, _options.Layer2Threshold - documentCount)),
            2 => (_options.Layer3Threshold, Math.Max(0, _options.Layer3Threshold - documentCount)),
            _ => ((int?)null, (int?)null)
        };

        return new StatusResponse
        {
            DocumentCount = documentCount,
            ActiveLayer = active?.LayerNumber ?? 0,
            LayerName = active?.LayerName ?? "Yok",
            NextLayerThreshold = nextThreshold,
            DocumentsUntilNextLayer = untilNext
        };
    }

    /// <summary>Belge sayısına göre devreye girecek katmanı seçer.</summary>
    internal ISearchLayer? SelectLayer(int documentCount)
    {
        if (documentCount >= _options.Layer3Threshold && _layersByNumber.TryGetValue(3, out var l3))
            return l3;
        if (documentCount >= _options.Layer2Threshold && _layersByNumber.TryGetValue(2, out var l2))
            return l2;
        return _layersByNumber.TryGetValue(1, out var l1) ? l1 : null;
    }
}
