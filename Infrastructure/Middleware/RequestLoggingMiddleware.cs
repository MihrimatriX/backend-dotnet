using System.Diagnostics;
using EcommerceBackend.Infrastructure.Logging;

namespace EcommerceBackend.Infrastructure.Middleware;

/// <summary>
/// HTTP isteklerini süre, durum kodu ve korelasyon ile yapılandırılmış loglar.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private const int SlowRequestThresholdMs = 1500;
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (ShouldSkipResponseBuffering(path))
        {
            await InvokeWithoutBodyCapture(context);
            return;
        }

        var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey] as string ?? "-";
        var request = context.Request;
        var stopwatch = Stopwatch.StartNew();

        var originalBody = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;
            var status = context.Response.StatusCode;

            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["Method"] = request.Method,
                ["Path"] = path,
                ["StatusCode"] = status,
                ["ElapsedMs"] = elapsed,
                ["ClientIp"] = GetClientIpAddress(context),
            }))
            {
                if (elapsed >= SlowRequestThresholdMs)
                {
                    _logger.LogWarning(
                        "Yavaş istek: {Method} {Path} → {StatusCode} ({ElapsedMs} ms)",
                        request.Method,
                        path,
                        status,
                        elapsed);
                }
                else if (status >= 500)
                {
                    _logger.LogError(
                        "HTTP {StatusCode} {Method} {Path} ({ElapsedMs} ms)",
                        status,
                        request.Method,
                        path,
                        elapsed);
                }
                else if (status >= 400)
                {
                    _logger.LogWarning(
                        "HTTP {StatusCode} {Method} {Path} ({ElapsedMs} ms)",
                        status,
                        request.Method,
                        path,
                        elapsed);
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP {StatusCode} {Method} {Path} ({ElapsedMs} ms)",
                        status,
                        request.Method,
                        path,
                        elapsed);
                }
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
    }

    private async Task InvokeWithoutBodyCapture(HttpContext context)
    {
        var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey] as string ?? "-";
        var request = context.Request;
        var path = context.Request.Path.Value ?? string.Empty;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;
            var status = context.Response.StatusCode;

            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["Method"] = request.Method,
                ["Path"] = path,
                ["StatusCode"] = status,
                ["ElapsedMs"] = elapsed,
            }))
            {
                _logger.LogDebug(
                    "HTTP {StatusCode} {Method} {Path} ({ElapsedMs} ms) [no-buffer]",
                    status,
                    request.Method,
                    path,
                    elapsed);
            }
        }
    }

    private static bool ShouldSkipResponseBuffering(string path) =>
        path.StartsWith("/api/health", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/health", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/health-ui", StringComparison.OrdinalIgnoreCase);

    private static string GetClientIpAddress(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
            return xForwardedFor.Split(',')[0].Trim();

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
            return xRealIp;

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
