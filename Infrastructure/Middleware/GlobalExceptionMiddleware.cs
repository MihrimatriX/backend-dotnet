using System.Text.Json;
using EcommerceBackend.Application.Common;
using EcommerceBackend.Infrastructure.Web;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Middleware;

/// <summary>
/// İşlenmeyen istisnaları sözleşme zarfına çevirir (§1.2). <c>error</c> alanı yalnızca geliştirmede doldurulur.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.GetCorrelationId();

        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "İstek iptal edildi: {Method} {Path} — {CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value,
                correlationId);
            if (!context.Response.HasStarted)
            {
                await ApiErrorResponse.WriteAsync(
                    context,
                    StatusCodes.Status499ClientClosedRequest,
                    ErrorCodes.RequestCancelled,
                    "İstek iptal edildi.");
            }
            return;
        }

        var (status, code, message, logLevel) = Map(exception);

        _logger.Log(
            logLevel,
            exception,
            "İşlenmeyen hata: {ExceptionType} — {Method} {Path} — {CorrelationId} — {Message}",
            exception.GetType().Name,
            context.Request.Method,
            context.Request.Path.Value,
            correlationId,
            exception.Message);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Yanıt başladığı için hata gövdesi yazılamıyor. {CorrelationId}", correlationId);
            return;
        }

        var detail = _env.IsDevelopment() ? (exception.InnerException ?? exception).Message : null;
        // Güvenlik başlıkları korunur; yalnızca yarım kalmış (tamponlanmış) gövde atılır.
        if (context.Response.Body.CanSeek)
            context.Response.Body.SetLength(0);
        context.Response.Headers.ContentLength = null;
        await ApiErrorResponse.WriteAsync(context, status, code, message, detail);
    }

    private static (int Status, string Code, string Message, LogLevel Level) Map(Exception exception) => exception switch
    {
        BadHttpRequestException bad => (bad.StatusCode, ErrorCodes.BadRequest, ErrorMessages.BadRequest, LogLevel.Warning),
        JsonException or FormatException or ArgumentException =>
            (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest, ErrorMessages.BadRequest, LogLevel.Warning),
        UnauthorizedAccessException =>
            (StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, ErrorMessages.Unauthorized, LogLevel.Warning),
        KeyNotFoundException =>
            (StatusCodes.Status404NotFound, ErrorCodes.NotFound, ErrorMessages.NotFound, LogLevel.Information),
        DbUpdateConcurrencyException =>
            (StatusCodes.Status409Conflict, ErrorCodes.Conflict, ErrorMessages.Conflict, LogLevel.Warning),
        DbUpdateException =>
            (StatusCodes.Status409Conflict, ErrorCodes.Conflict, "Kayıt veritabanı kısıtı nedeniyle kaydedilemedi.", LogLevel.Error),
        _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError, ErrorMessages.InternalError, LogLevel.Error),
    };
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder) =>
        builder.UseMiddleware<GlobalExceptionMiddleware>();
}
