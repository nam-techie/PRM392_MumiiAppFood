<h1 align="center" id="top">🍜 MUMII APP FOOD - Microservices Backend</h1>

<p align="center"><em>Nền tảng khám phá ẩm thực hiện đại được xây dựng với kiến trúc Microservices</em></p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/C%23-12-239120?style=flat&logo=csharp&logoColor=white" alt="C# 12" />
  <img src="https://img.shields.io/badge/MongoDB-47A248?style=flat&logo=mongodb&logoColor=white" alt="MongoDB" />
  <img src="https://img.shields.io/badge/SQLite-003B57?style=flat&logo=sqlite&logoColor=white" alt="SQLite" />
  <img src="https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white" alt="Docker" />
  <img src="https://img.shields.io/badge/YARP-512BD4?style=flat&logo=dotnet&logoColor=white" alt="YARP" />
  <img src="https://img.shields.io/badge/JWT-000000?style=flat&logo=jsonwebtokens&logoColor=white" alt="JWT" />
  <img src="https://img.shields.io/badge/Gemini%20AI-4285F4?style=flat&logo=google&logoColor=white" alt="Gemini AI" />
</p>

## 📋 Mục lục

- [Tổng quan dự án](#-tổng-quan-dự-án)
- [Tính năng chính](#-tính-năng-chính)
- [Kiến trúc hệ thống](#-kiến-trúc-hệ-thống)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Hướng dẫn cài đặt](#-hướng-dẫn-cài-đặt)
- [Hướng dẫn cấu hình](#-hướng-dẫn-cấu-hình)
- [Hướng dẫn chạy Backend](#-hướng-dẫn-chạy-backend)
- [Hướng dẫn sử dụng API](#-hướng-dẫn-sử-dụng-api)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Tài liệu tham khảo](#-tài-liệu-tham-khảo)
- [Troubleshooting](#-troubleshooting)
- [Đóng góp](#-đóng-góp)

---

## 🎯 Tổng quan dự án

**Mumii App Food** là một nền tảng khám phá ẩm thực và mạng xã hội hiện đại, được xây dựng với kiến trúc **Microservices** và **Clean Architecture**. Dự án cho phép người dùng khám phá các nhà hàng, chia sẻ trải nghiệm ẩm thực, và nhận gợi ý thông minh từ AI.

### Khả năng chính
- 🔍 **Khám phá nhà hàng**: Tìm kiếm theo vị trí, bộ lọc nâng cao
- 📱 **Mạng xã hội**: Chia sẻ bài đăng, tương tác với cộng đồng
- 🤖 **Gợi ý AI**: Tích hợp Google Gemini AI cho gợi ý cá nhân hóa
- 💬 **Tương tác thời gian thực**: Bình luận, phản ứng, theo dõi

---

## ✨ Tính năng chính

### 🔐 Authentication Service (Port 8081)
- Đăng ký và đăng nhập người dùng
- Xác thực JWT với phân quyền theo vai trò (User/Admin)
- Quản lý profile và thông tin tài khoản
- Xác thực OAuth (Google) - tùy chọn
- Gửi email xác thực và thông báo

### 🏪 Discovery Service (Port 8082)
- Quản lý thông tin nhà hàng (CRUD)
- Tìm kiếm nhà hàng theo từ khóa, vị trí
- Tìm kiếm theo khoảng cách địa lý (Geolocation)
- Hệ thống đánh giá và review
- Quản lý favorite/đánh dấu nhà hàng yêu thích

### 📝 Social Service (Port 8083)
- Tạo và quản lý bài đăng (Posts) với mood tracking
- Hệ thống bình luận và reply
- Reactions (Like, Love, Wow)
- Feed cá nhân hóa
- Tương tác giữa người dùng

### 🤖 AI Service (Port 8084)
- Chat với AI về ẩm thực
- Gợi ý món ăn theo tâm trạng (mood)
- Phân tích hình ảnh món ăn
- Gợi ý nhà hàng thông minh
- Tích hợp Google Gemini AI

### 🌐 API Gateway (Port 8080)
- Cổng vào duy nhất cho tất cả client
- Load balancing và routing
- Centralized Swagger UI
- Health checks và monitoring

---

## 🏛️ Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Applications                      │
│              (Flutter Mobile App / Web App)                 │
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTP/REST
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              🌐 API Gateway (YARP) :8080                     │
│         - Routing & Load Balancing                           │
│         - Centralized Swagger UI                            │
│         - Health Checks                                      │
└───┬──────────┬──────────┬──────────┬────────────────────────┘
    │          │          │          │
    │          │          │          │
    ▼          ▼          ▼          ▼
┌────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│ Auth   │ │Discovery │ │ Social   │ │   AI     │
│ :8081  │ │  :8082   │ │  :8083   │ │  :8084   │
└───┬────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘
    │          │             │             │
    │          │             │             │
    ▼          ▼             ▼             ▼
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                 │
│  │ MongoDB  │  │ RabbitMQ │  │  Redis   │                 │
│  │ (3 DBs)  │  │ (Events) │  │ (Cache)  │                 │
│  └──────────┘  └──────────┘  └──────────┘                 │
│  mumii_auth                                                  │
│  mumii_discovery                                             │
│  mumii_social                                                │
└─────────────────────────────────────────────────────────────┘
```

### Clean Architecture Layers

Mỗi service tuân theo **Clean Architecture** với 3 layers:

```
Mumii.{Service}/
├── 📁 Api/              # API Layer (Controllers, Program.cs)
├── 📁 Domain/           # Domain Layer (Entities, Interfaces)
└── 📁 Infrastructure/   # Infrastructure Layer (Repositories, Services)
```

**Dependency Flow**: Api → Domain ← Infrastructure  
**Nguyên tắc**: Domain không phụ thuộc vào bất kỳ framework nào

### Core Services Overview

| Service | Port | Database | Chức năng chính |
|---------|------|----------|-----------------|
| **Auth** | 8081 | `mumii_auth` | Authentication, User Management, Profile |
| **Discovery** | 8082 | `mumii_discovery` | Restaurant Search, Location Services |
| **Social** | 8083 | `mumii_social` | Posts, Comments, Reactions |
| **AI** | 8084 | - | Gemini AI Integration |
| **Gateway** | 8080 | - | API Routing, Load Balancing |

---

## 🛠️ Công nghệ sử dụng

### Backend Stack
- **.NET 8 (LTS)** - Framework chính
- **C# 12** - Ngôn ngữ lập trình
- **ASP.NET Core 8** - Web framework
- **Entity Framework Core 8** - ORM

### Database
- **MongoDB** - Database chính (Production)
- **SQLite** - Database cho development nhanh
- **MySQL 8.0** - Tùy chọn (qua Docker)

### Infrastructure
- **Docker & Docker Compose** - Containerization
- **YARP (Yet Another Reverse Proxy)** - API Gateway
- **RabbitMQ** - Message Queue (tương lai)
- **Redis** - Caching (tương lai)

### Authentication & Security
- **JWT (JSON Web Tokens)** - Authentication
- **BCrypt** - Password hashing
- **OAuth 2.0** - Social login (tùy chọn)

### AI Integration
- **Google Gemini AI** - AI chat và gợi ý

### Development Tools
- **Swagger/OpenAPI** - API Documentation
- **Serilog** - Structured Logging
- **Central Package Management** - NuGet package management

---

## 💻 Yêu cầu hệ thống

### Bắt buộc
- ✅ **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ **Docker Desktop** (tùy chọn, cho MySQL/RabbitMQ) - [Download](https://www.docker.com/products/docker-desktop/)
- ✅ **Git** - [Download](https://git-scm.com/downloads)

### Tùy chọn (cho MongoDB)
- 🔧 **MongoDB Atlas Account** (miễn phí) hoặc MongoDB local
- 🔧 **Google Gemini API Key** - [Lấy từ Google AI Studio](https://makersuite.google.com/app/apikey)

### Development Tools
- 📝 **Visual Studio 2022** hoặc **VS Code** với C# extension
- 📝 **Postman** hoặc **Insomnia** - Để test API

---

## 📥 Hướng dẫn cài đặt

### Bước 1: Clone Repository

```bash
# Clone repository từ GitHub
git clone https://github.com/your-username/mumii-microservices.git

# Di chuyển vào thư mục dự án
cd mumii-microservices
```

### Bước 2: Kiểm tra .NET SDK

```bash
# Kiểm tra phiên bản .NET
dotnet --version
# Phải >= 8.0.0

# Restore packages
dotnet restore
```

### Bước 3: Verify Solution Build

```bash
# Build toàn bộ solution để đảm bảo không có lỗi
dotnet build Mumii.Microservices.sln
```

---

## ⚙️ Hướng dẫn cấu hình

### 1. Cấu hình Database

Dự án hỗ trợ **2 loại database**:

#### Option A: SQLite (Khuyên dùng cho Development)

**Ưu điểm**: Không cần setup, chạy ngay lập tức

Database files sẽ được tạo tự động trong mỗi service:
- `src/Services/Auth/Mumii.Auth.Api/auth.db`
- `src/Services/Discovery/Mumii.Discovery.Api/discovery.db`
- `src/Services/Social/Mumii.Social.Api/social.db`

**Cấu hình mặc định** (đã có sẵn trong `appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=auth.db"
  }
}
```

#### Option B: MongoDB (Production)

1. **Tạo MongoDB Atlas Account** (miễn phí):
   - Truy cập: https://www.mongodb.com/cloud/atlas
   - Tạo cluster miễn phí
   - Lấy Connection String

2. **Cấu hình trong `appsettings.json`**:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb+srv://username:password@cluster.mongodb.net/?retryWrites=true&w=majority",
    "DatabaseName": "mumii_auth"
  }
}
```

3. **Lặp lại cho các services**:
   - `Mumii.Auth.Api/appsettings.json` → `mumii_auth`
   - `Mumii.Discovery.Api/appsettings.json` → `mumii_discovery`
   - `Mumii.Social.Api/appsettings.json` → `mumii_social`

#### Option C: MySQL (qua Docker)

Nếu muốn dùng MySQL, xem [docs/SETUP.md](docs/SETUP.md)

### 2. Cấu hình Gemini AI

1. **Lấy API Key**:
   - Truy cập: https://makersuite.google.com/app/apikey
   - Đăng nhập với Google account
   - Tạo API key mới
   - Copy API key

2. **Cấu hình trong `Mumii.AI.Api/appsettings.json`**:

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
  }
}
```

Hoặc sử dụng **Environment Variable**:
```bash
# Windows PowerShell
$env:Gemini__ApiKey="YOUR_GEMINI_API_KEY_HERE"

# Linux/Mac
export Gemini__ApiKey="YOUR_GEMINI_API_KEY_HERE"
```

### 3. Cấu hình JWT (Authentication)

Cấu hình trong `Mumii.Auth.Api/appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "Mumii",
    "Audience": "Mumii.Client",
    "Key": "YOUR_STRONG_SECRET_KEY_HERE_MIN_32_CHARS",
    "ExpiryHours": 24
  }
}
```

**Lưu ý bảo mật**:
- Key phải có độ dài tối thiểu 32 ký tự
- Sử dụng key khác nhau cho Production
- Không commit key vào Git

### 4. Cấu hình API Gateway

File: `src/ApiGateway/appsettings.json`

```json
{
  "ReverseProxy": {
    "Routes": {
      "auth-route": {
        "ClusterId": "auth-cluster",
        "Match": {
          "Path": "/api/auth/{**catch-all}"
        }
      },
      "discovery-route": {
        "ClusterId": "discovery-cluster",
        "Match": {
          "Path": "/api/restaurants/{**catch-all}"
        }
      },
      "social-route": {
        "ClusterId": "social-cluster",
        "Match": {
          "Path": "/api/posts/{**catch-all}"
        }
      },
      "ai-route": {
        "ClusterId": "ai-cluster",
        "Match": {
          "Path": "/api/chat/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "auth-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:8081"
          }
        }
      },
      "discovery-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:8082"
          }
        }
      },
      "social-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:8083"
          }
        }
      },
      "ai-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:8084"
          }
        }
      }
    }
  }
}
```

### 5. Environment Variables (Tùy chọn)

Tạo file `.env` trong thư mục root (nếu sử dụng):

```bash
# Gemini AI
GEMINI_API_KEY=your_gemini_api_key_here

