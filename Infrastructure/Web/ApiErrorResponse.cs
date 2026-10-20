using System.Text.Json;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Serialization;
using EcommerceBackend.Infrastructure.Middleware;

namespace EcommerceBackend.Infrastructure.Web;

/// <summary>
/// Çerçeve seviyesindeki hata zarfları (401/403/404/429/500/doğrulama): <c>traceId</c> = <c>X-Correlation-Id</c> (§1.1).
/// </summary>
public static class ApiErrorResponse
{
    public static BaseResponseDto<object> Create(
        HttpContext context,
        string errorCode,
        string message,
        IDictionary<string, string[]>? errors = null,
        string? error = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode,
        Errors = errors,
        Error = error,
        TraceId = context.GetCorrelationId(),
    };

    public static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message,
        string? error = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            Create(context, errorCode, message, error: error),
            ApiJson.Options,
            context.RequestAborted);
    }
}
