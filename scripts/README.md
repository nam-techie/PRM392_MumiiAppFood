# 📁 Scripts Directory

Thư mục chứa các scripts tiện ích cho dự án Mumii Microservices.

## 🚀 **Scripts chính:**

### **Windows (PowerShell):**
- **`run-all-services.ps1`** - Chạy tất cả services + mở Swagger UI tập trung ⭐
- **`run-sqlite.ps1`** - Chạy tất cả services với SQLite database
- **`stop-sqlite.ps1`** - Dừng tất cả services
- **`test-ai-service.ps1`** - Test AI Service với Gemini API

### **Linux/Mac (Bash):**
- **`run-sqlite.sh`** - Chạy tất cả services với SQLite database

### **Database:**
- **`01-init-databases.sql`** - Script khởi tạo database cho MySQL (nếu cần)

---

## 🎯 **Cách sử dụng:**

### **Chạy dự án:**
```powershell
# Windows (Recommended - có Swagger UI tập trung)
.\scripts\run-all-services.ps1

# Windows (Alternative)
.\scripts\run-sqlite.ps1

# Linux/Mac
./scripts/run-sqlite.sh
```

### **Dừng dự án:**
```powershell
# Windows
.\scripts\stop-sqlite.ps1
```

### **Test AI Service:**
```powershell
# Windows
.\scripts\test-ai-service.ps1
```

---

## 📋 **Yêu cầu:**

- **.NET 8 SDK**
- **PowerShell** (cho Windows scripts)
- **Bash** (cho Linux/Mac scripts)
- **GEMINI_API_KEY** (cho AI Service)

---

## 🔧 **Troubleshooting:**

Nếu gặp lỗi, hãy chạy:
```bash
# Clean và restore
dotnet clean
dotnet restore
dotnet build

# Rồi chạy lại script
.\scripts\run-sqlite.ps1
```

---

**💡 Tip:** Tất cả scripts đều có output màu sắc và hướng dẫn chi tiết!
