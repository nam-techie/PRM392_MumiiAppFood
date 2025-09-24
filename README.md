<h1 align="center">MUMII_MICROSERVICES_BE</h1>

<p align="center"><em>Building Scalable Food Discovery Platform with Modern Architecture</em></p>

<p align="center">
  <img src="https://img.shields.io/github/last-commit/your-username/mumii-microservices?style=flat&label=last%20commit" alt="Last Commit" />
  <img src="https://img.shields.io/badge/C%23-95.0%25-blue?style=flat&logo=csharp&logoColor=white" alt="C# 95%" />
  <img src="https://img.shields.io/github/languages/count/your-username/mumii-microservices?style=flat&label=languages" alt="Languages Count" />
  <img src="https://img.shields.io/badge/Microservices-4-green?style=flat" alt="Microservices Count" />
</p>

<p align="center"><em>Built with enterprise-grade tools and technologies:</em></p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp&logoColor=white" alt="C#" />
  <img src="https://img.shields.io/badge/MySQL-8.0-4479A1?style=flat&logo=mysql&logoColor=white" alt="MySQL" />
  <img src="https://img.shields.io/badge/Entity%20Framework-512BD4?style=flat&logo=dotnet&logoColor=white" alt="Entity Framework" />
  <img src="https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white" alt="Docker" />
  <img src="https://img.shields.io/badge/YARP-512BD4?style=flat&logo=dotnet&logoColor=white" alt="YARP" />
  <img src="https://img.shields.io/badge/RabbitMQ-FF6600?style=flat&logo=rabbitmq&logoColor=white" alt="RabbitMQ" />
  <img src="https://img.shields.io/badge/Redis-DC382D?style=flat&logo=redis&logoColor=white" alt="Redis" />
  <img src="https://img.shields.io/badge/JWT-000000?style=flat&logo=jsonwebtokens&logoColor=white" alt="JWT" />
  <img src="https://img.shields.io/badge/Swagger-85EA2D?style=flat&logo=swagger&logoColor=black" alt="Swagger" />
  <img src="https://img.shields.io/badge/Serilog-1C1C1C?style=flat&logo=serilog&logoColor=white" alt="Serilog" />
  <img src="https://img.shields.io/badge/Gemini%20AI-4285F4?style=flat&logo=google&logoColor=white" alt="Gemini AI" />
