using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EcommerceBackend.Infrastructure.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly Dictionary<string, List<DateTime>> _requests = new();
        private readonly int _maxRequests = 100; // per minute
        private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            var now = DateTime.UtcNow;

            // Clean old requests
            CleanOldRequests(clientIp, now);

            // Check rate limit
            if (_requests.ContainsKey(clientIp) && _requests[clientIp].Count >= _maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            // Add current request
            if (!_requests.ContainsKey(clientIp))
            {
                _requests[clientIp] = new List<DateTime>();
            }
            _requests[clientIp].Add(now);

            await _next(context);
        }

        private void CleanOldRequests(string clientIp, DateTime now)
        {
            if (_requests.ContainsKey(clientIp))
            {
                _requests[clientIp] = _requests[clientIp]
                    .Where(time => now - time < _timeWindow)
                    .ToList();
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