# MongoDB (nếu dùng)
MONGODB_CONNECTION_STRING=mongodb+srv://...

# JWT
JWT_SECRET_KEY=your_jwt_secret_key_here

# MySQL (nếu dùng Docker)
MYSQL_ROOT_PASSWORD=mumii2024
RABBITMQ_DEFAULT_USER=admin
RABBITMQ_DEFAULT_PASS=mumii2024
```

---

## 🚀 Hướng dẫn chạy Backend

### Phương pháp 1: SQLite (Khuyên dùng - Nhanh nhất)

**Ưu điểm**: Không cần Docker, chạy ngay lập tức

#### Windows PowerShell:

```powershell
# Chạy script tự động (khởi động tất cả services)
.\scripts\run-sqlite.ps1
```

#### Linux/Mac:

```bash
# Chạy script tự động
chmod +x scripts/run-sqlite.sh
./scripts/run-sqlite.sh
```

#### Chạy thủ công (từng service):

Mở **5 terminal windows** và chạy từng service:

```bash
# Terminal 1: Auth Service
cd src/Services/Auth/Mumii.Auth.Api
dotnet run --urls "http://localhost:8081"

# Terminal 2: Discovery Service
cd src/Services/Discovery/Mumii.Discovery.Api
dotnet run --urls "http://localhost:8082"

