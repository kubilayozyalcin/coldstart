namespace ColdStart.Api.Hosting;

/// <summary>
/// Proje veya repo kökündeki <c>.env</c> dosyasını okuyup satırlarını process
/// environment'ına yükler. Docker dışı çalıştırmada (<c>dotnet run</c>) API
/// anahtarının elle export edilmesini gereksiz kılar; Docker'da compose aynı
/// dosyayı zaten okuduğu ve dosya imaja kopyalanmadığı için yükleyici sessizce
/// atlanır. Mevcut environment değişkenleri hiçbir durumda ezilmez — gerçek
/// environment her zaman dosyaya göre önceliklidir.
/// </summary>
public static class DotEnvLoader
{
    /// <summary>Bilinen konumlardaki ilk <c>.env</c> dosyasını yükler.</summary>
    public static void Load()
    {
        string cwd = Directory.GetCurrentDirectory();
        string baseDir = AppContext.BaseDirectory;
        string[] candidates =
        {
            Path.Combine(cwd, ".env"),
            Path.GetFullPath(Path.Combine(cwd, "..", "..", ".env")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", ".env"))
        };

        string? file = candidates.FirstOrDefault(File.Exists);
        if (file is null) return;

        foreach (string rawLine in File.ReadAllLines(file))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            int separator = line.IndexOf('=');
            if (separator <= 0) continue;

            string name = line[..separator].Trim();
            string value = line[(separator + 1)..].Trim().Trim('"', '\'');
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, value);
        }
    }
}
