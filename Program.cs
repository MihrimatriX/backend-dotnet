using Microsoft.EntityFrameworkCore;
using EcommerceBackend.Infrastructure.Data;
using EcommerceBackend.Infrastructure.Repositories;
using EcommerceBackend.Application.Services;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Serialization;
using EcommerceBackend.Infrastructure.DependencyInjection;
using EcommerceBackend.Infrastructure.Middleware;
using EcommerceBackend.Infrastructure.Security;
using EcommerceBackend.Infrastructure.Web;
using Serilog;
using Prometheus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;

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
    .AddApplicationPart(typeof(EcommerceBackend.Infrastructure.Web.Controllers.ProductController).Assembly)
    .AddJsonOptions(options => ApiJson.Configure(options.JsonSerializerOptions))
    .ConfigureApiBehavior();
builder.Services.AddEndpointsApiExplorer();

// Enhanced Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Commerce API",
        Version = "v1.0.0",
        Description = "E-ticaret REST API: PostgreSQL/SQLite, Redis katalog önbelleği, RabbitMQ (MassTransit) ile sipariş olayları, JWT, Prometheus metrikleri. Sözleşme: docs/API_CONTRACT.md",
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
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<ICheckoutPaymentSimulator, CheckoutPaymentSimulator>();

builder.Services.AddEcommerceInfrastructure(builder.Configuration);

var redisConnection = builder.Configuration.ResolveRedisConnectionString();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is healthy"))
    .AddEcommerceInfrastructureHealthChecks(builder.Configuration, redisConnection);

// Reverse proxy arkasında gerçek istemci IP'si (hız limiti, giriş geçmişi). Varsayılan yalnızca loopback'e güvenir;
// proxy başka bir host'taysa adresi ForwardedHeaders:KnownProxies listesine ekleyin.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
        options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
});

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();

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

// HTTP: CORS, hız limiti, JWT (§1.4, §2)
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddApiRateLimiting(builder.Configuration);
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
// Repoda commit'li anahtarlar herkese açık: üretimde Jwt__Key ortam değişkeni zorunlu.
if (builder.Environment.IsProduction()
    && (System.Text.Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32
        || jwtOptions.Key.StartsWith("mySecretKey", StringComparison.Ordinal)))
{
    throw new InvalidOperationException(
        "Jwt:Key üretimde Jwt__Key ortam değişkeniyle verilmeli (en az 32 bayt, repodaki örnek anahtar olamaz).");
}
builder.Services.AddApiJwtAuthentication(jwtOptions);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCorrelationId();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseGlobalExceptionMiddleware();
app.UseApiStatusCodeEnvelopes();

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

app.UseRouting();

// Prometheus metrics
app.UseHttpMetrics();

// app.UseHttpsRedirection(); // Disabled for Docker
app.UseCors(ApiWebSetup.CorsPolicyName);
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "E-Commerce API is running!");
app.MapGet("/health", () => "OK");
app.MapGet("/actuator/health", () => Results.Json(new { status = "UP" }));
app.MapMetrics("/actuator/prometheus");

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

    // Seed bilinen şifreli admin/demo hesapları oluşturur; üretimde asla çalışmamalı.
    if (!app.Environment.IsProduction())
        await new DataSeeder(context).SeedAsync();

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
