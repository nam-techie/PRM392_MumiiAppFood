# Mumii Microservices - Codebase Documentation

Tài liệu chi tiết về cấu trúc codebase, architecture patterns và conventions của dự án Mumii.

## Mục lục
- [Tổng quan Architecture](#tổng-quan-architecture)
- [Cấu trúc Project](#cấu-trúc-project)
- [Design Patterns](#design-patterns)
- [Service Details](#service-details)
- [Database Schema](#database-schema)
- [API Contracts](#api-contracts)
- [Development Guidelines](#development-guidelines)

---

## Tổng quan Architecture

### Microservices Pattern
```
🏗️ Mumii Architecture Overview

📱 Client Applications (Flutter, Web)
    ↓ HTTP/REST
🌐 API Gateway (YARP) :8080
    ↓ Load Balance & Route
┌─────────────────────────────────────────┐
│  🔐 Auth Service :8081                  │  ← JWT, Accounts
├─────────────────────────────────────────┤ 
│  🏪 Discovery Service :8082             │  ← Restaurants, Location
├─────────────────────────────────────────┤
│  📝 Social Service :8083                │  ← Posts, Comments, Reactions
├─────────────────────────────────────────┤
│  🤖 AI Service :8084                    │  ← Gemini AI Integration
└─────────────────────────────────────────┘
    ↓ Data & Messaging
┌─────────────────────────────────────────┐
│  💾 MySQL (3 databases)                │
│  📡 RabbitMQ (Event Bus)               │
│  🗄️ Redis (Caching)                    │
└─────────────────────────────────────────┘
```

### Key Architecture Principles
- **Clean Architecture**: Dependency Inversion, Domain-centric
- **DDD (Domain Driven Design)**: Bounded contexts per service
- **CQRS Ready**: Separate read/write models
- **Event-Driven**: Domain events với RabbitMQ
- **API-First**: OpenAPI/Swagger documentation

---

## Cấu trúc Project

### Root Structure
```
Mumii.Microservices/
├── 📁 src/                          # Source code chính
│   ├── 📁 ApiGateway/               # YARP API Gateway
│   ├── 📁 Services/                 # Business services
│   │   ├── 📁 Auth/                 # Authentication service
│   │   ├── 📁 Discovery/            # Restaurant discovery
│   │   ├── 📁 Social/               # Social features
│   │   └── 📁 AI/                   # AI chat service
│   └── 📁 Shared/                   # Shared libraries
│       └── 📁 Common/               # DTOs, Events, Constants
├── 📁 scripts/                      # Database scripts
├── 📁 docker/                       # Docker configurations
├── 📄 docker-compose.yml           # Development orchestration
├── 📄 Mumii.Microservices.sln      # Solution file
├── 📄 README.md                     # Project overview
├── 📄 SETUP.md                      # Setup guide
├── 📄 CODEBASE.md                   # This file
└── 📄 env.example                   # Environment template
```

### Service Structure (per service)
```
Service/
├── 📁 Mumii.{Service}.Api/          # Web API layer
│   ├── 📁 Controllers/              # API controllers
│   ├── 📄 Program.cs                # Application entry point
│   ├── 📄 appsettings.json          # Configuration
│   └── 📄 Dockerfile                # Container config
├── 📁 Mumii.{Service}.Domain/       # Domain layer
│   ├── 📁 Entities/                 # Domain entities
│   ├── 📁 Interfaces/               # Repository interfaces
│   └── 📁 Events/                   # Domain events
└── 📁 Mumii.{Service}.Infrastructure/ # Infrastructure layer
    ├── 📁 Data/                     # DbContext, Configurations
    ├── 📁 Repositories/             # Repository implementations
    ├── 📁 Services/                 # External service clients
    └── 📄 DependencyInjection.cs    # DI registration
```

---

## Design Patterns

### 1. Clean Architecture
```
🎯 Dependency Flow (Inward)

📱 API Layer (Controllers)
    ↓ depends on
🏢 Application Layer (Use Cases)
    ↓ depends on  
🏛️ Domain Layer (Entities, Business Rules)
    ↑ implements
🔧 Infrastructure Layer (Data, External Services)
```

### 2. Repository Pattern
```csharp
// Domain Interface
public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(string id);
    Task<PagedResult<Restaurant>> GetPagedAsync(int page, int size);
    Task<Restaurant> AddAsync(Restaurant restaurant);
    Task SaveChangesAsync();
}

// Infrastructure Implementation
public class RestaurantRepository : IRestaurantRepository
{
    private readonly DiscoveryDbContext _context;
    // Implementation...
}
```

### 3. Domain Events
```csharp
// Domain Entity
public class Post
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public static Post Create(string content)
    {
        var post = new Post { Content = content };
        post._domainEvents.Add(new PostCreatedEvent(post.Id));
        return post;
    }
}

// Event Handler
public class PostCreatedEventHandler : INotificationHandler<PostCreatedEvent>
{
    public async Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Handle event (send notification, update stats, etc.)
    }
}
```

### 4. CQRS Pattern
```csharp
// Command (Write)
public record CreateRestaurantCommand(string Name, string Address);

// Query (Read)
public record GetRestaurantQuery(string Id);

// Handlers
public class CreateRestaurantCommandHandler : IRequestHandler<CreateRestaurantCommand, string>
public class GetRestaurantQueryHandler : IRequestHandler<GetRestaurantQuery, RestaurantDto>
```

---

## Service Details

### 🔐 Auth Service

**Responsibilities:**
- User authentication & authorization
- JWT token management
- Account CRUD operations

**Key Components:**
```csharp
// Domain
public class Account
{
    public string Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    
    public static Account Create(string email, string password, string displayName)
    public bool VerifyPassword(string password)
    public void ChangePassword(string currentPassword, string newPassword)
}

// Services
public interface IJwtService
{
    string GenerateAccessToken(Account account);
    string GenerateRefreshToken();
    bool ValidateToken(string token, out string? accountId);
}
```

**APIs:**
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập
- `GET /api/auth/profile` - Lấy profile
- `PUT /api/auth/profile` - Cập nhật profile

### 🏪 Discovery Service

**Responsibilities:**
- Restaurant management
- Location-based search
- Rating system

**Key Components:**
```csharp
// Domain
public class Restaurant
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public decimal Rating { get; private set; }
    
    public static Restaurant Create(string name, string address, ...)
    public double? CalculateDistanceTo(decimal lat, decimal lng)
    public void UpdateRating(decimal newRating, int totalRatings)
}
```

**APIs:**
- `GET /api/restaurants` - Danh sách nhà hàng
- `GET /api/restaurants/{id}` - Chi tiết nhà hàng
- `GET /api/restaurants/search` - Tìm kiếm
- `GET /api/restaurants/nearby` - Tìm gần vị trí

### 📝 Social Service

**Responsibilities:**
- Social posts với mood
- Comments & replies
- Reactions (like, love, wow)

**Key Components:**
```csharp
// Domain
public class Post
{
    public string Id { get; private set; }
    public string Content { get; private set; }
    public string? Mood { get; private set; }
    public List<Comment> Comments { get; private set; }
    public List<Reaction> Reactions { get; private set; }
    
    public static Post Create(string accountId, string content, ...)
    public void AddReaction(string accountId, string reactionType)
    public Comment AddComment(string accountId, string content)
}
```

**APIs:**
- `GET /api/posts` - Feed posts
- `POST /api/posts` - Tạo post
- `PUT /api/posts/{id}/react` - Toggle reaction
- `POST /api/posts/{id}/comments` - Thêm comment

### 🤖 AI Service

**Responsibilities:**
- Gemini AI integration
- Food-specific conversations
- Mood-based suggestions
- Image analysis

**Key Components:**
```csharp
// Domain
public interface IGeminiService
{
    Task<string> ChatAboutFoodAsync(string userMessage);
    Task<string> SuggestFoodByMoodAsync(string mood, string? location);
    Task<string> AnalyzeFoodImageAsync(string imageUrl);
    Task<string> SuggestRestaurantsAsync(string preferences, string? location);
}
```

**APIs:**
- `POST /api/chat/food` - Chat về đồ ăn
- `POST /api/chat/suggest-by-mood` - Gợi ý theo mood
- `POST /api/chat/analyze-image` - Phân tích hình ảnh
- `POST /api/chat/suggest-restaurants` - Gợi ý nhà hàng

---

## Database Schema

### Auth Database (`mumii_auth`)
```sql
CREATE TABLE accounts (
    id VARCHAR(36) PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    avatar_url VARCHAR(500),
    role ENUM('User', 'Admin') DEFAULT 'User',
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

### Discovery Database (`mumii_discovery`)
```sql
CREATE TABLE restaurants (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    address TEXT NOT NULL,
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    region VARCHAR(100),
    avg_price DECIMAL(10,2),
    rating DECIMAL(2,1) DEFAULT 0,
    description TEXT,
    image_urls JSON,
    tags JSON,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT false
);
```

### Social Database (`mumii_social`)
```sql
CREATE TABLE posts (
    id VARCHAR(36) PRIMARY KEY,
    account_id VARCHAR(36) NOT NULL,
    content TEXT NOT NULL,
    mood VARCHAR(50),
    image_urls JSON,
    restaurant_id VARCHAR(36),
    reaction_count INT DEFAULT 0,
    comment_count INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT false
);

CREATE TABLE comments (
    id VARCHAR(36) PRIMARY KEY,
    post_id VARCHAR(36) NOT NULL,
    account_id VARCHAR(36) NOT NULL,
    content TEXT NOT NULL,
    parent_comment_id VARCHAR(36),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT false,
    FOREIGN KEY (post_id) REFERENCES posts(id) ON DELETE CASCADE
);

CREATE TABLE reactions (
    id VARCHAR(36) PRIMARY KEY,
    post_id VARCHAR(36) NOT NULL,
    account_id VARCHAR(36) NOT NULL,
    type ENUM('LIKE', 'LOVE', 'WOW') NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_reaction (post_id, account_id),
    FOREIGN KEY (post_id) REFERENCES posts(id) ON DELETE CASCADE
);
```

---

## API Contracts

### Standard Response Format
```csharp
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public T? Data { get; init; }
    public List<string> Errors { get; init; }
    public DateTime Timestamp { get; init; }
}
```

### Pagination
```csharp
public record PagedResult<T>
{
    public List<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
```

### Common DTOs
```csharp
// Auth
public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, AccountDto Account);

// Discovery
public record RestaurantDto(string Id, string Name, string Address, ...);
public record SearchRestaurantsQuery(string? Query, string? Region, ...);

// Social
public record PostDto(string Id, string Content, string? Mood, ...);
public record CreatePostRequest(string Content, string? Mood, ...);

// AI
public record ChatRequest(string Message);
public record MoodSuggestionRequest(string Mood, string? Location);
```

---

## Development Guidelines

### Coding Standards

#### 1. Naming Conventions
```csharp
// Classes: PascalCase
public class RestaurantService

// Methods: PascalCase
public async Task<Restaurant> GetByIdAsync(string id)

// Properties: PascalCase
public string DisplayName { get; private set; }

// Fields: camelCase with underscore
private readonly ILogger _logger;

// Constants: PascalCase
public const string DefaultRegion = "HaNoi";
```

#### 2. File Organization
```
Controller/
├── RestaurantsController.cs         # API endpoints
├── PostsController.cs               # Group related endpoints
└── ChatController.cs                # AI endpoints

Entities/
├── Restaurant.cs                    # Domain entities
├── Post.cs                          # Business logic
└── Account.cs                       # Domain rules

DTOs/
├── RestaurantDTOs.cs               # Group related DTOs
├── SocialDTOs.cs                   # Request/Response models
└── AuthDTOs.cs                     # Data transfer objects
```

#### 3. Error Handling
```csharp
// Controller level
try
{
    var result = await _service.ProcessAsync(request);
    return Ok(ApiResponse<T>.SuccessResult(result));
}
catch (ArgumentException ex)
{
    _logger.LogWarning("Validation failed: {Message}", ex.Message);
    return BadRequest(ApiResponse<T>.ErrorResult("Dữ liệu không hợp lệ", ex.Message));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error in {Method}", nameof(CreatePost));
    return StatusCode(500, ApiResponse<T>.ErrorResult("Lỗi hệ thống"));
}
```

#### 4. Logging Standards
```csharp
// Use structured logging
_logger.LogInformation("User {UserId} created post {PostId}", userId, postId);

// Log levels:
// - LogTrace: Very detailed debugging
// - LogDebug: Debugging information
// - LogInformation: General information
// - LogWarning: Something unexpected but not error
// - LogError: Error occurred but application continues
// - LogCritical: Critical error, application may abort
```

### Testing Guidelines

#### 1. Unit Tests
```csharp
[Test]
public async Task CreateRestaurant_WithValidData_ShouldReturnRestaurant()
{
    // Arrange
    var request = new CreateRestaurantRequest("Test Restaurant", "Test Address");
    
    // Act
    var result = await _controller.CreateRestaurant(request);
    
    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Value.Success, Is.True);
}
```

#### 2. Integration Tests
```csharp
[Test]
public async Task GetRestaurants_ShouldReturnPagedResults()
{
    // Test with real database
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DiscoveryDbContext>();
    
    // Seed test data
    // Execute request
    // Verify response
}
```

### Performance Guidelines

#### 1. Database Queries
```csharp
// ✅ Good: Use pagination
var restaurants = await _context.Restaurants
    .Where(r => !r.IsDeleted)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

// ❌ Bad: Load all data
var allRestaurants = await _context.Restaurants.ToListAsync();
```

#### 2. Async/Await
```csharp
// ✅ Good: Async all the way
public async Task<Restaurant> GetRestaurantAsync(string id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ Bad: Blocking async calls
public Restaurant GetRestaurant(string id)
{
    return _repository.GetByIdAsync(id).Result; // Don't do this
}
```

#### 3. Memory Management
```csharp
// ✅ Good: Use using statements
using var scope = _serviceProvider.CreateScope();
using var httpClient = _httpClientFactory.CreateClient();

// ✅ Good: Dispose resources
public class SomeService : IDisposable
{
    public void Dispose() => _resource?.Dispose();
}
```

### Security Guidelines

#### 1. JWT Security (theo chuẩn bithub.vn)
```csharp
// ✅ Good: Strong secret key (>= 256 bits)
JWT_SECRET_KEY=p2kQbYz7Jr9fT4wM1nV8sD6xC3aL5uH0rZ2eX9tQ1bN7mK8p...

// ✅ Good: Include standard claims
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, account.Id),
    new(ClaimTypes.Email, account.Email),
    new(ClaimTypes.Role, account.Role.ToString()),
    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
};

// ✅ Good: Proper token validation
var validationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
};
```

#### 2. Input Validation
```csharp
// ✅ Validate in domain entities
public static Restaurant Create(string name, string address)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Tên nhà hàng không được để trống");
    
    if (string.IsNullOrWhiteSpace(address))
        throw new ArgumentException("Địa chỉ không được để trống");
    
    return new Restaurant { Name = name.Trim(), Address = address.Trim() };
}
```

#### 2. Authentication
```csharp
// Use [Authorize] attribute
[Authorize]
[HttpPost]
public async Task<ActionResult> CreatePost([FromBody] CreatePostRequest request)
{
    var accountId = User.FindFirst("account_id")?.Value;
    // Process...
}
```

#### 3. SQL Injection Prevention
```csharp
// ✅ Good: Use parameterized queries (EF Core handles this)
var user = await _context.Accounts
    .FirstOrDefaultAsync(a => a.Email == email);

// ❌ Bad: Raw SQL with concatenation
var sql = $"SELECT * FROM accounts WHERE email = '{email}'"; // Don't do this
```

---

## Monitoring & Observability

### Health Checks
```csharp
// Startup configuration
builder.Services.AddHealthChecks()
    .AddDbContext<AuthDbContext>()
    .AddRabbitMQ()
    .AddRedis();

// Custom health check
public class GeminiHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context)
    {
        try
        {
            // Test Gemini API connectivity
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
```

### Metrics & Logging
```csharp
// Custom metrics
public class MetricsService
{
    private readonly Counter _requestCounter;
    private readonly Histogram _requestDuration;
    
    public void IncrementRequests(string endpoint) => _requestCounter.WithTags("endpoint", endpoint).Increment();
    public void RecordDuration(string endpoint, double duration) => _requestDuration.Record(duration);
}
```

## Package Management

### Central Package Management
Dự án sử dụng **Central Package Management** để tránh version conflicts:

```xml
<!-- Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- All package versions managed centrally -->
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.3" />
    <PackageVersion Include="System.Text.Json" Version="9.0.1" />
    <!-- ... other packages -->
  </ItemGroup>
</Project>
```

### Package Version Strategy
- **Microsoft.Extensions.***: Aligned to 9.0.3 (compatible with .NET 8)
- **ASP.NET Core**: Keep at 8.0.x for .NET 8 projects
- **Entity Framework**: 8.0.12 (latest stable for .NET 8)
- **Third-party**: Latest stable versions

### Fixing Package Conflicts
```bash
# Automated fix
.\scripts\fix-nuget-conflicts.ps1    # Windows
./scripts/fix-nuget-conflicts.sh     # Linux/Mac

# Manual steps
dotnet nuget locals all --clear      # Clear cache
rm -rf **/bin **/obj                 # Clean builds
dotnet restore                       # Restore with central management
dotnet build                         # Verify no conflicts
```

### Common NuGet Errors
- **NU1605**: Package downgrade - fixed by version alignment
- **NU1903**: Security vulnerability - fixed by updating to secure versions
- **NU1607**: Version conflict - resolved by central management

---

**📖 Tài liệu này sẽ được cập nhật thường xuyên khi codebase phát triển.**
