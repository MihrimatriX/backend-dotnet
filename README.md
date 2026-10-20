# E-commerce Backend (.NET)

.NET 9 e-ticaret REST API'si. [`docs/API_CONTRACT.md`](docs/API_CONTRACT.md)'deki ortak sözleşmeyi Spring ikiziyle (`backend-spring`) birebir uygular; vitrin (`temp-shop-net`) yalnızca API adresini değiştirerek ikisinden birine bağlanır.

İnceleme raporu: https://mihrimatrix.github.io/backend-dotnet/

## Neler var

- **Kimlik:** JWT (HS256, 24 saat). Rol, `Auth:AdminEmails` listesine göre atanır. `logout-all-devices` ve şifre değişikliği eski token'ları iptal eder. 15 dakika içinde art arda 5 hatalı girişte hesap kilitlenir (429 `RATE_LIMITED`).
- **Veri:** EF Core. Geliştirmede SQLite (`EnsureCreated`), Docker/üretimde PostgreSQL (migration'lar `Migrations/` altında).
- **Önbellek:** Redis katalog önbelleği (isteğe bağlı; erişilemezse health `Degraded` olur, API çalışmaya devam eder).
- **Mesajlaşma:** Sipariş olayları transactional outbox'a yazılır, MassTransit ile RabbitMQ'ya (geliştirmede InMemory) yayınlanır. 10 başarısız denemeden sonra mesaj atlanır, nedeni `last_error`'da kalır.
- **Gözlemlenebilirlik:** Serilog, korelasyon kimliği (`X-Correlation-ID`), Prometheus (`/actuator/prometheus`, `/api/metrics`), health uçları (`/health`, `/actuator/health`, `/api/health[/ready|/live]`).
- **HTTP:** IP başına sabit pencere hız limiti (`RateLimit:RequestsPerMinute`), CORS, güvenlik başlıkları, tek tip hata zarfı.

## Hızlı başlangıç

### Yerel (SQLite, harici servis gerekmez)

```bash
dotnet run --project EcommerceBackend.csproj
# http://localhost:5000  ·  Swagger: http://localhost:5000/swagger
```

`appsettings.Development.json`: SQLite (`ecommerce.db`), InMemory mesajlaşma, demo lojistik açık. `EnsureCreated` mevcut dosyaya yeni kolon eklemediği için eski dosyalar açılışta `SqliteCartItemsBootstrap` ile yükseltilir; yeni şema değişikliklerini oraya da ekleyin (ya da `ecommerce.db`'yi silin).

> Makinede yalnızca .NET 10 runtime varsa: `DOTNET_ROLL_FORWARD=Major dotnet run ...`

### Docker (PostgreSQL + Redis + RabbitMQ)

```bash
docker compose up --build -d      # veya ./scripts/docker-up.sh | .ps1
```

| Servis | Adres |
|---|---|
| API | http://localhost:5000 |
| PostgreSQL | localhost:5432 |
| Redis | localhost:16379 |
| RabbitMQ yönetim | http://localhost:15672 |

Compose API'yi `Development` ortamında çalıştırır (Swagger açık, hata ayrıntısı döner). Üretim için kullanmayın.

## Demo hesapları

`DataSeeder` **yalnızca Production dışındaki ortamlarda** çalışır ve şu hesapları idempotent olarak oluşturur:

| E-posta | Şifre | Rol |
|---|---|---|
| admin@example.com | admin123 | Admin |
| manager@shop.demo | Manager123! | Admin |
| user1@example.com … user4@example.com | user123 | User |
| support@shop.demo · demo.buyer@shop.local · staff@shop.demo | Support123! · Buyer123! · Staff123! | User |

Ek demo verisi (sipariş, yorum, kart…): `dotnet run -- seed-demo` (`--force` ile yeniden).

## Üretim yapılandırması

`appsettings.Production.json` secret içermez. Ortam değişkenleri:

| Değişken | Not |
|---|---|
| `Jwt__Key` | Zorunlu, en az 32 bayt. Repodaki örnek anahtarla uygulama açılmaz. |
| `ConnectionStrings__DefaultConnection` | Zorunlu, PostgreSQL bağlantısı |
| `RabbitMq__Host`, `RabbitMq__Username`, `RabbitMq__Password` | Zorunlu |
| `Redis__ConnectionString` | İsteğe bağlı |
| `Auth__AdminEmails__0`, `__1`… | Admin e-postaları |
| `ALLOWED_ORIGINS` | CORS, virgülle ayrılmış |
| `ForwardedHeaders__KnownProxies__0`… | Reverse proxy loopback değilse, gerçek istemci IP'si için |

Migration'lar uygulama açılışında (`MigrateAsync`) uygulanır; elle: `./scripts/migrate.sh`.

## Geliştirme

```bash
dotnet tool restore                          # dotnet-ef
dotnet build EcommerceBackend.sln
dotnet test EcommerceBackend.sln             # Testcontainers: Docker gerekir
dotnet ef migrations add <Ad> --project EcommerceBackend.csproj
node scripts/contract-smoke.mjs run http://localhost:5000 out-dotnet.json   # sözleşme testi
```

CI (`.github/workflows/ci.yml`) her PR'da build, entegrasyon testleri, bekleyen migration kontrolü ve sözleşme smoke testini çalıştırır.

## Proje yapısı

Tek proje (`EcommerceBackend.csproj`), klasörlere bölünmüş:

```
Domain/Entities          EF entity'leri
Application/             DTO'lar, servisler, seçenekler, hata kodları, JSON ayarları
Infrastructure/          DbContext, seed, middleware, mesajlaşma/outbox, Redis, controller'lar
Migrations/              PostgreSQL migration'ları
tests/                   Entegrasyon testleri (xUnit + Testcontainers)
scripts/                 Docker, migration, test ve smoke script'leri (.sh / .ps1)
docs/                    API sözleşmesi ve GitHub Pages raporu
```
