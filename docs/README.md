# 📚 Mumii Documentation Index

Thư mục chứa tất cả tài liệu dự án Mumii Microservices.

## 📋 Mục lục

### 🚀 Quick Start & Setup
- **[QUICK_START.md](QUICK_START.md)** - Hướng dẫn chạy dự án trong 5 phút
- **[SETUP.md](SETUP.md)** - Cài đặt và cấu hình chi tiết
- **[SQLITE_SETUP.md](SQLITE_SETUP.md)** - Setup với SQLite (không cần Docker)
- **[SWAGGER_SETUP.md](SWAGGER_SETUP.md)** - Cấu hình Swagger UI tập trung

### 📱 Frontend Integration
- **[FLUTTER_API_GUIDE.md](FLUTTER_API_GUIDE.md)** - Hướng dẫn đầy đủ cho Flutter developers
- **[FLUTTER_QUICK_START.md](FLUTTER_QUICK_START.md)** - Quick reference cho Flutter

### 🔧 Backend Documentation
- **[CODEBASE.md](CODEBASE.md)** - Cấu trúc code và design patterns
- **[API_DOCUMENTATION.md](API_DOCUMENTATION.md)** - Tất cả API endpoints đã implement
- **[AUTH_API_COMPLETE_GUIDE.md](AUTH_API_COMPLETE_GUIDE.md)** - Complete Auth API guide

---

## 🗺️ Hướng dẫn sử dụng

### **Bắt đầu từ đâu?**

1. **Lần đầu setup project?** → Đọc [QUICK_START.md](QUICK_START.md)
2. **Cần cấu hình chi tiết?** → Đọc [SETUP.md](SETUP.md)
3. **Là Flutter developer?** → Đọc [FLUTTER_API_GUIDE.md](FLUTTER_API_GUIDE.md)
4. **Cần hiểu codebase?** → Đọc [CODEBASE.md](CODEBASE.md)
5. **Tìm API endpoint?** → Đọc [API_DOCUMENTATION.md](API_DOCUMENTATION.md)

---

## 📁 Cấu trúc Documentation

```
docs/
├── README.md (this file)
│
├── 🚀 Quick Start & Setup
│   ├── QUICK_START.md
│   ├── SETUP.md
│   ├── SQLITE_SETUP.md
│   └── SWAGGER_SETUP.md
│
├── 📱 Frontend Integration
│   ├── FLUTTER_API_GUIDE.md      # Full guide
│   └── FLUTTER_QUICK_START.md    # Quick reference
│
└── 🔧 Backend Documentation
    ├── CODEBASE.md
    ├── API_DOCUMENTATION.md
    └── AUTH_API_COMPLETE_GUIDE.md
```

---

## 🎯 Tài liệu theo Use Case

### **Cho Backend Developers**
1. [CODEBASE.md](CODEBASE.md) - Hiểu architecture và code structure
2. [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - Xem tất cả API endpoints
3. [SWAGGER_SETUP.md](SWAGGER_SETUP.md) - Test APIs với Swagger UI

### **Cho Frontend Developers (Flutter)**
1. [FLUTTER_QUICK_START.md](FLUTTER_QUICK_START.md) - Quick reference
2. [FLUTTER_API_GUIDE.md](FLUTTER_API_GUIDE.md) - Full integration guide
3. [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - API specifications

### **Cho DevOps/Setup**
1. [QUICK_START.md](QUICK_START.md) - Run project nhanh
2. [SETUP.md](SETUP.md) - Setup chi tiết
3. [SQLITE_SETUP.md](SQLITE_SETUP.md) - Development với SQLite

---

## ⚡ Quick Links

### **API References**
- 🔐 **Auth APIs**: [AUTH_API_COMPLETE_GUIDE.md](AUTH_API_COMPLETE_GUIDE.md)
- 🏪 **Discovery APIs**: [API_DOCUMENTATION.md#discovery-service-apis](API_DOCUMENTATION.md)
- 📝 **Social APIs**: [API_DOCUMENTATION.md#social-service-apis](API_DOCUMENTATION.md)
- 🤖 **AI APIs**: [API_DOCUMENTATION.md#ai-service-apis](API_DOCUMENTATION.md)

### **Base URLs**
- **Local**: `http://localhost:8080` (Gateway)
- **Auth Service**: `http://localhost:8081`
- **Discovery Service**: `http://localhost:8082`
- **Social Service**: `http://localhost:8083`
- **AI Service**: `http://localhost:8084`

### **Swagger UI**
- **Gateway**: http://localhost:8080/
- **Auth**: http://localhost:8081/swagger
- **Discovery**: http://localhost:8082/swagger
- **Social**: http://localhost:8083/swagger
- **AI**: http://localhost:8084/swagger

---

## 🔄 Cập nhật

Tài liệu được cập nhật theo dự án. Các file đã bị xóa:
- ❌ `FINAL_IMPLEMENTATION_STATUS.md` - File status cũ, không cần nữa
- ❌ `NEW_APIS_SUMMARY.md` - Duplicate với AUTH_API_COMPLETE_GUIDE.md

---

**💡 Tip**: Luôn đọc README.md ở root trước khi bắt đầu!

