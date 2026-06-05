using System.Security.Cryptography;
using System.Text;

namespace ColdStart.VectorRag.Chunking;

/// <summary>
/// Belge içeriğinin değişip değişmediğini tespit etmek için kullanılan
/// deterministik özet (SHA-256). Qdrant payload'ında saklanır; store'daki
/// güncel içerikle karşılaştırılarak bayat indeks kayıtları bulunur.
/// </summary>
public static class ContentHasher
{
    /// <summary>Verilen metnin SHA-256 özetini küçük harfli hex string olarak döner.</summary>
    public static string Compute(string content)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
