using Microsoft.EntityFrameworkCore;
using EcommerceBackend.Infrastructure.Data;
using EcommerceBackend.Infrastructure.Repositories;
using EcommerceBackend.Application.Services;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Infrastructure.DependencyInjection;
using EcommerceBackend.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Prometheus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
// using AspNetCoreRateLimit; // Temporarily disabled
var builder = WebApplication.CreateBuilder(args);
var isSeedDemoForceCmd = args.Any(a => string.Equals(a, "seed-demo-force", StringComparison.OrdinalIgnoreCase));
var runDemoSeedOnly = isSeedDemoForceCmd
    || args.Any(a =>
        string.Equals(a, "seed-demo", StringComparison.OrdinalIgnoreCase)
        || string.Equals(a, "--seed-demo", StringComparison.OrdinalIgnoreCase));
var demoSeedForce = runDemoSeedOnly && (
    isSeedDemoForceCmd
    || string.Equals(Environment.GetEnvironmentVariable("ECOMMERCE_SEED_DEMO_FORCE"), "1", StringComparison.Ordinal)
    || args.Any(a =>
        string.Equals(a, "--force", StringComparison.OrdinalIgnoreCase)
        || string.Equals(a, "--force-seed-demo", StringComparison.OrdinalIgnoreCase)));

// MassTransit / Testcontainers: hosted service'lerin (otobüs durdurma) DI dispose'dan önce tamamlanması için süre.
builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(60));

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddApplicationPart(typeof(EcommerceBackend.Infrastructure.Web.Controllers.ProductController).Assembly);
builder.Services.AddEndpointsApiExplorer();

// Enhanced Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Commerce API",
        Version = "v1.0.0",
        Description = "E-ticaret REST API: PostgreSQL/SQLite, Redis katalog önbelleği, RabbitMQ (MassTransit) ile sipariş olayları, JWT, Prometheus metrikleri.",
        Contact = new OpenApiContact
        {
            Name = "AFU",
            Email = "afu@example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

var databaseProvider = builder.Configuration["Database:Provider"]?.Trim();
if (string.IsNullOrEmpty(databaseProvider))
{
    databaseProvider = connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)
        ? "Sqlite"
        : "Npgsql";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(connectionString);
    else
        options.UseNpgsql(connectionString);
});

builder.Services.AddMemoryCache();

builder.Services.Configure<CheckoutOptions>(builder.Configuration.GetSection(CheckoutOptions.SectionName));
builder.Services.AddScoped<ICheckoutPaymentSimulator, CheckoutPaymentSimulator>();

builder.Services.AddEcommerceInfrastructure(builder.Configuration);

var redisConnection = builder.Configuration.ResolveRedisConnectionString();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is healthy"))
    .AddEcommerceInfrastructureHealthChecks(builder.Configuration, redisConnection);

// Health Checks UI
builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(10);
    options.MaximumHistoryEntriesPerEndpoint(100);
    options.AddHealthCheckEndpoint("E-Commerce API", "/api/health");
}).AddInMemoryStorage();

// Rate Limiting (temporarily disabled due to IMemoryCache dependency issue)
// builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("RateLimit"));
// builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
// builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
// builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
// builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IHelpSupportService, HelpSupportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();


// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
var corsMethods = builder.Configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
var corsHeaders = builder.Configuration.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? new[] { "*" };
var allowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials", true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .WithMethods(corsMethods)
              .WithHeaders(corsHeaders);

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default-key"))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
        c.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
        c.DocumentTitle = "E-Commerce API Documentation";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });
}

// Prometheus metrics
app.UseHttpMetrics();

// Security headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Korelasyon (Serilog LogContext + X-Correlation-Id)
app.UseCorrelationId();

// Request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Rate limiting (temporarily disabled)
// app.UseIpRateLimiting();

// app.UseHttpsRedirection(); // Disabled for Docker
app.UseCors("AllowReactApp");

// Middleware
app.UseGlobalExceptionMiddleware();
app.UseValidationMiddleware();

app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/api/health");
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    // options.AddCustomStylesheet("health-ui.css"); // CSS file not found, disabled
});

// Map controllers
app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/", () => "E-Commerce API is running!");
app.MapGet("/health", () => "OK");
app.MapGet("/actuator/health", () => "OK");
app.MapGet("/actuator/prometheus", () => "OK");

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        await context.Database.EnsureCreatedAsync();
        await SqliteCartItemsBootstrap.EnsureAsync(context);
    }
    else
    {
        await context.Database.MigrateAsync();
    }

    var seeder = new DataSeeder(context);
    await seeder.SeedAsync();

    if (runDemoSeedOnly)
        await DemoDataSeeder.SeedAsync(context, force: demoSeedForce);
}

if (runDemoSeedOnly)
{
    Log.Information("seed-demo tamamlandı; sunucu başlatılmadı.");
    return;
}

try
{
    Log.Information("Starting E-Commerce API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
