using System.Text.Json.Serialization;

namespace EcommerceBackend.Application.DTOs
{
    public class BaseResponseDto<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }
        
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }
        
        public BaseResponseDto() { }
        
        public BaseResponseDto(bool success, string message, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
        
        public BaseResponseDto(bool success, string message, string? error)
        {
            Success = success;
            Message = message;
            Error = error;
        }
        
        public static BaseResponseDto<T> SuccessResult(T data) => new(true, "Operation successful", data);
        public static BaseResponseDto<T> SuccessResult(string message, T data) => new(true, message, data);
        public static BaseResponseDto<T> SuccessResult(string message) => new(true, message);
        public static BaseResponseDto<T> ErrorResult(string message) => new(false, message, default(T));
        public static BaseResponseDto<T> ErrorResult(string message, string error) => new(false, message, error);
    }

    // Special version for value types like bool, int, etc.
    public class BaseResponseDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Data { get; set; }
        
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }
        
        public BaseResponseDto() { }
        
        public BaseResponseDto(bool success, string message, object? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
        
        public BaseResponseDto(bool success, string message, string? error)
        {
            Success = success;
            Message = message;
            Error = error;
        }
        
        public static BaseResponseDto SuccessResult(object data) => new(true, "Operation successful", data);
        public static BaseResponseDto SuccessResult(string message, object data) => new(true, message, data);
        public static BaseResponseDto SuccessResult(string message) => new(true, message);
        public static BaseResponseDto ErrorResult(string message) => new(false, message, null);
        public static BaseResponseDto ErrorResult(string message, string error) => new(false, message, error);
    }
}