# Terminal 3: Social Service
cd src/Services/Social/Mumii.Social.Api
dotnet run --urls "http://localhost:8083"

# Terminal 4: AI Service
cd src/Services/AI/Mumii.AI.Api
dotnet run --urls "http://localhost:8084"

# Terminal 5: API Gateway
cd src/ApiGateway
dotnet run --urls "http://localhost:8080"
```

**Lưu ý**: Chờ tất cả services khởi động hoàn tất trước khi test API.

### Phương pháp 2: Docker Compose (Production-like)

#### Bước 1: Khởi động Infrastructure

```bash
# Khởi động MySQL, RabbitMQ, Redis
docker-compose up -d mysql rabbitmq redis

# Chờ MySQL sẵn sàng (30-60 giây)
docker logs mumii-mysql -f
# Thấy "ready for connections" là được
```

#### Bước 2: Khởi động Services

```bash
# Khởi động tất cả services
docker-compose up --build

# Hoặc chạy background
docker-compose up -d --build
```

#### Bước 3: Kiểm tra Services

```bash
# Xem logs
docker-compose logs -f

# Kiểm tra containers
docker ps
```

### Phương pháp 3: Chạy Local (Không Docker)

#### Bước 1: Cấu hình Database

Chọn một trong các option:
- SQLite: Không cần setup gì
- MongoDB: Cấu hình connection string trong `appsettings.json`

#### Bước 2: Chạy từng Service

Mở 5 terminal windows và chạy:

```bash
# Terminal 1: Auth Service
cd src/Services/Auth/Mumii.Auth.Api
dotnet run --urls "http://localhost:8081"

