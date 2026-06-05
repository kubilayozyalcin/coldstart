using ColdStart.Core.Entities;
using ColdStart.Core.Results;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Retrieve edilen bağlamdan doğal dil cevabı üreten bileşenin soyutlaması
/// (RAG'in "Generate" adımı). Layer 3'te Semantic Kernel + GPT-4o-mini
/// uygulaması kullanılır; testlerde mock'lanır. İleri çalışmada local model
/// değişimi bu abstraction üzerinden mümkündür.
/// </summary>
public interface IAnswerGenerator
{
    /// <summary>Cevap üretiminde kullanılan modelin adı (ör. <c>gpt-4o-mini</c>).</summary>
    string ModelName { get; }

    /// <summary>
    /// Kullanıcı sorgusunu ve retrieve edilen chunk'ları LLM'e bağlam olarak verip
    /// yalnızca bu bağlama dayanan bir cevap üretir.
    /// </summary>
    Task<Result<string>> GenerateAsync(
        string query,
        IReadOnlyList<DocumentChunk> contextChunks,
        CancellationToken cancellationToken = default);
}
