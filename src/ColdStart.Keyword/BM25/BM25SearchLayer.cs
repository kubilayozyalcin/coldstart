using ColdStart.Core.Abstractions;
using ColdStart.Core.Entities;
using ColdStart.Core.Models;
using ColdStart.Core.Results;
using ColdStart.Core.Services;
using ColdStart.Keyword.Tokenization;
using Microsoft.Extensions.Options;

namespace ColdStart.Keyword.BM25;

/// <summary>
/// Klasik Okapi BM25 sıralamasını uygulayan kural-tabanlı arama katmanı.
/// Hiçbir LLM çağrısı yapmaz; küçük corpus'larda (documentCount &lt; 50)
/// deterministik baseline davranışı sağlar. Her arama isteğinde indeks
/// uçtan uca yeniden hesaplanır — Faz 1 hedef corpus boyutu (max ~200)
/// için yeterli; gerekirse cache'e taşınabilir.
/// </summary>
public sealed class BM25SearchLayer : ISearchLayer
{
    private readonly IDocumentStore _store;
    private readonly TurkishTokenizer _tokenizer;
    private readonly BM25Parameters _parameters;

    /// <summary>DI üzerinden bağımlılıkları alır.</summary>
    public BM25SearchLayer(
        IDocumentStore store,
        TurkishTokenizer tokenizer,
        IOptions<BM25Parameters> parameters)
    {
        _store = store;
        _tokenizer = tokenizer;
        _parameters = parameters.Value;
    }

    /// <inheritdoc />
    public int LayerNumber => 1;

    /// <inheritdoc />
    public string LayerName => "Keyword (BM25)";

    /// <inheritdoc />
    public async Task<Result<SearchResponse>> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Document> documents = await _store.GetAllAsync(cancellationToken);

        if (documents.Count == 0)
            return EmptyResponse("Sistemde henüz belge bulunmuyor; cevap üretilemiyor.", documentCount: 0);

        IReadOnlyList<string> queryTokens = _tokenizer.Tokenize(request.Query);
        if (queryTokens.Count == 0)
            return Error.Validation("Sorguda kullanılabilir terim bulunamadı.");

        IReadOnlyList<TokenizedDocument> tokenized = documents
            .Select(d => new TokenizedDocument(d, _tokenizer.Tokenize(d.Content)))
            .Where(t => t.Tokens.Count > 0)
            .ToArray();

        if (tokenized.Count == 0)
            return EmptyResponse("İçerikleri tokenize edilebilen belge bulunamadı.", documents.Count);

        double averageLength = tokenized.Average(t => (double)t.Tokens.Count);
        Dictionary<string, int> documentFrequency = ComputeDocumentFrequency(tokenized);

        int topK = Math.Clamp(request.TopK, 1, 50);
        List<ScoredDocument> scored = ScoreDocuments(
            tokenized,
            queryTokens,
            documentFrequency,
            averageLength);

        IReadOnlyList<ScoredDocument> top = scored
            .OrderByDescending(s => s.Score)
            .Take(topK)
            .ToArray();

        if (top.Count == 0)
            return EmptyResponse("Sorguyla eşleşen belge bulunamadı.", documents.Count);

        IReadOnlyList<SearchSource> sources = top
            .Select(t => new SearchSource
            {
                DocumentId = t.Document.Id,
                Snippet = ExtractiveAnswerComposer.MakeSnippet(t.Document.Content),
                Score = t.Score
            })
            .ToArray();

        return new SearchResponse
        {
            Answer = ExtractiveAnswerComposer.Compose(
                top.Select(t => (t.Document.Content, t.Score)).ToArray()),
            ActiveLayer = LayerNumber,
            LayerName = LayerName,
            DocumentCount = documents.Count,
            Sources = sources
        };
    }

    private static Dictionary<string, int> ComputeDocumentFrequency(IReadOnlyList<TokenizedDocument> tokenized)
    {
        Dictionary<string, int> df = new(StringComparer.Ordinal);
        foreach (TokenizedDocument doc in tokenized)
        {
            HashSet<string> seen = new(StringComparer.Ordinal);
            foreach (string token in doc.Tokens)
            {
                if (seen.Add(token))
                    df[token] = df.GetValueOrDefault(token) + 1;
            }
        }
        return df;
    }

    private List<ScoredDocument> ScoreDocuments(
        IReadOnlyList<TokenizedDocument> tokenized,
        IReadOnlyList<string> queryTokens,
        IReadOnlyDictionary<string, int> documentFrequency,
        double averageLength)
    {
        int corpusSize = tokenized.Count;
        IReadOnlyList<string> distinctQuery = queryTokens.Distinct().ToArray();

        List<ScoredDocument> scored = new(corpusSize);
        foreach (TokenizedDocument doc in tokenized)
        {
            Dictionary<string, int> termFrequency = new(StringComparer.Ordinal);
            foreach (string token in doc.Tokens)
                termFrequency[token] = termFrequency.GetValueOrDefault(token) + 1;

            int documentLength = doc.Tokens.Count;
            double score = 0d;

            foreach (string queryTerm in distinctQuery)
            {
                if (!termFrequency.TryGetValue(queryTerm, out int termFreq) || termFreq == 0)
                    continue;
                if (!documentFrequency.TryGetValue(queryTerm, out int df) || df == 0)
                    continue;

                double idf = Math.Log(((corpusSize - df) + 0.5) / (df + 0.5) + 1);
                double numerator = termFreq * (_parameters.K1 + 1);
                double denominator = termFreq + _parameters.K1 *
                    (1 - _parameters.B + _parameters.B * (documentLength / averageLength));

                score += idf * (numerator / denominator);
            }

            if (score > 0d)
                scored.Add(new ScoredDocument(doc.Document, score));
        }

        return scored;
    }

    private static SearchResponse EmptyResponse(string message, int documentCount) => new()
    {
        Answer = message,
        ActiveLayer = 1,
        LayerName = "Keyword (BM25)",
        DocumentCount = documentCount,
        Sources = Array.Empty<SearchSource>()
    };

    private readonly record struct TokenizedDocument(Document Document, IReadOnlyList<string> Tokens);

    private readonly record struct ScoredDocument(Document Document, double Score);
}
