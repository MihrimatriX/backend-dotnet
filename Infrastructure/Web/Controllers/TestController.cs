using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("hello")]
        public ActionResult<object> Hello()
        {
            var response = new
            {
                message = "Hello from .NET Core!",
                status = "success",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                framework = ".NET 9.0",
                version = "1.0.0"
            };

            return Ok(response);
        }

        [HttpGet("health")]
        public ActionResult<object> Health()
        {
            var response = new
            {
                status = "UP",
                service = "ecommerce-backend-dotnet",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            };

            return Ok(response);
        }

        [HttpGet("info")]
        public ActionResult<object> Info()
        {
            var response = new
            {
                service = "E-Commerce Backend API",
                version = "1.0.0",
                framework = ".NET 9.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                uptime = Environment.TickCount64,
                machine = Environment.MachineName,
                os = Environment.OSVersion.ToString()
            };

            return Ok(response);
        }

        [HttpGet("ping")]
        public ActionResult<object> Ping()
        {
            return Ok(new { message = "pong", timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
        }
    }
}