</p>

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [API Documentation](#api-documentation)
- [Database Schema](#database-schema)
- [Development](#development)
- [Monitoring](#monitoring)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

**Mumii** is a modern food discovery and social platform built with microservices architecture, empowering users to explore culinary experiences through intelligent recommendations and community sharing.

### Key Capabilities
- **Restaurant Discovery**: Location-based search with advanced filtering
- **Social Sharing**: Mood-driven food posts with community engagement  
- **AI-Powered Suggestions**: Gemini AI integration for personalized recommendations
- **Real-time Interactions**: Comments, reactions, and social features

## Features

### Core Services
- **Authentication Service**: JWT-based security with role management
- **Discovery Service**: Restaurant search, location services, and ratings
- **Social Service**: Posts, comments, reactions with mood tracking
- **AI Service**: Gemini AI chat for food recommendations and image analysis

### Technical Features
- **Microservices Architecture**: Clean separation of concerns
- **API-First Design**: Comprehensive OpenAPI/Swagger documentation
- **Event-Driven**: Asynchronous messaging with RabbitMQ
- **Containerized**: Docker & Docker Compose for easy deployment
- **Scalable**: Designed for horizontal scaling and high availability

### Tech Stack
```
Backend:      .NET 8, ASP.NET Core, Entity Framework Core
Database:     MySQL 8.0 with JSON support
Authentication: JWT Bearer Token with RS256
Messaging:    RabbitMQ for event-driven communication
Caching:      Redis for performance optimization
API Gateway:  YARP (Yet Another Reverse Proxy)
AI Integration: Google Gemini AI for intelligent features
Containerization: Docker & Docker Compose
Monitoring:   Serilog structured logging
Documentation: OpenAPI/Swagger specifications
```

---

## Kiến trúc

```
Mumii Microservices Architecture

Flutter App
    ↓ HTTP/REST
API Gateway (YARP) :8080
    ↓
┌─────────────────────────────────────────┐
│  Auth Service :8081                   │  ← JWT, Account Management
├─────────────────────────────────────────┤ 
│  Discovery Service :8082           │  ← Restaurant Search & Location
├─────────────────────────────────────────┤
│  Social Service :8083                 │  ← Posts, Comments, Reactions
└─────────────────────────────────────────┘
    ↓
MySQL (3 databases: auth, discovery, social)
RabbitMQ (async events)
Redis (caching)
```

### Core Services

| Service | Port | Chức năng | Database |
|---------|------|-----------|----------|
| **Auth** | 8081 | Authentication, User Management | `mumii_auth` |
| **Discovery** | 8082 | Restaurant Search, Location Services | `mumii_discovery` |
| **Social** | 8083 | Posts, Comments, Reactions | `mumii_social` |
| **AI** | 8084 | Gemini AI Chat, Food Suggestions | - |
| **Gateway** | 8080 | API Gateway, Load Balancing | - |

---

## Cài đặt nhanh

### **Yêu cầu hệ thống**
- Docker & Docker Compose
- .NET 8 SDK (optional, cho development)
- Git

### **1. Clone Repository**
```bash
git clone https://github.com/your-username/mumii-microservices.git
cd mumii-microservices
```

### **2. Khởi động Infrastructure**
```bash
# Khởi động database và message queue
docker-compose up -d mysql rabbitmq redis

# Chờ MySQL khởi động hoàn tất (khoảng 30-60 giây)
docker logs mumii-mysql -f
```

### **3. Khởi động Services**

#### **Option A: Docker Compose (Recommended)**
```bash
# Khởi động tất cả services
docker-compose up --build

# Hoặc chạy background
docker-compose up -d --build
```

#### **Option B: Local Development**
```bash
# Terminal 1: Auth Service
cd src/Services/Auth/Mumii.Auth.Api
dotnet run

# Terminal 2: Discovery Service  
cd src/Services/Discovery/Mumii.Discovery.Api
dotnet run

# Terminal 3: Social Service
cd src/Services/Social/Mumii.Social.Api
dotnet run

# Terminal 4: API Gateway
cd src/ApiGateway
dotnet run
```

### **4. Kiểm tra Services**
```bash
# Health checks
curl http://localhost:8080/health        # API Gateway
curl http://localhost:8081/health        # Auth Service
curl http://localhost:8082/health        # Discovery Service  
curl http://localhost:8083/health        # Social Service
curl http://localhost:8084/health        # AI Service

# Service info
curl http://localhost:8080/              # Gateway info
```

---

## API Documentation

### **Base URLs**
- **API Gateway**: `http://localhost:8080`
- **Auth Service**: `http://localhost:8081`
- **Discovery Service**: `http://localhost:8082`
- **Social Service**: `http://localhost:8083`
- **AI Service**: `http://localhost:8084`

### **Swagger Documentation**
- Auth API: http://localhost:8081/swagger
- Discovery API: http://localhost:8082/swagger
- Social API: http://localhost:8083/swagger
- AI API: http://localhost:8084/swagger

### **Quick API Examples**

#### **1. Authentication**
```bash
# Đăng ký tài khoản
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123",
    "displayName": "John Doe"
  }'

# Đăng nhập
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com", 
    "password": "password123"
  }'

# Response sẽ chứa access_token để sử dụng cho các API khác
```

#### **2. Restaurant Discovery**
```bash
# Lấy danh sách nhà hàng
curl http://localhost:8080/api/restaurants

# Tìm kiếm nhà hàng theo vị trí
curl "http://localhost:8080/api/restaurants/nearby?lat=21.0285&lng=105.8542&radiusKm=5"

# Tìm kiếm theo từ khóa
curl "http://localhost:8080/api/restaurants/search?q=phở"
```

#### **3. Social Posts**
```bash
# Tạo bài đăng (cần JWT token)
curl -X POST http://localhost:8080/api/posts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "content": "Hôm nay ăn phở ngon quá! 🍜",
    "mood": "SATISFIED",
    "imageUrls": ["https://example.com/pho.jpg"]
  }'

# Lấy feed posts
curl http://localhost:8080/api/posts
```

#### **4. AI Chat với Gemini**
```bash
# Chat về đồ ăn
curl -X POST http://localhost:8080/api/chat/food \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Hôm nay tôi muốn ăn gì đó ngon và healthy"
  }'

# Gợi ý theo mood
curl -X POST http://localhost:8080/api/chat/suggest-by-mood \
  -H "Content-Type: application/json" \
  -d '{
    "mood": "HAPPY",
    "location": "Hà Nội"
  }'

# Phân tích hình ảnh đồ ăn
curl -X POST http://localhost:8080/api/chat/analyze-image \
  -H "Content-Type: application/json" \
  -d '{
    "imageUrl": "https://example.com/food-image.jpg"
  }'
```

---

## Database Schema

### **Auth Database (`mumii_auth`)**
```sql
-- Accounts table
accounts (
  id VARCHAR(36) PRIMARY KEY,
  email VARCHAR(255) UNIQUE,
  password_hash VARCHAR(255),
  display_name VARCHAR(100),
  avatar_url VARCHAR(500),
  role ENUM('User', 'Admin'),
  is_active BOOLEAN,
  created_at TIMESTAMP,
  updated_at TIMESTAMP
)
```

### **Discovery Database (`mumii_discovery`)**
```sql
-- Restaurants table
restaurants (
  id VARCHAR(36) PRIMARY KEY,
  name VARCHAR(255),
  address TEXT,
  latitude DECIMAL(10,8),
  longitude DECIMAL(11,8),
  region VARCHAR(100),
  avg_price DECIMAL(10,2),
  rating DECIMAL(2,1),
  description TEXT,
  image_urls JSON,
  tags JSON,
  created_at TIMESTAMP,
  is_deleted BOOLEAN
)
```

### **Social Database (`mumii_social`)**
```sql
-- Posts table
posts (
  id VARCHAR(36) PRIMARY KEY,
  account_id VARCHAR(36),
  content TEXT,
  mood VARCHAR(50),
  image_urls JSON,
  restaurant_id VARCHAR(36),
  reaction_count INT,
  comment_count INT,
  created_at TIMESTAMP,
  is_deleted BOOLEAN
)

-- Comments & Reactions tables...
```

---

## Development

### **Project Structure**
```
Mumii.Microservices/
├── src/
│   ├── ApiGateway/                    # YARP API Gateway
│   ├── Services/
│   │   ├── Auth/                      # 🔐 Authentication Service
│   │   │   ├── Mumii.Auth.Api/        # → Web API
│   │   │   ├── Mumii.Auth.Domain/     # → Domain Logic
│   │   │   └── Mumii.Auth.Infrastructure/ # → Data Access
│   │   ├── Discovery/                 # 🏪 Restaurant Discovery
│   │   └── Social/                    # 📝 Social Features
│   └── Shared/
│       └── Common/                    # → DTOs, Events, Constants
├── docker/
├── scripts/                           # → Database init scripts
└── README.md
```

### **Development Commands**

#### **Database Operations**
```bash
# Tạo migration mới (ví dụ Auth service)
cd src/Services/Auth/Mumii.Auth.Infrastructure
dotnet ef migrations add InitialCreate -s ../Mumii.Auth.Api

# Apply migrations
dotnet ef database update -s ../Mumii.Auth.Api
```

#### **Docker Operations**
```bash
# Build specific service
docker-compose build auth-service

# View logs
docker-compose logs -f auth-service

# Restart services
docker-compose restart

# Clean up
docker-compose down -v  # Remove volumes too
```

#### **Testing**
```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test src/Services/Auth/Mumii.Auth.Tests/
```

### **Environment Variables**

#### **Auth Service**
```env
ConnectionStrings__DefaultConnection=Server=localhost;Database=mumii_auth;Uid=root;Pwd=mumii2024;
JWT_SECRET_KEY=your_jwt_secret_key_here
Jwt__Issuer=Mumii
Jwt__Audience=Mumii.Client
Jwt__ExpiryHours=24
```

#### **Discovery Service**
```env
ConnectionStrings__DefaultConnection=Server=localhost;Database=mumii_discovery;Uid=root;Pwd=mumii2024;
```

#### **Social Service**
```env
ConnectionStrings__DefaultConnection=Server=localhost;Database=mumii_social;Uid=root;Pwd=mumii2024;
```

---

## Monitoring

### **Health Checks**
Tất cả services đều có health check endpoints:
- `/health` - Overall health
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### **Management UIs**
- **RabbitMQ Management**: http://localhost:15672
  - Username: `admin`
  - Password: `mumii2024`

### **Logging**
- Sử dụng **Serilog** cho structured logging
- Logs được output ra console với format dễ đọc
- Production: có thể extend để ghi vào file hoặc ELK stack

---

## Deployment

### **Production Environment**

#### **Docker Compose Production**
```bash
# Sử dụng production compose file
docker-compose -f docker-compose.prod.yml up -d

# Scale services
docker-compose -f docker-compose.prod.yml up -d --scale auth-service=2
```

#### **Kubernetes (Future)**
```bash
# Deploy to Kubernetes cluster
kubectl apply -f k8s/

# Check deployment status
kubectl get pods -n mumii
```

### **CI/CD Pipeline**
Có thể tích hợp với:
- **GitHub Actions** cho automated testing & deployment
- **Azure DevOps** cho enterprise scenarios
- **Jenkins** cho on-premise setups

---

## Roadmap

### Phase 1 - Core Features (Completed)
- [x] Authentication với JWT
- [x] Discovery Service (restaurants search)
- [x] Social Service (posts, comments, reactions)
- [x] AI Service với Gemini integration
- [x] API Gateway với YARP
- [x] Docker containerization
- [x] Central Package Management
- [x] NuGet conflicts resolution

### Phase 2 - Enhanced Features
- [ ] Flutter mobile app
- [ ] Image upload service
- [ ] Push notifications
- [ ] User profile management
- [ ] Restaurant rating system

### Phase 3 - AI & Analytics
- [ ] AI recommendation engine
- [ ] User behavior analytics
- [ ] Advanced search với ML
- [ ] Personalized feed algorithm

### Phase 4 - Scale & Performance
- [ ] Event Sourcing
- [ ] CQRS pattern
- [ ] Distributed caching
- [ ] Kubernetes deployment
- [ ] Monitoring với Prometheus/Grafana

---

## Contributing

Chúng tôi rất hoan nghênh các contributions! 

### Development Workflow
1. Fork repository
2. Tạo feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open Pull Request

### Coding Standards
- Sử dụng **Clean Architecture** patterns
- Tuân thủ **SOLID principles**
- Code coverage tối thiểu 80%
- Sử dụng **conventional commits**

### Issue Templates
Khi tạo issue, vui lòng sử dụng các templates:
- Bug Report
- Feature Request
- Documentation
- Question

---

## Support & Contact

- **Email**: support@mumii.com
- **Discord**: [Mumii Community](https://discord.gg/mumii)
- **Issues**: [GitHub Issues](https://github.com/your-username/mumii-microservices/issues)
- **Wiki**: [Project Wiki](https://github.com/your-username/mumii-microservices/wiki)

---

## License

Dự án này sử dụng MIT License. Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

---

## Acknowledgments

- **Microsoft** cho .NET 8 và Entity Framework Core
- **Docker** cho containerization platform
- **MySQL** cho reliable database
- **RabbitMQ** cho message queuing
- **Community** cho các open source packages tuyệt vời

---

<div align="center">

**Nếu project này hữu ích, hãy cho chúng tôi một star!**

Made with love by **Mumii Team**

[Back to top](#mumii---food-discovery--social-platform)

</div>
