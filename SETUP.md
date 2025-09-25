# Mumii Microservices - Setup & Configuration Guide

Hướng dẫn chi tiết để setup và cấu hình dự án Mumii Microservices từ đầu.

## Mục lục
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt môi trường](#cài-đặt-môi-trường)
- [Environment Variables](#environment-variables)
- [Database Setup](#database-setup)
- [Khởi chạy dự án](#khởi-chạy-dự-án)
- [Kiểm tra hoạt động](#kiểm-tra-hoạt-động)
- [Troubleshooting](#troubleshooting)

---

## Yêu cầu hệ thống

### Bắt buộc
- **Docker Desktop** 4.0+ - [Download](https://www.docker.com/products/docker-desktop/)
- **Git** - [Download](https://git-scm.com/downloads)
- **Gemini API Key** - [Lấy từ Google AI Studio](https://makersuite.google.com/app/apikey)

### Tùy chọn (cho development)
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** hoặc **VS Code**
- **MySQL Workbench** cho quản lý database

---

## Cài đặt môi trường

### 1. Clone Repository
```bash
git clone https://github.com/your-username/mumii-microservices.git
cd mumii-microservices
```

### 2. Kiểm tra Docker
```bash
# Kiểm tra Docker đang chạy
docker --version
docker-compose --version

# Nếu chưa có, khởi động Docker Desktop
```

### 3. Cấu hình Environment Variables
```bash
# Tạo file .env từ template
cp env.example .env

# Sửa file .env với các giá trị thực tế
notepad .env   # Windows
nano .env      # Linux/Mac
```

---

## Environment Variables

### Tạo file `.env`
Tạo file `.env` trong thư mục root với nội dung:

```bash
# ================================
# Mumii Microservices Environment
# ================================

# 🤖 GEMINI AI CONFIGURATION
# Lấy API key từ: https://makersuite.google.com/app/apikey
GEMINI_API_KEY=your_gemini_api_key_here

# 🗄️ DATABASE CONFIGURATION
MYSQL_ROOT_PASSWORD=mumii2024
MYSQL_DATABASE=mumii_auth

# 📡 RABBITMQ CONFIGURATION
RABBITMQ_DEFAULT_USER=admin
RABBITMQ_DEFAULT_PASS=mumii2024

# 🔐 JWT CONFIGURATION
JWT_SECRET_KEY=MumiiSecretKey123456789012345678901234567890
JWT_ISSUER=Mumii
JWT_AUDIENCE=Mumii.Client
JWT_EXPIRY_HOURS=24

# 🗄️ REDIS CONFIGURATION
REDIS_PASSWORD=mumii2024

# 🌐 SERVICE URLS (Development)
API_GATEWAY_URL=http://localhost:8080
AUTH_SERVICE_URL=http://localhost:8081
DISCOVERY_SERVICE_URL=http://localhost:8082
SOCIAL_SERVICE_URL=http://localhost:8083
AI_SERVICE_URL=http://localhost:8084

# 📊 LOGGING LEVEL
LOG_LEVEL=Information
```

### Các biến môi trường quan trọng

| Biến | Mô tả | Bắt buộc | Giá trị mặc định |
|------|-------|----------|------------------|
| `GEMINI_API_KEY` | API key từ Google AI Studio | ✅ | - |
| `MYSQL_ROOT_PASSWORD` | Mật khẩu MySQL root | ✅ | `mumii2024` |
| `RABBITMQ_DEFAULT_USER` | Username RabbitMQ | ✅ | `admin` |
| `RABBITMQ_DEFAULT_PASS` | Password RabbitMQ | ✅ | `mumii2024` |
| `JWT_SECRET_KEY` | Secret key cho JWT | ✅ | (đã cung cấp) |

### Lấy Gemini API Key

1. Truy cập [Google AI Studio](https://makersuite.google.com/app/apikey)
2. Đăng nhập với Google account
3. Click "Create API Key"
4. Copy API key và paste vào file `.env`

```bash
# Ví dụ:
GEMINI_API_KEY=AIzaSyCXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

---

## Database Setup

### Cấu trúc Database
Dự án sử dụng 3 databases riêng biệt:

```
📦 MySQL Instance
├── 🔐 mumii_auth      (Authentication data)
├── 🏪 mumii_discovery (Restaurant data)  
└── 📝 mumii_social    (Posts, comments, reactions)
```

### Schema tự động
- Database schemas được tạo tự động khi khởi động services
- Sample data được seed sẵn trong `scripts/01-init-databases.sql`
- Không cần setup thủ công

### Kết nối Database (tùy chọn)
```bash
# MySQL connection details
Host: localhost
Port: 3306
Username: root
Password: mumii2024

# Databases:
# - mumii_auth
# - mumii_discovery  
# - mumii_social
```

---

## Khởi chạy dự án

### Option 1: Docker Compose (Recommended)

#### Full Stack
```bash
# Khởi động tất cả services
docker-compose up --build

# Hoặc chạy background
docker-compose up -d --build

# Xem logs
docker-compose logs -f
```

#### Infrastructure Only
```bash
# Chỉ khởi động database và message queue
docker-compose up -d mysql rabbitmq redis

# Chờ MySQL sẵn sàng (30-60 giây)
docker logs mumii-mysql -f
```

### Option 2: Local Development

#### Bước 1: Khởi động Infrastructure
```bash
docker-compose up -d mysql rabbitmq redis
```

#### Bước 2: Chạy từng service (terminal riêng)
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

---

## Kiểm tra hoạt động

### Health Checks
```bash
# Kiểm tra tất cả services
curl http://localhost:8080/health
curl http://localhost:8081/health
curl http://localhost:8082/health
curl http://localhost:8083/health
curl http://localhost:8084/health
```

### JWT Authentication Test
```bash
# Automated JWT test (Windows)
.\scripts\test-jwt.ps1

# Manual test - Register user
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@mumii.com",
    "password": "test123456",
    "displayName": "Test User"
  }'

# Login and get token
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@mumii.com",
    "password": "test123456"
  }'

# Use token for protected endpoint
curl -X GET http://localhost:8080/api/auth/profile \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

### Service Information
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

### Swagger Documentation
- Auth API: http://localhost:8081/swagger
- Discovery API: http://localhost:8082/swagger
- Social API: http://localhost:8083/swagger
- AI API: http://localhost:8084/swagger

### Test APIs

#### 1. Test Auth Service
```bash
# Đăng ký user mới
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@mumii.com",
    "password": "test123",
    "displayName": "Test User"
  }'
```

#### 2. Test Discovery Service
```bash
# Lấy danh sách nhà hàng
curl http://localhost:8080/api/restaurants
```

#### 3. Test AI Service
```bash
# Chat với AI
curl -X POST http://localhost:8080/api/chat/food \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Hôm nay tôi nên ăn gì?"
  }'
```

---

## Troubleshooting

### Lỗi thường gặp

#### 1. Docker không khởi động được
```bash
# Kiểm tra Docker đang chạy
docker info

# Nếu lỗi, restart Docker Desktop
# Windows: Restart Docker Desktop
# Linux: sudo systemctl restart docker
```

#### 2. Port đã được sử dụng
```bash
# Kiểm tra port đang sử dụng
netstat -an | findstr "8080"  # Windows
lsof -i :8080                # macOS/Linux

# Kill process nếu cần
taskkill /PID <PID> /F       # Windows
kill -9 <PID>                # macOS/Linux
```

#### 3. MySQL không kết nối được
```bash
# Kiểm tra MySQL container
docker logs mumii-mysql

# Reset MySQL volume nếu cần
docker-compose down -v
docker-compose up -d mysql
```

#### 4. Gemini API không hoạt động
```bash
# Kiểm tra API key trong .env
cat .env | grep GEMINI_API_KEY

# Test API key manually
curl -X POST "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{"contents":[{"parts":[{"text":"Hello"}]}]}'
```

#### 5. Services không communicate được
```bash
# Kiểm tra Docker network
docker network ls
docker network inspect mumii_mumii-network

# Restart toàn bộ stack
docker-compose down
docker-compose up --build
```

#### 6. NuGet package conflicts (NU1605, NU1903)
```bash
# Windows PowerShell
.\scripts\fix-nuget-conflicts.ps1

# Linux/Mac
chmod +x scripts/fix-nuget-conflicts.sh
./scripts/fix-nuget-conflicts.sh

# Manual fix
dotnet nuget locals all --clear
# Remove bin/obj folders
dotnet restore
dotnet build
```

### Commands hữu ích

#### Docker Management
```bash
# Xem tất cả containers
docker ps -a

# Xem logs của service cụ thể
docker logs mumii-auth-service -f

# Restart service cụ thể
docker-compose restart auth-service

# Clean up tất cả
docker-compose down -v
docker system prune -f
```

#### Database Management
```bash
# Connect vào MySQL container
docker exec -it mumii-mysql mysql -uroot -pmumii2024

# Backup database
docker exec mumii-mysql mysqldump -uroot -pmumii2024 --all-databases > backup.sql

# Restore database
docker exec -i mumii-mysql mysql -uroot -pmumii2024 < backup.sql
```

#### Development Commands
```bash
# Build specific service
docker-compose build auth-service

# Scale services
docker-compose up -d --scale auth-service=2

# View resource usage
docker stats
```

---

## Development Workflow

### Khi thay đổi code

#### Option 1: Rebuild Docker
```bash
# Rebuild service đã thay đổi
docker-compose build auth-service
docker-compose up -d auth-service
```

#### Option 2: Local Development
```bash
# Run service locally để hot reload
cd src/Services/Auth/Mumii.Auth.Api
dotnet watch run --urls "http://localhost:8081"
```

### Testing Changes
```bash
# Run unit tests
dotnet test

# Integration tests với Docker
docker-compose -f docker-compose.test.yml up --build
```

### Environment Switching
```bash
# Development
cp env.example .env.dev
export $(cat .env.dev | xargs)

# Production
cp env.example .env.prod
export $(cat .env.prod | xargs)
```

---

## Deployment Notes

### Production Environment Variables
```bash
# Production .env should have:
GEMINI_API_KEY=production_api_key
MYSQL_ROOT_PASSWORD=strong_production_password
JWT_SECRET_KEY=very_long_random_production_key
LOG_LEVEL=Warning
```

### Security Considerations
- Đổi tất cả passwords mặc định
- Sử dụng HTTPS cho production
- Enable JWT expiration ngắn hơn
- Setup proper firewall rules
- Regular backup databases

---

**🚀 Chúc bạn setup thành công! Nếu gặp vấn đề gì, hãy tham khảo phần Troubleshooting hoặc tạo issue.**
