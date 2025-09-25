# 🗄️ Mumii Microservices - SQLite Setup Guide

## ✅ **Đã chuyển đổi từ MySQL sang SQLite**

Dự án đã được cấu hình để sử dụng **SQLite** thay vì MySQL, giúp chạy ngay lập tức mà không cần Docker.

---

## 🎯 **Lợi ích của SQLite**

### ✅ **Ưu điểm:**
- **Chạy ngay lập tức** - Không cần cài đặt MySQL server
- **File database** - Mỗi service có 1 file `.db` riêng
- **Portable** - Copy file `.db` là có database
- **Perfect cho development** - Setup nhanh, test dễ dàng

### ❌ **Nhược điểm:**
- **Không scale tốt** - Nhiều user đồng thời sẽ chậm
- **Không có advanced features** - Stored procedures, triggers hạn chế
- **Production** - Nên chuyển về MySQL/PostgreSQL

---

## 📁 **Cấu trúc Database Files**

```
Mumii.Microservices/
├── auth.db          # Auth Service database
├── discovery.db     # Discovery Service database  
├── social.db        # Social Service database
└── src/
    └── Services/
        ├── Auth/
        ├── Discovery/
        └── Social/
```

---

## 🔧 **Các thay đổi đã thực hiện**

### **1. Package References**
**Trước (MySQL):**
```xml
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" />
```

**Sau (SQLite):**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
```

**Files đã cập nhật:**
- `src/Services/Auth/Mumii.Auth.Infrastructure/Mumii.Auth.Infrastructure.csproj`
- `src/Services/Discovery/Mumii.Discovery.Infrastructure/Mumii.Discovery.Infrastructure.csproj`
- `src/Services/Social/Mumii.Social.Infrastructure/Mumii.Social.Infrastructure.csproj`

### **2. DependencyInjection.cs**
**Trước (MySQL):**
```csharp
services.AddDbContext<AuthDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
});
```

**Sau (SQLite):**
```csharp
services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlite(connectionString);
});
```

**Files đã cập nhật:**
- `src/Services/Auth/Mumii.Auth.Infrastructure/DependencyInjection.cs`
- `src/Services/Discovery/Mumii.Discovery.Infrastructure/DependencyInjection.cs`
- `src/Services/Social/Mumii.Social.Infrastructure/DependencyInjection.cs`

### **3. Connection Strings**
**Trước (MySQL):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mumii_auth;Uid=root;Pwd=mumii2024;"
  }
}
```

**Sau (SQLite):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=auth.db"
  }
}
```

**Files đã cập nhật:**
- `src/Services/Auth/Mumii.Auth.Api/appsettings.json`
- `src/Services/Discovery/Mumii.Discovery.Api/appsettings.json`
- `src/Services/Social/Mumii.Social.Api/appsettings.json`

---

## 🚀 **Cách chạy với SQLite**

### **Bước 1: Restore packages**
```bash
dotnet restore
```

### **Bước 2: Chạy từng service**
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

### **Bước 3: Kiểm tra hoạt động**
```bash
# Health checks
curl http://localhost:8080/health
curl http://localhost:8081/health
curl http://localhost:8082/health
curl http://localhost:8083/health
curl http://localhost:8084/health

# Swagger UI
# Mở browser: http://localhost:8080/
```

---

## 🔄 **Cách chuyển đổi giữa SQLite và MySQL**

### **Chuyển từ SQLite sang MySQL:**

#### **1. Cập nhật Package References**
```xml
<!-- Thay thế -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
<!-- Bằng -->
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" />
```

#### **2. Cập nhật DependencyInjection.cs**
```csharp
// Thay thế
options.UseSqlite(connectionString);

// Bằng
options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
    mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
```

#### **3. Cập nhật Connection Strings**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mumii_auth;Uid=root;Pwd=mumii2024;"
  }
}
```

#### **4. Khởi động MySQL**
```bash
docker-compose up -d mysql
```

