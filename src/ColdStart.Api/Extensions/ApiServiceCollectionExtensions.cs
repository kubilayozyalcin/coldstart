using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ColdStart.Api.Extensions;

/// <summary>
/// API sunum katmanının servis kayıtları. <c>Program.cs</c> doğrudan bu
/// metotları çağırır.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Controller'ları, JSON serileştirmeyi ve OpenAPI/Swagger dökümantasyonunu
    /// kaydeder. XML doc dosyaları otomatik olarak Swagger'a iliştirilir.
    /// </summary>
    public static IServiceCollection AddApiPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(o => o.LowercaseUrls = true);

        // Rate limiter: aynı IP'den dakikada 120 istek. Demo için makul üst sınır
        // (status polling 2 sn'de bir → 30/dk; arama + ingest için ~90 istek hareket
        // alanı). OpenAI fatura/quota'yı koruyor; tek IP'den DoS denemelerini yavaşlatıyor.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                string partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 120,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
        });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ColdStart API",
                Version = "v1",
                Description =
                    "Sparse Knowledge Environments için Adaptif Hibrit RAG. " +
                    "Üç katmanlı (BM25 / Lightweight Embedding / Vector RAG) " +
                    "pipeline'ın HTTP cephesi."
            });

            string baseDirectory = AppContext.BaseDirectory;
            foreach (string xmlFile in Directory.EnumerateFiles(
                         baseDirectory, "ColdStart.*.xml", SearchOption.TopDirectoryOnly))
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }
        });

        return services;
    }
}
