using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Infrastructure.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                context.Request.EnableBuffering();
                var originalBodyStream = context.Response.Body;

                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                if (context.Response.StatusCode == 400)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var response = await new StreamReader(responseBody).ReadToEndAsync();
                    
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<BaseResponseDto<object>>(response);
                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Error))
                        {
                            responseBody.Seek(0, SeekOrigin.Begin);
                            responseBody.SetLength(0);
                            
                            var validationResponse = BaseResponseDto<object>.ErrorResult("Validation failed", errorResponse.Error);
                            var jsonResponse = JsonSerializer.Serialize(validationResponse);
                            
                            await responseBody.WriteAsync(System.Text.Encoding.UTF8.GetBytes(jsonResponse));
                        }
                    }
                    catch
                    {
                        // If deserialization fails, keep original response
                    }
                }

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            else
            {
                await _next(context);
            }
        }
    }

    public static class ValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseValidationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidationMiddleware>();
        }
    }
}
