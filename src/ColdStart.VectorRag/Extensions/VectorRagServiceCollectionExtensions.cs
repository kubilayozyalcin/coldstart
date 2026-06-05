using ColdStart.Core.Abstractions;
using ColdStart.VectorRag.Chunking;
using ColdStart.VectorRag.Evaluation;
using ColdStart.VectorRag.Generation;
using ColdStart.VectorRag.Options;
using ColdStart.VectorRag.Qdrant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ColdStart.VectorRag.Extensions;

/// <summary>
/// Layer 3 (Vector RAG) servis kayıtları: Qdrant client ayarları, chunking
/// stratejisi, Semantic Kernel tabanlı cevap üretici ve arama katmanının kendisi.
/// <see cref="IEmbeddingService"/> ve OpenAI ayarları Layer 2 kaydından
/// (<c>AddEmbeddingSearch</c>) gelir; iki katman aynı embedding uzayını paylaşır.
/// </summary>
public static class VectorRagServiceCollectionExtensions
{
    /// <summary>
    /// Qdrant ve VectorRag ayarlarını bağlar; <see cref="IVectorStore"/>,
    /// <see cref="IDocumentChunker"/>, <see cref="IAnswerGenerator"/> ve
    /// Layer 3 <see cref="ISearchLayer"/> kayıtlarını yapar.
    /// </summary>
    public static IServiceCollection AddVectorRag(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<QdrantOptions>(configuration.GetSection(QdrantOptions.SectionName));
        services.Configure<VectorRagOptions>(configuration.GetSection(VectorRagOptions.SectionName));

        services.AddSingleton<IDocumentChunker, FixedSizeChunker>();
        services.AddSingleton<IVectorStore, QdrantVectorStore>();
        services.AddSingleton<IAnswerGenerator, SemanticKernelAnswerGenerator>();
        services.AddSingleton<IRagEvaluator, LlmRagEvaluator>();
        services.AddSingleton<ISearchLayer, VectorRagSearch>();
        return services;
    }
}
