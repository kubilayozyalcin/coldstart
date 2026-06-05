using ColdStart.Experiments.Experiments;

// ColdStart deney koşucusu. Kullanım:
//   dotnet run --project experiments/ColdStart.Experiments -- transition
//   dotnet run --project experiments/ColdStart.Experiments -- activation [--with-llm]
//   dotnet run --project experiments/ColdStart.Experiments -- sensitivity [--with-llm]
//   dotnet run --project experiments/ColdStart.Experiments -- all [--with-llm]
// --with-llm: OpenAI çağrısı yapan ölçümleri (embedding/RAG arama + LLM-as-a-judge) açar.

string experiment = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "transition";
bool withLlm = args.Contains("--with-llm");
string repoRoot = FindRepoRoot();

Console.WriteLine($"ColdStart Experiments — deney: {experiment}, LLM: {(withLlm ? "açık" : "kapalı")}");
Console.WriteLine($"Repo kökü: {repoRoot}");
Console.WriteLine();

var outputs = new List<string>();

if (experiment is "transition" or "all")
    outputs.Add(await TransitionAccuracyExperiment.RunAsync(repoRoot));

if (experiment is "activation" or "all")
    outputs.Add(await ActivationTimeExperiment.RunAsync(repoRoot, withLlm));

if (experiment is "sensitivity" or "all")
    outputs.Add(await ThresholdSensitivityExperiment.RunAsync(repoRoot, withLlm));

if (outputs.Count == 0)
{
    Console.Error.WriteLine($"Bilinmeyen deney: '{experiment}'. Geçerli: transition | activation | sensitivity | all");
    return 1;
}

Console.WriteLine();
Console.WriteLine("Sonuç dosyaları:");
foreach (string path in outputs)
    Console.WriteLine($"  {path}");

return 0;

// Çalışma dizininden yukarı çıkarak ColdStart.sln'in bulunduğu kökü bulur;
// deney, hangi dizinden çalıştırılırsa çalıştırılsın data/ yollarını doğru çözer.
static string FindRepoRoot()
{
    DirectoryInfo? dir = new(AppContext.BaseDirectory);
    while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ColdStart.sln")))
        dir = dir.Parent;
    return dir?.FullName
           ?? throw new InvalidOperationException("ColdStart.sln bulunamadı; deney repo içinden çalıştırılmalı.");
}
