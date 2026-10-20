using Microsoft.AspNetCore.Mvc;
using Prometheus;
using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly ILogger<MetricsController> _logger;

        // Custom metrics
        private static readonly Counter HttpRequestsTotal = Metrics
            .CreateCounter("http_requests_total", "Total HTTP requests", new[] { "method", "endpoint", "status" });

        private static readonly Histogram HttpRequestDuration = Metrics
            .CreateHistogram("http_request_duration_seconds", "HTTP request duration", new[] { "method", "endpoint" });

        private static readonly Gauge ActiveConnections = Metrics
            .CreateGauge("active_connections", "Number of active connections");

        private static readonly Counter OrdersTotal = Metrics
            .CreateCounter("orders_total", "Total orders", new[] { "status" });

        private static readonly Gauge ProductsInStock = Metrics
            .CreateGauge("products_in_stock", "Number of products in stock");

        public MetricsController(ILogger<MetricsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetMetrics()
        {
            try
            {
                using var stream = new MemoryStream();
                await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(stream, CancellationToken.None);
                stream.Position = 0;
                var metrics = new StreamReader(stream).ReadToEnd();
                return Content(metrics, "text/plain; version=0.0.4; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics");
                return StatusCode(500, "Error retrieving metrics");
            }
        }

        [HttpGet("prometheus")]
        public async Task<ActionResult<string>> GetPrometheusMetrics()
        {
            try
            {
                using var stream = new MemoryStream();
                await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(stream, CancellationToken.None);
                stream.Position = 0;
                var metrics = new StreamReader(stream).ReadToEnd();
                return Content(metrics, "text/plain; version=0.0.4; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Prometheus metrics");
                return StatusCode(500, "Error retrieving Prometheus metrics");
            }
        }

        [HttpGet("custom")]
        public ActionResult<object> GetCustomMetrics()
        {
            try
            {
                var customMetrics = new
                {
                    httpRequests = new
                    {
                        total = HttpRequestsTotal.Value,
                        description = "Total HTTP requests"
                    },
                    httpRequestDuration = new
                    {
                        description = "HTTP request duration histogram"
                    },
                    activeConnections = new
                    {
                        value = ActiveConnections.Value,
                        description = "Number of active connections"
                    },
                    orders = new
                    {
                        total = OrdersTotal.Value,
                        description = "Total orders"
                    },
                    products = new
                    {
                        inStock = ProductsInStock.Value,
                        description = "Number of products in stock"
                    },
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                return Ok(customMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom metrics");
                return StatusCode(500, "Error retrieving custom metrics");
            }
        }

        // Helper methods to update metrics
        public static void IncrementHttpRequests(string method, string endpoint, string status)
        {
            HttpRequestsTotal.WithLabels(method, endpoint, status).Inc();
        }

        public static void RecordHttpRequestDuration(string method, string endpoint, double duration)
        {
            HttpRequestDuration.WithLabels(method, endpoint).Observe(duration);
        }

        public static void SetActiveConnections(double count)
        {
            ActiveConnections.Set(count);
        }

        public static void IncrementOrders(string status)
        {
            OrdersTotal.WithLabels(status).Inc();
        }

        public static void SetProductsInStock(double count)
        {
            ProductsInStock.Set(count);
        }
    }
}