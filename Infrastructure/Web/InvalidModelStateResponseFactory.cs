using System.Text.Json;
using EcommerceBackend.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcommerceBackend.Infrastructure.Web;

/// <summary>
/// [ApiController] model doğrulama cevabı (§1.2):
/// <list type="bullet">
/// <item>Okunamayan JSON, tip uyuşmazlığı, eksik/okunamayan gövde veya rota/sorgu parametresi → 400 <c>BAD_REQUEST</c>.</item>
/// <item>DataAnnotations hataları → 400 <c>VALIDATION_ERROR</c>; <c>errors</c> camelCase alan adına göre,
/// <c>message</c> alfabetik ilk alanın ilk mesajı (deterministik).</item>
/// </list>
/// </summary>
public static class InvalidModelStateResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var parameterNames = context.ActionDescriptor.Parameters
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var errors = new SortedDictionary<string, string[]>(StringComparer.Ordinal);
        var malformed = false;

        foreach (var (key, entry) in context.ModelState)
        {
            if (entry.ValidationState != ModelValidationState.Invalid || entry.Errors.Count == 0)
                continue;

            // "$..." = JSON okuma/tip hatası, "" = boş gövde, parametre adı = bağlanamayan gövde/rota/sorgu değeri.
            if (key.Length == 0 || key.StartsWith('$') || parameterNames.Contains(key)
                || entry.Errors.Any(e => e.Exception is not null))
            {
                malformed = true;
                continue;
            }

            errors[ToCamelCasePath(key)] = entry.Errors
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)
                .ToArray();
        }

        var body = malformed || errors.Count == 0
            ? ApiErrorResponse.Create(context.HttpContext, ErrorCodes.BadRequest, ErrorMessages.BadRequest)
            : ApiErrorResponse.Create(context.HttpContext, ErrorCodes.ValidationError, errors.First().Value[0], errors);

        return new BadRequestObjectResult(body);
    }

    /// <summary><c>Items[0].ProductId</c> → <c>items[0].productId</c>.</summary>
    private static string ToCamelCasePath(string key) =>
        string.Join('.', key.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
