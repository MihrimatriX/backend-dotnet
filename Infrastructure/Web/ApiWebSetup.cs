using System.Globalization;
using System.Threading.RateLimiting;
using EcommerceBackend.Application.Common;
using EcommerceBackend.Infrastructure.Logging;
using EcommerceBackend.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcommerceBackend.Infrastructure.Web;

/// <summary>
/// HTTP katmanının ortak kuralları (§1.4): CORS, IP başına hız limiti ve gövdesiz hata durumları için JSON zarf.
/// </summary>
public static class ApiWebSetup
{
    public const string CorsPolicyName = "ApiCors";

    private static readonly string[] DefaultCorsOrigins =
    [
        "http://localhost:3000",
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "http://localhost:8080",
    ];

    public static IMvcBuilder ConfigureApiBehavior(this IMvcBuilder mvc) =>
        mvc.ConfigureApiBehaviorOptions(options =>
            options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Create);

    /// <summary>
    /// İzinli origin'ler: <c>ALLOWED_ORIGINS</c> (virgülle ayrılmış) → <c>Cors:AllowedOrigins</c> → varsayılan liste.
    /// <c>X-Correlation-Id</c> tarayıcıya açılır.
    /// </summary>
    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = ResolveCorsOrigins(configuration);
        return services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders(CorrelationIdConstants.HeaderName)));
    }

    private static string[] ResolveCorsOrigins(IConfiguration configuration)
    {
        var fromEnv = configuration["ALLOWED_ORIGINS"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (fromEnv is { Length: > 0 })
            return fromEnv;

        var fromConfig = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        return fromConfig is { Length: > 0 } ? fromConfig : DefaultCorsOrigins;
    }

    /// <summary>
    /// IP başına sabit pencere (varsayılan dakikada 300). Sağlık, metrik ve Swagger uçları muaftır.
    /// Aşımda 429 <c>RATE_LIMITED</c> zarfı ve <c>Retry-After</c> başlığı döner.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limit = (configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>() ?? new RateLimitOptions())
            .RequestsPerMinute;

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                limit <= 0 || IsRateLimitExempt(context.Request.Path)
                    ? RateLimitPartition.GetNoLimiter("exempt")
                    : RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = limit,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true,
                        }));
            options.OnRejected = async (context, _) =>
            {
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var wait)
                    ? (int)Math.Ceiling(wait.TotalSeconds)
                    : 60;
                context.HttpContext.Response.Headers.RetryAfter = Math.Max(1, retryAfter).ToString(CultureInfo.InvariantCulture);
                await ApiErrorResponse.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status429TooManyRequests,
                    ErrorCodes.RateLimited,
                    ErrorMessages.RateLimited);
            };
        });
    }

    private static bool IsRateLimitExempt(PathString path) =>
        path.Equals("/health", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/actuator", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/api/health", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/api/metrics", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/health-ui", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gövdesiz 4xx/5xx cevapları (bilinmeyen rota, 405, 415 …) <c>/api/**</c> altında JSON zarfa çevirir.
    /// </summary>
    public static IApplicationBuilder UseApiStatusCodeEnvelopes(this IApplicationBuilder app) =>
        app.UseStatusCodePages(async context =>
        {
            var http = context.HttpContext;
            if (!http.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                return;

            var status = http.Response.StatusCode;
            var (code, message) = status switch
            {
                StatusCodes.Status401Unauthorized => (ErrorCodes.Unauthorized, ErrorMessages.Unauthorized),
                StatusCodes.Status403Forbidden => (ErrorCodes.Forbidden, ErrorMessages.Forbidden),
                StatusCodes.Status404NotFound => (ErrorCodes.NotFound, ErrorMessages.NotFound),
                StatusCodes.Status405MethodNotAllowed => (ErrorCodes.MethodNotAllowed, ErrorMessages.MethodNotAllowed),
                StatusCodes.Status415UnsupportedMediaType => (ErrorCodes.UnsupportedMediaType, ErrorMessages.UnsupportedMediaType),
                StatusCodes.Status429TooManyRequests => (ErrorCodes.RateLimited, ErrorMessages.RateLimited),
                >= 500 => (ErrorCodes.InternalError, ErrorMessages.InternalError),
                _ => (ErrorCodes.BadRequest, ErrorMessages.BadRequest),
            };

            await ApiErrorResponse.WriteAsync(http, status, code, message);
        });
}
