using EcommerceBackend.Infrastructure.Logging;
using Serilog.Context;

namespace EcommerceBackend.Infrastructure.Middleware;

/// <summary>
/// İstek boyunca CorrelationId üretir veya istemci başlığından alır; Serilog ve yanıt başlığına yazar.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var id = ResolveCorrelationId(context);
        context.Items[CorrelationIdConstants.HttpContextItemKey] = id;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(CorrelationIdConstants.HeaderName, id);
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", id))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out var fromHeader))
        {
            var raw = fromHeader.FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(raw) && raw.Length <= 128)
                return raw;
        }

        return Guid.NewGuid().ToString("N");
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder) =>
        builder.UseMiddleware<CorrelationIdMiddleware>();
}