# Terminal 2: Discovery Service
cd src/Services/Discovery/Mumii.Discovery.Api
dotnet run --urls "http://localhost:8082"

# Terminal 3: Social Service
cd src/Services/Social/Mumii.Social.Api
dotnet run --urls "http://localhost:8083"

# Terminal 4: AI Service
cd src/Services/AI/Mumii.AI.Api
dotnet run --urls "http://localhost:8084"

# Terminal 5: API Gateway
cd src/ApiGateway
dotnet run --urls "http://localhost:8080"
```

### Development Mode với Hot Reload

Sử dụng `dotnet watch` để tự động reload khi có thay đổi:

```bash
# Thay vì dotnet run, dùng:
dotnet watch run --urls "http://localhost:8081"
```

---

## ✅ Kiểm tra Services đã chạy thành công

### 1. Health Checks

```bash
# Kiểm tra tất cả services
curl http://localhost:8080/health        # API Gateway
curl http://localhost:8081/health        # Auth Service
curl http://localhost:8082/health        # Discovery Service
curl http://localhost:8083/health       # Social Service
curl http://localhost:8084/health       # AI Service
```

**Expected Response**: `{"status":"Healthy"}` hoặc tương tự

### 2. Service Info

```bash
# API Gateway info
curl http://localhost:8080/

