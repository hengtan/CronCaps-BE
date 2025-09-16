# 🕐 CronCaps Backend

> **Enterprise-grade Cron Job Management System built with .NET 8 + Clean Architecture**

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Docker](https://img.shields.io/badge/docker-ready-blue.svg)](docker-compose.yml)

## 🎯 Overview

CronCaps Backend is a robust, scalable, and maintainable cron job management system designed with Clean Architecture principles. It provides enterprise-level features for scheduling, monitoring, and managing automated tasks with real-time insights and comprehensive logging.

## ✨ Key Features

- 🏗️ **Clean Architecture** - Domain-driven design with clear separation of concerns
- 🔐 **JWT Authentication** - Secure user authentication and role-based authorization
- 📊 **Real-time Monitoring** - Live job execution status with SignalR
- 🗃️ **Multi-database Support** - PostgreSQL + MongoDB + Redis
- ⚡ **High Performance** - Memory caching and optimized queries
- 🔍 **Comprehensive Logging** - Structured logging with Serilog
- 📈 **Analytics Dashboard** - Job performance metrics and insights
- 🐳 **Docker Ready** - Complete containerized development environment
- ✅ **Fully Tested** - Unit, integration, and performance tests
- 📚 **OpenAPI/Swagger** - Complete API documentation

## 🏛️ Architecture

```
src/
├── CronCaps.Domain/          # 🏛️  Entities, Value Objects, Domain Logic
│   ├── Entities/             # Core business entities
│   ├── ValueObjects/         # Immutable value objects
│   ├── Services/             # Domain services
│   ├── Events/               # Domain events
│   └── Exceptions/           # Domain exceptions
│
├── CronCaps.Application/     # 📋 Use Cases, DTOs, Interfaces
│   ├── UseCases/             # Application use cases
│   ├── DTOs/                 # Data transfer objects
│   ├── Interfaces/           # Repository & service contracts
│   ├── Validators/           # Input validation with FluentValidation
│   ├── Mapping/              # AutoMapper profiles
│   └── Behaviors/            # Cross-cutting concerns
│
├── CronCaps.Infrastructure/  # 🔧 Data Access, External Services
│   ├── Data/                 # EF Core contexts & configurations
│   ├── Repositories/         # Repository implementations
│   ├── Cache/                # Redis caching implementation
│   ├── External/             # Third-party service integrations
│   ├── Messaging/            # Message queuing (Hangfire)
│   └── Monitoring/           # Application performance monitoring
│
├── CronCaps.API/            # 🚀 Controllers, Middleware, Configuration
│   ├── Controllers/          # REST API endpoints
│   ├── Middleware/           # Custom middleware
│   ├── Filters/              # Action filters
│   ├── Hubs/                 # SignalR hubs
│   └── Configuration/        # Startup configuration
│
└── CronCaps.BackgroundServices/ # ⚙️ Schedulers, Workers
    ├── Services/             # Background service implementations
    ├── Jobs/                 # Hangfire job definitions
    └── Schedulers/           # Job scheduling logic
```

## 🛠️ Tech Stack

### **Core Framework**
- **.NET 8** - Latest LTS with performance improvements
- **C# 12** - Latest language features
- **ASP.NET Core 8** - Web API framework

### **Database & Caching**
- **PostgreSQL 16** - Primary relational database (ACID compliance)
- **Entity Framework Core 8** - ORM with migrations
- **MongoDB 7** - Document store for logs and analytics
- **Redis 7** - In-memory caching and session store

### **Authentication & Security**
- **JWT Bearer Tokens** - Stateless authentication
- **ASP.NET Core Identity** - User management
- **Role-based Authorization** - Fine-grained access control
- **HTTPS Enforcement** - TLS encryption

### **Background Processing**
- **Hangfire** - Background job processing
- **Quartz.NET** - Advanced scheduling (optional)
- **SignalR** - Real-time communication

### **Monitoring & Logging**
- **Serilog** - Structured logging
- **Application Insights** - Performance monitoring (optional)
- **Health Checks** - Service health monitoring

### **Development Tools**
- **Docker & Docker Compose** - Containerization
- **Swagger/OpenAPI** - API documentation
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **xUnit** - Unit testing framework

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### 1. Clone and Setup
```bash
git clone <repository-url>
cd croncaps-be

# Start infrastructure services
docker-compose up -d

# Verify services are running
docker-compose ps
```

### 2. Database Setup
```bash
# Run migrations
dotnet ef database update --project src/CronCaps.Infrastructure

# Verify databases are created
# PostgreSQL: localhost:5432 (croncaps_user/croncaps_dev_password)
# MongoDB: localhost:27017 (root/croncaps_dev_password)
# Redis: localhost:6379 (croncaps_dev_password)
```

### 3. Build and Run
```bash
# Build solution
dotnet build

# Run API
dotnet run --project src/CronCaps.API

# Run background services (separate terminal)
dotnet run --project src/CronCaps.BackgroundServices
```

### 4. Access Services
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Hangfire Dashboard**: http://localhost:5000/hangfire
- **Database Tools**:
  - Adminer: http://localhost:8082
  - Mongo Express: http://localhost:8083
  - Redis Commander: http://localhost:8084

## 🔧 Configuration

### Development Settings
Copy and modify the configuration files:
```bash
# API configuration
cp src/CronCaps.API/appsettings.Development.example.json src/CronCaps.API/appsettings.Development.json

# Background services configuration
cp src/CronCaps.BackgroundServices/appsettings.Development.example.json src/CronCaps.BackgroundServices/appsettings.Development.json
```

### Environment Variables
Key environment variables for production:
```bash
# Database connections
ConnectionStrings__DefaultConnection=Server=...
ConnectionStrings__MongoConnection=mongodb://...
ConnectionStrings__RedisConnection=localhost:6379

# JWT settings
JwtSettings__SecretKey=your-super-secret-key-here
JwtSettings__Issuer=CronCaps
JwtSettings__Audience=CronCaps.API

# External services
HangfireSettings__Dashboard=true
MonitoringSettings__ApplicationInsights=<your-key>
```

## 📊 Database Schema

### PostgreSQL (Primary Data)
- **Users** - User accounts and profiles
- **Roles** - Role definitions and permissions
- **Jobs** - Cron job definitions and configurations
- **JobExecutions** - Basic execution tracking
- **Categories** - Job categorization
- **Teams** - Team-based access control

### MongoDB (Logs & Analytics)
- **execution_logs** - Detailed execution logs and outputs
- **system_metrics** - Performance and system metrics
- **audit_logs** - User activity and security audit trail

### Redis (Cache & Sessions)
- **User sessions** - JWT token blacklisting
- **Job cache** - Frequently accessed job data
- **Rate limiting** - API throttling data
- **Real-time data** - SignalR connection state

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/CronCaps.Domain.Tests
dotnet test tests/CronCaps.Application.Tests
dotnet test tests/CronCaps.Integration.Tests
```

### Test Strategy
- **Unit Tests** - Domain logic and application services
- **Integration Tests** - API endpoints and database operations
- **Performance Tests** - Load testing critical endpoints
- **Contract Tests** - API contract validation

## 📈 Performance & Monitoring

### Built-in Health Checks
- **Database Connectivity** - PostgreSQL, MongoDB, Redis
- **External Services** - SMTP, third-party APIs
- **Background Jobs** - Hangfire queue health
- **Memory Usage** - Application memory consumption

### Monitoring Endpoints
- `/health` - Overall application health
- `/health/ready` - Readiness probe for K8s
- `/health/live` - Liveness probe for K8s
- `/metrics` - Prometheus-compatible metrics

### Performance Features
- **Connection Pooling** - Optimized database connections
- **Lazy Loading** - Efficient entity loading
- **Response Caching** - HTTP response caching
- **Memory Caching** - In-memory data caching
- **Async/Await** - Non-blocking operations

## 🔐 Security Features

- **JWT Authentication** - Stateless token-based auth
- **Role-based Authorization** - Granular permissions
- **Rate Limiting** - API throttling protection
- **Input Validation** - Comprehensive input sanitization
- **SQL Injection Prevention** - Parameterized queries
- **XSS Protection** - Output encoding
- **CORS Configuration** - Cross-origin request control
- **Security Headers** - HSTS, CSP, X-Frame-Options

## 📚 API Documentation

The API is fully documented using OpenAPI/Swagger. Once the application is running, visit:
- **Swagger UI**: http://localhost:5000/swagger
- **OpenAPI JSON**: http://localhost:5000/swagger/v1/swagger.json

### Key Endpoints
```
Authentication:
POST   /api/auth/login          # User login
POST   /api/auth/refresh        # Refresh token
POST   /api/auth/logout         # User logout

Job Management:
GET    /api/jobs                # List jobs
POST   /api/jobs                # Create job
GET    /api/jobs/{id}           # Get job details
PUT    /api/jobs/{id}           # Update job
DELETE /api/jobs/{id}           # Delete job
POST   /api/jobs/{id}/execute   # Execute job manually

Monitoring:
GET    /api/jobs/{id}/executions    # Execution history
GET    /api/jobs/{id}/logs          # Execution logs
GET    /api/system/metrics          # System metrics
GET    /api/system/health           # Health status
```

## 🚀 Deployment

### Docker Production
```bash
# Build production images
docker build -t croncaps-api -f src/CronCaps.API/Dockerfile .
docker build -t croncaps-worker -f src/CronCaps.BackgroundServices/Dockerfile .

# Run with production compose
docker-compose -f docker-compose.prod.yml up -d
```

### Kubernetes
Kubernetes manifests are available in the `/k8s` directory:
```bash
kubectl apply -f k8s/
```

### Cloud Deployment
- **Azure**: App Service + Azure Database + Redis Cache
- **AWS**: ECS/EKS + RDS + ElastiCache
- **GCP**: Cloud Run + Cloud SQL + Memorystore

## 🛡️ Best Practices Implemented

### SOLID Principles
- **S** - Single Responsibility: Each class has one reason to change
- **O** - Open/Closed: Open for extension, closed for modification
- **L** - Liskov Substitution: Subtypes must be substitutable
- **I** - Interface Segregation: Many specific interfaces
- **D** - Dependency Inversion: Depend on abstractions

### Design Patterns
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Factory Pattern** - Object creation
- **Strategy Pattern** - Algorithm encapsulation
- **Observer Pattern** - Event handling
- **Decorator Pattern** - Behavior extension

### Clean Code Principles
- **Meaningful Names** - Self-documenting code
- **Small Functions** - Single responsibility functions
- **DRY** - Don't Repeat Yourself
- **KISS** - Keep It Simple, Stupid
- **YAGNI** - You Aren't Gonna Need It

### ACID Compliance
- **Atomicity** - All-or-nothing transactions
- **Consistency** - Data integrity constraints
- **Isolation** - Concurrent transaction handling
- **Durability** - Persistent data storage

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow SOLID principles and Clean Code practices
- Write comprehensive unit tests (>90% coverage)
- Use conventional commit messages
- Update documentation for new features
- Ensure all tests pass before submitting PR

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙋‍♂️ Support

- **Documentation**: [/docs](./docs)
- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Email**: support@croncaps.com

---

**Built with ❤️ using .NET 8 and Clean Architecture principles**