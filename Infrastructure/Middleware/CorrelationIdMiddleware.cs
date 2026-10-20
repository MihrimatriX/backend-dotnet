using System.Text.RegularExpressions;
using EcommerceBackend.Infrastructure.Logging;
using Serilog.Context;

namespace EcommerceBackend.Infrastructure.Middleware;

/// <summary>
/// İstek boyunca CorrelationId üretir veya istemci başlığından alır; Serilog ve yanıt başlığına yazar (§1.4).
/// İstemci değeri en fazla 128 karakter ve <c>[A-Za-z0-9._:-]</c> ise aynen kullanılır, aksi halde yenisi üretilir.
/// </summary>
public sealed partial class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var id = ResolveCorrelationId(context);
        context.Items[CorrelationIdConstants.HttpContextItemKey] = id;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdConstants.HeaderName] = id;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", id))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        var raw = context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault()?.Trim();
        return !string.IsNullOrEmpty(raw) && AllowedCorrelationId().IsMatch(raw)
            ? raw
            : Guid.NewGuid().ToString("N");
    }

    [GeneratedRegex("^[A-Za-z0-9._:-]{1,128}$")]
    private static partial Regex AllowedCorrelationId();
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder) =>
        builder.UseMiddleware<CorrelationIdMiddleware>();

    public static string GetCorrelationId(this HttpContext context) =>
        context.Items[CorrelationIdConstants.HttpContextItemKey] as string ?? context.TraceIdentifier;
}
