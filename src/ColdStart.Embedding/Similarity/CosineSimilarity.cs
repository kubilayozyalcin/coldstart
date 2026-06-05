namespace ColdStart.Embedding.Similarity;

/// <summary>
/// İki vektör arasındaki cosine similarity'yi hesaplayan saf fonksiyonel yardımcı.
/// İki vektörün eşit uzunlukta olduğu varsayılır; aksi halde
/// <see cref="ArgumentException"/> fırlatır.
/// </summary>
public static class CosineSimilarity
{
    /// <summary>
    /// İki vektör arasındaki cosine similarity'yi döner. Vektörlerden biri sıfır
    /// vektörü ise sonuç 0'dır.
    /// </summary>
    public static double Compute(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vektör boyutları eşleşmiyor.");

        double dot = 0d;
        double normA = 0d;
        double normB = 0d;

        for (int i = 0; i < a.Length; i++)
        {
            double ai = a[i];
            double bi = b[i];
            dot += ai * bi;
            normA += ai * ai;
            normB += bi * bi;
        }

        if (normA == 0d || normB == 0d)
            return 0d;

        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
