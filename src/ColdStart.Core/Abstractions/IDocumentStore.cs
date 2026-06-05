using ColdStart.Core.Entities;

namespace ColdStart.Core.Abstractions;

/// <summary>
/// Belge persistans katmanının soyutlaması. Faz 1 + 2'de bellekte (in-memory)
/// bir uygulamaya sahip olur; Faz 3'te Qdrant ile senkronize edilir.
/// </summary>
public interface IDocumentStore
{
    /// <summary>Store'daki toplam belge sayısını döner. PipelineRouter geçişlerinde kullanılır.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Tüm belgelerin tekilleştirilmiş ve thread-safe biçimde kopyasını döner.</summary>
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Verilen kimliğe sahip belgeyi döner; bulunamadıysa <c>null</c>.</summary>
    Task<Document?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Belgeyi ekler ya da var olanı (aynı kimlikle) tamamen değiştirir.</summary>
    Task UpsertAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>Verilen kimliğe sahip belgeyi siler. Silindiyse <c>true</c>, bulunamadıysa <c>false</c> döner.</summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Tüm belgeleri siler. Geriye silinen kayıt adedini döner. Demo / test için kullanışlıdır.</summary>
    Task<int> ClearAsync(CancellationToken cancellationToken = default);
}