### **Chuyển từ MySQL sang SQLite:**

#### **1. Cập nhật Package References**
```xml
<!-- Thay thế -->
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" />
<!-- Bằng -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
```

#### **2. Cập nhật DependencyInjection.cs**
```csharp
// Thay thế
options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), ...);

// Bằng
options.UseSqlite(connectionString);
```

#### **3. Cập nhật Connection Strings**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=auth.db"
  }
}
```

---

## 📊 **So sánh SQLite vs MySQL**

| Tính năng | SQLite | MySQL |
|-----------|--------|-------|
| **Setup** | ✅ Không cần cài đặt | ❌ Cần cài đặt server |
| **Performance** | ⚠️ Tốt cho single user | ✅ Tốt cho multi-user |
| **Concurrency** | ❌ Hạn chế | ✅ Tốt |
| **Advanced Features** | ❌ Hạn chế | ✅ Đầy đủ |
| **File Size** | ✅ Nhỏ gọn | ❌ Lớn hơn |
| **Portable** | ✅ Copy file là xong | ❌ Cần export/import |
| **Production** | ❌ Không khuyến khích | ✅ Khuyến khích |

---

## 🛠️ **EF Core Commands với SQLite**

### **Tạo Migration**
```bash
# Auth Service
cd src/Services/Auth/Mumii.Auth.Infrastructure
dotnet ef migrations add InitialCreate -s ../Mumii.Auth.Api

# Discovery Service
cd src/Services/Discovery/Mumii.Discovery.Infrastructure
dotnet ef migrations add InitialCreate -s ../Mumii.Discovery.Api

# Social Service
cd src/Services/Social/Mumii.Social.Infrastructure
dotnet ef migrations add InitialCreate -s ../Mumii.Social.Api
```

### **Apply Migration**
```bash
# Auth Service
cd src/Services/Auth/Mumii.Auth.Api
dotnet ef database update

# Discovery Service
cd src/Services/Discovery/Mumii.Discovery.Api
dotnet ef database update

# Social Service
cd src/Services/Social/Mumii.Social.Api
dotnet ef database update
```

### **Xóa Migration**
```bash
dotnet ef migrations remove
```

---

## 🔍 **Kiểm tra Database Files**

### **Xem nội dung database**
```bash
# Sử dụng SQLite CLI
sqlite3 auth.db
.tables
.schema
.quit

# Hoặc dùng DB Browser for SQLite (GUI)
# Download từ: https://sqlitebrowser.org/
```

### **Backup database**
```bash
# Copy file .db
cp auth.db auth_backup.db
cp discovery.db discovery_backup.db
cp social.db social_backup.db
```

---

## 🐛 **Troubleshooting**

### **Lỗi "Database is locked"**
```bash
# Kiểm tra process đang sử dụng database
lsof auth.db  # macOS/Linux
# Hoặc restart service
```

### **Lỗi "File not found"**
```bash
# Database file sẽ được tạo tự động khi chạy lần đầu
# Đảm bảo có quyền ghi trong thư mục
```

### **Lỗi Migration**
```bash
# Xóa migration cũ và tạo lại
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 📝 **Best Practices**

### **Development**
- ✅ Sử dụng SQLite cho development
- ✅ Backup database files thường xuyên
- ✅ Sử dụng migrations để quản lý schema

### **Production**
- ❌ Không sử dụng SQLite cho production
- ✅ Chuyển sang MySQL/PostgreSQL
- ✅ Sử dụng connection pooling
- ✅ Setup proper backup strategy

---

## 🎉 **Kết quả**

Bây giờ bạn có thể:
- ✅ **Chạy ngay lập tức** không cần Docker
- ✅ **Test APIs** dễ dàng với SQLite
- ✅ **Chuyển đổi dễ dàng** giữa SQLite và MySQL
- ✅ **Development nhanh** hơn nhiều

**Happy Coding! 🚀**
