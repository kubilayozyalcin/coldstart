namespace ColdStart.VectorRag.Options;

/// <summary>
/// Qdrant bağlantı ayarları. <c>Qdrant</c> bölümünden okunur. Varsayılanlar,
/// projedeki <c>docker-compose.yml</c> ile ayağa kalkan local instance'a göredir.
/// </summary>
public sealed class QdrantOptions
{
    /// <summary>Konfigürasyon bölümünün adı (<c>appsettings.json</c>).</summary>
    public const string SectionName = "Qdrant";

    /// <summary>Qdrant sunucusunun adresi.</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>gRPC port'u (docker-compose'ta 6334 olarak yayınlanır).</summary>
    public int GrpcPort { get; set; } = 6334;

    /// <summary>TLS kullanılsın mı? Local docker instance'ında kapalıdır.</summary>
    public bool UseTls { get; set; }

    /// <summary>Chunk'ların yazılacağı koleksiyonun adı.</summary>
    public string CollectionName { get; set; } = "coldstart_chunks";
}
