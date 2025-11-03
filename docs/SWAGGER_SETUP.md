# 🚀 Mumii API Gateway - Swagger UI Setup

## ✅ Đã hoàn thành cấu hình Swagger UI tập trung

### 🎯 Tính năng đã thêm:
- **Swagger UI tập trung** tại `http://localhost:8080/`
- **Proxy routes** cho swagger của từng service
- **Dropdown selection** để chọn service cần test
- **Không cần CORS** vì tất cả đi qua gateway

---

## 🔧 Cấu hình đã thực hiện

### 1. **appsettings.json** - Thêm proxy routes cho Swagger
```json
{
  "ReverseProxy": {
    "Routes": {
      // ... existing API routes ...
      
      // Swagger proxy routes
      "auth-swagger": {
        "ClusterId": "auth-cluster",
        "Match": { "Path": "/swagger/auth/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/swagger/{**catch-all}" }]
      },
      "discovery-swagger": {
        "ClusterId": "discovery-cluster", 
        "Match": { "Path": "/swagger/discovery/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/swagger/{**catch-all}" }]
      },
      "social-swagger": {
        "ClusterId": "social-cluster",
        "Match": { "Path": "/swagger/social/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/swagger/{**catch-all}" }]
      },
      "ai-swagger": {
        "ClusterId": "ai-cluster",
        "Match": { "Path": "/swagger/ai/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/swagger/{**catch-all}" }]
      }
    }
  }
}
```

### 2. **Program.cs** - Thêm Swagger UI middleware
```csharp
// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Swagger UI configuration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/auth/v1/swagger.json", "Auth API v1");
    c.SwaggerEndpoint("/swagger/discovery/v1/swagger.json", "Discovery API v1");
    c.SwaggerEndpoint("/swagger/social/v1/swagger.json", "Social API v1");
    c.SwaggerEndpoint("/swagger/ai/v1/swagger.json", "AI API v1");
    c.RoutePrefix = string.Empty; // Swagger UI tại root "/"
});
```

### 3. **Mumii.ApiGateway.csproj** - Thêm package
```xml
<PackageReference Include="Swashbuckle.AspNetCore" />
```

---

## 🚀 Cách sử dụng

### **Bước 1: Khởi động tất cả services**
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

### **Bước 2: Mở Swagger UI**
Truy cập: **http://localhost:8080/**

### **Bước 3: Chọn service để test**
- Click dropdown **"Select a definition"** ở góc trên bên phải
- Chọn service cần test:
  - **Auth API v1** - Authentication & User Management
  - **Discovery API v1** - Restaurant Search & Location
  - **Social API v1** - Posts, Comments, Reactions  
  - **AI API v1** - Gemini AI Chat & Suggestions

---

## 🎯 Lợi ích

### ✅ **Một cửa vào duy nhất**
- Không cần nhớ nhiều URL khác nhau
- Tất cả APIs đều qua gateway

### ✅ **Không có CORS issues**
- Tất cả requests đi qua cùng một domain
- Không cần cấu hình CORS phức tạp

### ✅ **Đúng kiến trúc microservices**
- Gateway làm entry point duy nhất
- Services ẩn sau gateway

### ✅ **Developer-friendly**
- Dropdown để chọn service dễ dàng
- Tất cả APIs trong một UI

---

## 🔍 Test APIs

### **1. Auth Service**
```bash
# Register user
POST /api/auth/register
{
  "email": "test@mumii.com",
  "password": "test123",
  "displayName": "Test User"
}

# Login
POST /api/auth/login
{
  "email": "test@mumii.com", 
  "password": "test123"
}
```

### **2. Discovery Service**
```bash
# Get restaurants
GET /api/restaurants

# Search restaurants
GET /api/restaurants/search?q=phở
```

### **3. Social Service**
```bash
# Get posts
GET /api/posts

# Create post (cần JWT token)
POST /api/posts
{
  "content": "Hôm nay ăn phở ngon quá! 🍜",
  "mood": "SATISFIED"
}
```

### **4. AI Service**
```bash
# Chat with AI
POST /api/chat/food
{
  "message": "Hôm nay tôi nên ăn gì?"
}
```

---

## 🐛 Troubleshooting

### **Swagger UI không load được?**
```bash
# Kiểm tra services đang chạy
curl http://localhost:8081/health  # Auth
curl http://localhost:8082/health  # Discovery
curl http://localhost:8083/health  # Social
curl http://localhost:8084/health  # AI
curl http://localhost:8080/health  # Gateway
```

### **Swagger JSON không tìm thấy?**
- Đảm bảo services có Swagger enabled
- Kiểm tra route proxy trong appsettings.json
- Xem logs của ApiGateway

### **CORS errors?**
- Không nên có vì tất cả đi qua gateway
- Nếu có, kiểm tra cấu hình CORS trong Program.cs

---

## 🎉 Kết quả

Bây giờ bạn có:
- ✅ **Swagger UI tập trung** tại `http://localhost:8080/`
- ✅ **4 service definitions** trong dropdown
- ✅ **Tất cả APIs** có thể test từ một nơi
- ✅ **Không CORS issues**
- ✅ **Đúng kiến trúc microservices**

**Happy Testing! 🚀**
