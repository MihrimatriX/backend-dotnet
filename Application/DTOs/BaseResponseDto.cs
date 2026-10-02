using System.Text.Json.Serialization;

namespace EcommerceBackend.Application.DTOs
{
    /// <summary>
    /// Ortak API zarfı (docs/API_CONTRACT.md §1.1). <c>success</c> ve <c>message</c> her zaman yazılır;
    /// <c>data</c>, <c>error</c>, <c>errorCode</c>, <c>errors</c>, <c>traceId</c> null ise yazılmaz
    /// (<c>data</c> kuralı <see cref="Serialization.ApiJson"/> içinde uygulanır).
    /// </summary>
    public class BaseResponseDto<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        /// <summary>Yalnızca geliştirme ortamında teknik ayrıntı.</summary>
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }

        [JsonPropertyName("errorCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ErrorCode { get; set; }

        /// <summary>Doğrulama hataları: camelCase alan adı → mesajlar.</summary>
        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string[]>? Errors { get; set; }

        /// <summary>Destek ve log korelasyonu için (çerçeve seviyesindeki hata yanıtlarında doldurulur).</summary>
        [JsonPropertyName("traceId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TraceId { get; set; }

        /// <summary>
        /// Hata sonucunun HTTP durum kodu (yalnızca sunucu içi, JSON'a yazılmaz). Aynı <c>errorCode</c> uca göre
        /// farklı durum alabildiğinden (ör. <c>PRODUCT_NOT_FOUND</c>: ürün detayında 404, sepete eklemede 400)
        /// kararı servis verir.
        /// </summary>
        [JsonIgnore]
        public int? StatusCode { get; set; }

        public static BaseResponseDto<T> SuccessResult(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        public static BaseResponseDto<T> Fail(string message, string errorCode, int statusCode = 400) =>
            new() { Success = false, Message = message, ErrorCode = errorCode, StatusCode = statusCode };

        public static BaseResponseDto<T> NotFound(string message, string errorCode) => Fail(message, errorCode, 404);

        public static BaseResponseDto<T> Forbidden(string message) => Fail(message, Common.ErrorCodes.Forbidden, 403);

        /// <summary>Başka bir tipteki hata sonucunu (kod ve durum korunarak) bu tipe taşır.</summary>
        public static BaseResponseDto<T> From<TOther>(BaseResponseDto<TOther> failure) => new()
        {
            Success = false,
            Message = failure.Message,
            ErrorCode = failure.ErrorCode,
            Errors = failure.Errors,
            StatusCode = failure.StatusCode,
        };
    }
}