# Response example:
{
  "service": "Mumii API Gateway",
  "version": "1.0.0",
  "status": "Running",
  "routes": {
    "auth": "/api/auth/*",
    "discovery": "/api/restaurants/*",
    "social": "/api/posts/*",
    "ai": "/api/chat/*"
  }
}
```

### 3. Swagger UI

Mở browser và truy cập:

- **Centralized Swagger**: http://localhost:8080
  - Chứa tất cả API endpoints của các services
- **Auth Service**: http://localhost:8081/swagger
- **Discovery Service**: http://localhost:8082/swagger
- **Social Service**: http://localhost:8083/swagger
- **AI Service**: http://localhost:8084/swagger

---

## 📚 Hướng dẫn sử dụng API

### Base URLs

| Service | Base URL | Swagger UI |
|---------|----------|------------|
| **API Gateway** | `http://localhost:8080` | http://localhost:8080 |
| **Auth Service** | `http://localhost:8081` | http://localhost:8081/swagger |
| **Discovery Service** | `http://localhost:8082` | http://localhost:8082/swagger |
| **Social Service** | `http://localhost:8083` | http://localhost:8083/swagger |
| **AI Service** | `http://localhost:8084` | http://localhost:8084/swagger |

**Khuyến nghị**: Sử dụng **API Gateway** (`http://localhost:8080`) cho tất cả requests.

### Authentication Flow

#### 1. Đăng ký tài khoản

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123",
    "displayName": "John Doe"
  }'
```

**Response**:
```json
{
  "success": true,
  "message": "Đăng ký thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "user": {
      "id": 1,
      "email": "user@example.com",
      "displayName": "John Doe",
      "role": "User"
    }
  }
}
```

#### 2. Đăng nhập

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

**Lưu `accessToken` từ response để dùng cho các API protected.**

#### 3. Lấy Profile (Protected Endpoint)

```bash
curl -X GET http://localhost:8080/api/auth/profile \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

### Restaurant Discovery APIs

#### 1. Lấy danh sách nhà hàng

```bash
curl http://localhost:8080/api/restaurants?page=1&pageSize=10
```

#### 2. Tìm kiếm nhà hàng

```bash
curl "http://localhost:8080/api/restaurants/search?q=phở&page=1&pageSize=10"
```

#### 3. Tìm nhà hàng gần vị trí

```bash
curl "http://localhost:8080/api/restaurants/nearby?lat=21.0285&lng=105.8542&radiusKm=5"
```

#### 4. Lấy chi tiết nhà hàng

```bash
curl http://localhost:8080/api/restaurants/1
```

### Social APIs

#### 1. Tạo bài đăng (Protected)

```bash
curl -X POST http://localhost:8080/api/posts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE" \
  -d '{
    "content": "Hôm nay ăn phở ngon quá! 🍜",
    "mood": "SATISFIED",
    "imageUrls": ["https://example.com/pho.jpg"]
  }'
```

#### 2. Lấy feed posts

```bash
curl http://localhost:8080/api/posts?page=1&pageSize=20
```

#### 3. Thêm bình luận (Protected)

```bash
curl -X POST http://localhost:8080/api/posts/1/comments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE" \
  -d '{
    "content": "Nhìn ngon quá!"
  }'
```

#### 4. Reaction (Like/Love/Wow) (Protected)

```bash
curl -X POST http://localhost:8080/api/posts/1/react \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE" \
  -d '{
    "reactionType": "LIKE"
  }'
```

### AI Service APIs

#### 1. Chat về đồ ăn

```bash
curl -X POST http://localhost:8080/api/chat/food \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Hôm nay tôi muốn ăn gì đó ngon và healthy"
  }'
```

#### 2. Gợi ý theo mood

```bash
curl -X POST http://localhost:8080/api/chat/suggest-by-mood \
  -H "Content-Type: application/json" \
  -d '{
    "mood": "HAPPY",
    "location": "Hà Nội"
  }'
```

#### 3. Phân tích hình ảnh món ăn

```bash
curl -X POST http://localhost:8080/api/chat/analyze-image \
  -H "Content-Type: application/json" \
  -d '{
    "imageUrl": "https://example.com/food-image.jpg"
  }'
```

---

## 📁 Cấu trúc dự án

