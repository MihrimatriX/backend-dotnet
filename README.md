# E-commerce Backend (.NET)

**Modern, ölçeklenebilir ve güvenli bir e-ticaret backend API'si - .NET 9, JWT kimlik doğrulama, Redis önbellekleme, PostgreSQL veritabanı, Prometheus/Grafana izleme ve Docker desteği ile üretim için hazır.**

A production-ready .NET 9 e-commerce backend application with comprehensive features including JWT authentication, Redis caching, PostgreSQL database, monitoring, and Docker support.

## 🚀 Features

### Core Features
- **JWT Authentication** - Secure token-based authentication
- **Multi-environment Support** - Development and production configurations
- **Database Migration** - Entity Framework Core migrations
- **Caching** - Redis-based caching for improved performance
- **API Documentation** - Swagger/OpenAPI documentation

### Security Features
- **Rate Limiting** - API rate limiting with AspNetCoreRateLimit
- **CORS Configuration** - Configurable cross-origin resource sharing
- **Security Headers** - Comprehensive security headers
- **Request Logging** - Detailed request/response logging with Serilog

### Monitoring & Observability
- **Health Checks** - Comprehensive health monitoring
- **Prometheus Integration** - Custom metrics collection
- **Grafana Dashboards** - Real-time monitoring dashboards
- **Request Tracing** - Unique request ID tracking

### Production Features
- **Docker Support** - Multi-stage Docker builds
- **Connection Pooling** - Optimized database connections
- **Async Processing** - Background task processing
- **Structured Logging** - Serilog with multiple sinks

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Load Balancer │    │   API Gateway   │
│   (React/Vue)   │◄──►│   (Nginx)       │◄──►│   (.NET Core)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        │
                       ┌─────────────────────────────────┼─────────────────────────────────┐
                       │                                 │                                 │
                       ▼                                 ▼                                 ▼
              ┌─────────────────┐              ┌─────────────────┐              ┌─────────────────┐
              │   PostgreSQL    │              │      Redis      │              │   Prometheus    │
              │   Database      │              │      Cache      │              │    Metrics      │
              └─────────────────┘              └─────────────────┘              └─────────────────┘
                                                                                         │
                                                                                         ▼
                                                                              ┌─────────────────┐
                                                                              │     Grafana     │
                                                                              │   Dashboards    │
                                                                              └─────────────────┘
```

## 🛠️ Technology Stack

- **.NET 9** - Latest .NET runtime
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with PostgreSQL
- **Redis** - Caching and session storage
- **JWT** - Authentication tokens
- **Serilog** - Structured logging
- **Prometheus** - Metrics collection
- **Grafana** - Monitoring dashboards
- **Docker** - Containerization
- **Swagger/OpenAPI** - API documentation

## 📋 Prerequisites

- .NET 9 SDK
- PostgreSQL 15+
- Redis 7+
- Docker & Docker Compose (optional)

## 🚀 Quick Start

### Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd backend-dotnet
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database**
   - Update connection string in `appsettings.Development.json`
   - Run migrations:
   ```bash
   dotnet ef database update
   ```

4. **Start Redis**
   ```bash
   redis-server
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

### Docker Setup

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

2. **Access the application**
   - **API**: http://localhost:8080
   - **Swagger UI**: http://localhost:8080/swagger
   - **Health Checks**: http://localhost:8080/health-ui
   - **Prometheus**: http://localhost:9090
   - **Grafana**: http://localhost:3000 (admin/admin123)

## 📚 API Documentation

### Authentication Endpoints
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh token

### Product Endpoints
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Order Endpoints
- `GET /api/orders` - Get user orders
- `POST /api/orders` - Create order
- `GET /api/orders/{id}` - Get order by ID
- `PUT /api/orders/{id}/status` - Update order status

### Health & Monitoring
- `GET /api/health` - Application health
- `GET /api/metrics/prometheus` - Prometheus metrics
- `GET /swagger` - API documentation

## 🔍 Monitoring

### Prometheus Metrics
- HTTP request metrics
- .NET runtime metrics
- Custom e-commerce metrics
- Database connection metrics

### Grafana Dashboards
- Application performance
- Business metrics
- Infrastructure monitoring
- Error tracking

### Access URLs
- **Application**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Health UI**: http://localhost:8080/health-ui
- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3000 (admin/admin123)

## 🐳 Docker

### Development
```bash
docker-compose up -d
```

### Production
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Services
- **app**: .NET application
- **postgres**: PostgreSQL database
- **redis**: Redis cache
- **prometheus**: Metrics collection
- **grafana**: Monitoring dashboards

## 🧪 Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
```bash
dotnet test --filter Category=Integration
```

### Health Check
```bash
curl http://localhost:8080/health
```

## 📈 Performance Optimization

### Database
- Connection pooling with Npgsql
- Optimized EF Core settings
- Database indexes
- Query optimization

### Caching
- Redis-based caching
- Cache eviction strategies
- TTL configuration

### Async Processing
- Background task processing
- Thread pool configuration
- Email notifications
- System notifications

## 🔒 Security

### Authentication
- JWT token-based authentication
- Password encryption with BCrypt
- Token expiration handling

### Authorization
- Role-based access control
- Endpoint security
- Method-level security

### Security Headers
- X-Content-Type-Options
- X-Frame-Options
- X-XSS-Protection
- Strict-Transport-Security
- Content-Security-Policy

### Rate Limiting
- API rate limiting
- IP-based restrictions
- Configurable limits

## 📝 Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `DB_HOST` - Database host
- `DB_PORT` - Database port
- `DB_NAME` - Database name
- `DB_USERNAME` - Database username
- `DB_PASSWORD` - Database password
- `REDIS_HOST` - Redis host
- `REDIS_PORT` - Redis port
- `JWT_SECRET` - JWT secret key
- `ALLOWED_ORIGINS` - CORS allowed origins
- `RATE_LIMIT` - Rate limit per minute

### Configuration Files
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development settings
- `appsettings.Production.json` - Production settings
