using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

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
        var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey] as string
            ?? context.TraceIdentifier;
        var traceForClient = correlationId;

        if (exception is OperationCanceledException)
        {
            _logger.LogInformation(
                "İstek iptal edildi veya zaman aşımı: {Method} {Path} — {CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value,
                correlationId);
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
                context.Response.ContentType = "application/json";
                var body = new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "İstek iptal edildi.",
                    ErrorCode = "REQUEST_CANCELLED",
                    TraceId = traceForClient,
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            }
            return;
        }

        var (status, response, logLevel) = MapException(exception, traceForClient);

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
            _logger.LogWarning(
                "Yanıt başladığı için hata gövdesi yazılamıyor. {CorrelationId}",
                correlationId);
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private (int Status, BaseResponseDto<object> Body, LogLevel Level) MapException(
        Exception exception,
        string traceForClient)
    {
        var showDetails = _env.IsDevelopment();

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                return (
                    (int)HttpStatusCode.BadRequest,
                    new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Geçersiz istek.",
                        Error = showDetails ? exception.Message : null,
                        ErrorCode = "BAD_REQUEST",
                        TraceId = traceForClient,
                    },
                    LogLevel.Warning);

            case UnauthorizedAccessException:
                return (
                    (int)HttpStatusCode.Unauthorized,
                    new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Yetkisiz erişim.",
                        Error = showDetails ? exception.Message : null,
                        ErrorCode = "UNAUTHORIZED",
                        TraceId = traceForClient,
                    },
                    LogLevel.Warning);

            case KeyNotFoundException:
                return (
                    (int)HttpStatusCode.NotFound,
                    new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Kaynak bulunamadı.",
                        Error = showDetails ? exception.Message : null,
                        ErrorCode = "NOT_FOUND",
                        TraceId = traceForClient,
                    },
                    LogLevel.Information);

            case InvalidOperationException:
                return (
                    (int)HttpStatusCode.BadRequest,
                    new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "İşlem geçersiz.",
                        Error = showDetails ? exception.Message : null,
                        ErrorCode = "INVALID_OPERATION",
                        TraceId = traceForClient,
                    },
                    LogLevel.Warning);

            case DbUpdateConcurrencyException:
                return (
                    (int)HttpStatusCode.Conflict,
                    new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Kayıt başka bir işlem tarafından değiştirildi. Tekrar deneyin.",
                        Error = showDetails ? exception.Message : null,
                        ErrorCode = "CONCURRENCY_CONFLICT",
                        TraceId = traceForClient,
                    },
                    LogLevel.Warning);

            case DbUpdateException dbEx:
                return (
                    (int)HttpStatusCode.Conflict,
                    new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Veritabanı kısıtı veya güncelleme hatası.",
                        Error = showDetails ? dbEx.InnerException?.Message ?? dbEx.Message : null,
                        ErrorCode = "DATABASE_UPDATE",
                        TraceId = traceForClient,
                    },
                    LogLevel.Error);

            default:
                return (
                    (int)HttpStatusCode.InternalServerError,
                    new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Beklenmeyen bir hata oluştu. Destek için traceId değerini iletin.",
                        Error = showDetails ? exception.Message : null,
                        ErrorCode = "INTERNAL_ERROR",
                        TraceId = traceForClient,
                    },
                    LogLevel.Error);
        }
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder) =>
        builder.UseMiddleware<GlobalExceptionMiddleware>();
}