```
Mumii.Microservices/
├── 📁 src/
│   ├── 📁 ApiGateway/                  # 🌐 API Gateway (YARP)
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   │
│   ├── 📁 Services/
│   │   ├── 📁 Auth/                     # 🔐 Authentication Service
│   │   │   ├── Mumii.Auth.Api/          # → API Layer
│   │   │   │   ├── Controllers/
│   │   │   │   │   ├── AccountsController.cs
│   │   │   │   │   ├── AuthController.cs
│   │   │   │   │   └── ...
│   │   │   │   ├── Program.cs
│   │   │   │   └── appsettings.json
│   │   │   ├── Mumii.Auth.Domain/       # → Domain Layer
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Account.cs
│   │   │   │   │   └── ...
│   │   │   │   └── Interfaces/
│   │   │   │       ├── IAccountRepository.cs
│   │   │   │       └── ...
│   │   │   └── Mumii.Auth.Infrastructure/ # → Infrastructure Layer
│   │   │       ├── Repositories/
│   │   │       ├── Services/
│   │   │       │   ├── JwtService.cs
│   │   │       │   └── ...
│   │   │       └── Data/
│   │   │
│   │   ├── 📁 Discovery/                # 🏪 Restaurant Discovery Service
│   │   │   ├── Mumii.Discovery.Api/
│   │   │   ├── Mumii.Discovery.Domain/
│   │   │   └── Mumii.Discovery.Infrastructure/
│   │   │
│   │   ├── 📁 Social/                    # 📝 Social Service
│   │   │   ├── Mumii.Social.Api/
│   │   │   ├── Mumii.Social.Domain/
│   │   │   └── Mumii.Social.Infrastructure/
│   │   │
│   │   └── 📁 AI/                        # 🤖 AI Service
│   │       ├── Mumii.AI.Api/
│   │       ├── Mumii.AI.Domain/
│   │       └── Mumii.AI.Infrastructure/
│   │
│   └── 📁 Shared/
│       └── 📁 Common/                    # 📦 Shared Library
│           ├── DTOs/
│           │   ├── AuthDTOs.cs
│           │   ├── DiscoveryDTOs.cs
│           │   └── SocialDTOs.cs
│           ├── Models/
│           │   └── ApiResponse.cs
│           ├── Enums/
│           ├── Constants/
│           └── Events/
│
├── 📁 scripts/                           # 🔧 Utility Scripts
│   ├── run-sqlite.ps1                    # Chạy với SQLite (Windows)
│   ├── run-sqlite.sh                     # Chạy với SQLite (Linux/Mac)
│   └── stop-sqlite.ps1                   # Dừng services
│
├── 📁 docs/                              # 📚 Documentation
│   ├── SETUP.md                          # Chi tiết setup
│   ├── CODEBASE.md                       # Chi tiết codebase
│   ├── API_DOCUMENTATION.md              # Tài liệu API
│   └── QUICK_START.md                    # Hướng dẫn nhanh
│
├── 📄 docker-compose.yml                  # Docker Compose config
├── 📄 Mumii.Microservices.sln            # Solution file
├── 📄 Directory.Packages.props           # Central Package Management
├── 📄 AGENTS.md                          # Quy tắc cho AI Agent
└── 📄 README.md                          # File này
```

### Dependency Graph

```
ApiGateway
    ↓
┌───┴─────────────────────────────────────┐
│                                         │
Auth.Api    Discovery.Api    Social.Api   AI.Api
    ↓              ↓              ↓           ↓
┌───┴───┐    ┌────┴────┐    ┌────┴────┐      │
│Domain │    │ Domain  │    │ Domain  │      │
└───┬───┘    └────┬────┘    └────┬────┘      │
    │             │              │           │
┌───┴─────────────┴──────────────┴──────────┘
│         Infrastructure                    │
└───────────────────────────────────────────┘
            ↑
    Shared.Common
```

---

## 📖 Tài liệu tham khảo

- 📄 [Setup Guide](docs/SETUP.md) - Hướng dẫn setup chi tiết
- 📄 [Codebase Documentation](docs/CODEBASE.md) - Chi tiết về codebase
- 📄 [API Documentation](docs/API_DOCUMENTATION.md) - Tài liệu API đầy đủ
- 📄 [Quick Start](docs/QUICK_START.md) - Hướng dẫn nhanh 5 phút
- 📄 [AGENTS.md](AGENTS.md) - Quy tắc và kiến trúc cho AI Agent

