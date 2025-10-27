# 🚀 Mumii Quick Start Guide

Hướng dẫn nhanh để khởi chạy dự án Mumii trong 5 phút!

## 📋 **Prerequisites**

### **Bắt buộc:**
- ✅ **Docker Desktop** - [Tải về](https://www.docker.com/products/docker-desktop/)
- ✅ **Git** - [Tải về](https://git-scm.com/downloads)

### **Tùy chọn (cho development):**
- 🔧 **.NET 8 SDK** - [Tải về](https://dotnet.microsoft.com/download/dotnet/8.0)
- 🔧 **Visual Studio 2022** hoặc **VS Code**

---

## ⚡ **Quick Start (Docker)**

### **Bước 1: Clone & Setup**
```bash
# Clone repository
git clone https://github.com/your-username/mumii-microservices.git
cd mumii-microservices

# Kiểm tra Docker đang chạy
docker --version
docker-compose --version
```

### **Bước 2: Khởi động hạ tầng**
```bash
# Khởi động database và message queue trước
docker-compose up -d mysql rabbitmq redis

# Chờ MySQL khởi động (30-60 giây)
docker logs mumii-mysql -f
# Thấy "ready for connections" là được!
```

### **Bước 3: Khởi động tất cả services**
```bash
# Build và khởi động tất cả
docker-compose up --build

# Hoặc chạy background
docker-compose up -d --build
```

### **Bước 4: Kiểm tra hoạt động**
```bash
# Health checks
curl http://localhost:8080/health        # API Gateway
curl http://localhost:8081/health        # Auth Service

# Swagger UI
# Mở browser: http://localhost:8081/swagger
```

🎉 **Xong! Tất cả services đã sẵn sàng!**

---

## 🧪 **Test API nhanh**

### **1. Đăng ký tài khoản**
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@mumii.com",
    "password": "test123",
    "displayName": "Test User"
  }'
```

### **2. Đăng nhập & lấy token**
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@mumii.com",
    "password": "test123"
  }'

# Lưu access_token từ response để dùng cho bước tiếp theo
```

### **3. Lấy profile (cần token)**
```bash
curl -X GET http://localhost:8080/api/auth/profile \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

### **4. Xem danh sách nhà hàng mẫu**
```bash
curl http://localhost:8080/api/restaurants
```

---

## 📊 **Service URLs**

| Service | URL | Swagger |
|---------|-----|---------|
| 🌐 **API Gateway** | http://localhost:8080 | - |
| 🔐 **Auth Service** | http://localhost:8081 | http://localhost:8081/swagger |
| 🏪 **Discovery Service** | http://localhost:8082 | http://localhost:8082/swagger |
| 📝 **Social Service** | http://localhost:8083 | http://localhost:8083/swagger |

### **Management UIs**
- 🐰 **RabbitMQ**: http://localhost:15672 (admin/mumii2024)

---

## 🐛 **Troubleshooting**

### **MySQL không khởi động được?**
```bash
# Kiểm tra logs
docker logs mumii-mysql

# Reset volume nếu cần
docker-compose down -v
docker-compose up -d mysql
```

### **Port bị conflict?**
```bash
# Kiểm tra port đang sử dụng
netstat -an | find "8080"   # Windows
lsof -i :8080              # macOS/Linux

# Dừng services
docker-compose down
```

### **Build lỗi?**
```bash
# Clean build
docker-compose down
docker system prune -f
docker-compose up --build --force-recreate
```

### **Services không kết nối được database?**
```bash
# Kiểm tra network
docker network ls
docker network inspect mumii_mumii-network

# Restart theo thứ tự
docker-compose restart mysql
sleep 30
docker-compose restart auth-service
```

---

## 🔧 **Development Mode**

Nếu muốn run local để debug:

### **Windows (PowerShell)**
```powershell
# Khởi động infrastructure
docker-compose up -d mysql rabbitmq redis

# Chạy từng service (mở terminal riêng cho mỗi service)
cd src\Services\Auth\Mumii.Auth.Api
dotnet run

cd src\Services\Discovery\Mumii.Discovery.Api  
dotnet run

cd src\Services\Social\Mumii.Social.Api
dotnet run

cd src\ApiGateway
dotnet run
```

### **macOS/Linux**
```bash
# Sử dụng script có sẵn
./scripts/run-local.sh

# Dừng tất cả
./scripts/stop-local.sh
```

---

## 📚 **Next Steps**

1. 📖 **Đọc [README.md](README.md)** để hiểu chi tiết architecture
2. 🔍 **Explore Swagger UIs** để xem tất cả APIs
3. 📱 **Thử các API** với Postman hoặc curl
4. 🔧 **Customize configs** trong `appsettings.json`
5. 🚀 **Start developing** features mới!

---

## 💡 **Pro Tips**

- 🔄 **Hot reload**: Sử dụng `dotnet watch run` cho development
- 📝 **Logs**: `docker-compose logs -f service-name`
- 🗄️ **Database**: Kết nối MySQL với client yêu thích (port 3306)
- 🐰 **Message Queue**: Monitor RabbitMQ qua web UI
- 🔍 **Debug**: Attach debugger vào container hoặc run local

---

**🎯 Happy Coding! Chúc bạn phát triển thành công với Mumii!**
