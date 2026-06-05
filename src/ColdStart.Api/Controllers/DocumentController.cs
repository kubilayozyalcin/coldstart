using ColdStart.Core.Abstractions;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace ColdStart.Api.Controllers;

/// <summary>
/// Belge yaşam döngüsünü yöneten controller. Tüm iş mantığı
/// <see cref="IDocumentIngestService"/>'e devredilir.
/// </summary>
public sealed class DocumentController : ApiController
{
    private readonly IDocumentIngestService _ingest;
    private readonly IDocumentMaintenanceService _maintenance;
    private readonly IDocumentQueryService _query;

    /// <summary>DI üzerinden ingest, maintenance ve query servislerini alır.</summary>
    public DocumentController(
        IDocumentIngestService ingest,
        IDocumentMaintenanceService maintenance,
        IDocumentQueryService query)
    {
        _ingest = ingest;
        _maintenance = maintenance;
        _query = query;
    }

    /// <summary>Tüm belgelerin özet listesini döner (en yeni başta).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<DocumentSummary>> result = await _query.ListAsync(cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Yeni bir belge ekler ya da aynı kimlikle var olanı günceller.
    /// İşlem sonrası güncel <c>documentCount</c> ve aktif katman döner.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IngestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ingest(
        [FromBody] IngestRequest request,
        CancellationToken cancellationToken)
    {
        Result<IngestResponse> result = await _ingest.IngestAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Verilen kimliğe sahip belgeyi siler.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result result = await _maintenance.DeleteAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Tüm belgeleri siler. Demo akışında Layer 1 → 2 → 3 geçişini sıfırdan
    /// göstermek için kullanılır. Silinen kayıt adedini döner.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        Result<int> result = await _maintenance.ClearAsync(cancellationToken);
        return ToActionResult(result);
    }
}