---

## 🔧 Troubleshooting

### 1. Lỗi Port đã được sử dụng

**Triệu chứng**: `Address already in use` hoặc `Port is already in use`

**Giải pháp**:
```bash
# Windows
netstat -ano | findstr :8080
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :8080
kill -9 <PID>
```

### 2. Database không kết nối được

**Triệu chứng**: `Unable to connect to database`

**Giải pháp SQLite**:
- Kiểm tra file database có tồn tại
- Kiểm tra quyền ghi file trong thư mục service
- Xóa file `.db` cũ và chạy lại để tạo mới

**Giải pháp MongoDB**:
- Kiểm tra connection string trong `appsettings.json`
- Kiểm tra network có kết nối internet (nếu dùng Atlas)
- Kiểm tra IP whitelist trong MongoDB Atlas

### 3. Gemini API không hoạt động

**Triệu chứng**: `401 Unauthorized` hoặc `Invalid API key`

**Giải pháp**:
- Kiểm tra API key trong `Mumii.AI.Api/appsettings.json`
- Đảm bảo API key hợp lệ từ [Google AI Studio](https://makersuite.google.com/app/apikey)
- Kiểm tra quota của API key

### 4. JWT Token không hợp lệ

**Triệu chứng**: `401 Unauthorized` khi gọi API protected

**Giải pháp**:
- Đảm bảo token được lấy từ `/api/auth/login`
- Kiểm tra header: `Authorization: Bearer <token>`
- Kiểm tra token chưa hết hạn
- Đăng nhập lại để lấy token mới

### 5. Services không khởi động được

**Triệu chứng**: Services crash ngay sau khi start

**Giải pháp**:
```bash
# Kiểm tra logs
# Windows PowerShell
Get-Content -Path "logs\*.log" -Tail 50

# Linux/Mac
tail -f logs/*.log

# Kiểm tra cấu hình
# Xem lại appsettings.json của service bị lỗi
```

### 6. Package conflicts (NU1605, NU1903)

**Triệu chứng**: Build lỗi với NuGet packages

**Giải pháp**:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Clean build
dotnet clean
dotnet restore
dotnet build

# Nếu vẫn lỗi, xóa bin/obj
find . -type d -name "bin" -exec rm -rf {} +
find . -type d -name "obj" -exec rm -rf {} +
dotnet restore
dotnet build
```

### 7. Docker containers không start

**Triệu chứng**: Containers exit ngay sau khi start

**Giải pháp**:
```bash
# Xem logs
docker logs mumii-auth-service
docker logs mumii-mysql

# Kiểm tra Docker đang chạy
docker ps -a

# Restart Docker Desktop (Windows/Mac)
# Hoặc
sudo systemctl restart docker  # Linux

# Clean và rebuild
docker-compose down -v
docker-compose up --build
```

---

## 🤝 Đóng góp

Chúng tôi rất hoan nghênh các contributions!

### Development Workflow

1. **Fork repository**
2. **Tạo feature branch**: `git checkout -b feature/amazing-feature`
3. **Commit changes**: `git commit -m 'Add amazing feature'`
4. **Push to branch**: `git push origin feature/amazing-feature`
5. **Open Pull Request**

### Coding Standards

- Sử dụng **Clean Architecture** patterns
- Tuân thủ **SOLID principles**
- Code coverage tối thiểu 80%
- Sử dụng **conventional commits**
- Đọc [AGENTS.md](AGENTS.md) để hiểu quy tắc code

### Issue Reporting

Khi tạo issue, vui lòng cung cấp:
- Mô tả vấn đề rõ ràng
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment (OS, .NET version, etc.)

---

## 📝 License

Dự án này sử dụng **MIT License**. Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

---

## 🙏 Acknowledgments

- **Microsoft** cho .NET 8 và Entity Framework Core
- **Docker** cho containerization platform
- **MongoDB** cho database solution
- **Google** cho Gemini AI
- **Community** cho các open source packages tuyệt vời

---

<div align="center">

**⭐ Nếu project này hữu ích, hãy cho chúng tôi một star! ⭐**

Made with ❤️ by **Mumii Team**

[Back to top](#top)

</div>
