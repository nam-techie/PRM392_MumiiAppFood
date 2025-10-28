# 🤖 AGENTS.md  
## 📜 Quy tắc và Hướng dẫn cho AI Agent - Dự án MumiiAppFood  

Chào mừng, **AI Agent!**  
Bạn là một **trợ lý lập trình chuyên về .NET và Microservices**.  
Nhiệm vụ của bạn là hỗ trợ phát triển dự án **MumiiAppFood** bằng cách tuân thủ nghiêm ngặt các **quy tắc, kiến trúc và convention** dưới đây.  
Mục tiêu: **Tạo ra code sạch, nhất quán, dễ bảo trì, dễ mở rộng và tuân thủ Clean Architecture.**

---

## 🏛️ 1. Tổng quan Kiến trúc (High-Level Architecture)

**Mô hình:** Microservices + Clean Architecture  
**Cơ sở dữ liệu:** MongoDB — mỗi service có database riêng:  
- `mumii_auth`  
- `mumii_discovery`  
- `mumii_social`

**Project chia sẻ dùng chung:** `Mumii.Shared.Common`  
Chứa DTOs, Models, Constants, Enums, và các logic chia sẻ giữa các service.

### ⚙️ Các Microservices chính:

| Service | Namespace | Database | Chức năng chính |
|----------|------------|-----------|------------------|
| **Authentication** | `Mumii.Auth` | `mumii_auth` | Quản lý User, Partner, Authentication, Authorization, Profile, Notifications |
| **Discovery** | `Mumii.Discovery` | `mumii_discovery` | Quản lý Restaurant, Review, Favorite; chịu trách nhiệm tìm kiếm và khám phá |
| **Social** | `Mumii.Social` | `mumii_social` | Quản lý Post, Comment, Mood, Like; cung cấp tính năng mạng xã hội |
| **AI** | `Mumii.AI` | *(Không có DB)* | Tích hợp Google Gemini và AI logic (Mood, Gợi ý, Chat) |
| **API Gateway** | `Mumii.ApiGateway` | *(Không có DB)* | Cổng vào duy nhất của client; dùng YARP để điều hướng request |

---

## 🧱 2. Quy tắc về Cấu trúc Project (Clean Architecture)

Mỗi service (`Auth`, `Discovery`, `Social`) **phải tuân theo 3 Layer**:

### 🧩 Domain Layer
- **Chứa:** Entities, Interfaces (Repositories, Services).  
- **Không phụ thuộc** bất kỳ framework hay kỹ thuật nào (MongoDB, ASP.NET…).  
- **Chỉ tham chiếu** `Mumii.Shared.Common`.  
- **Nguyên tắc:** Domain là "trái tim" của hệ thống — không được phép phụ thuộc vào bên ngoài.

### 🧩 Infrastructure Layer
- **Chứa:** Triển khai repository (MongoDB), service kỹ thuật (Jwt, Photo, Gemini...).  
- **Được phép tham chiếu:** `Domain` và `Shared.Common`.  
- **Chịu trách nhiệm:** Tương tác DB, file system, API ngoài, v.v.

### 🧩 Api Layer
- **Chứa:** Controllers, `Program.cs`, Swagger, cấu hình Dependency Injection, Authentication, CORS...  
- **Tham chiếu:** `Domain`, `Infrastructure`, `Shared.Common`.  
- **Chịu trách nhiệm:** Tiếp nhận HTTP request, ủy quyền xử lý cho service/domain.

---

## ✍️ 3. Quy tắc về Code (Coding Conventions)

### 🧠 Ngôn ngữ
- **C# 12**  
- **.NET 8 (LTS)**  
- Luôn dùng `async/await` cho I/O.  
- Mọi hàm async **phải truyền** `CancellationToken`.

### 🧱 Entities
- Thuộc tính `Id` luôn là `int`.  
- Dùng `private set` để bảo vệ trạng thái.  
- Logic nghiệp vụ nằm **trực tiếp trong entity** qua các method (VD: `restaurant.Approve()`).  
- Khởi tạo entity bằng `static Create(...)` để áp dụng validation.  

### 📦 DTOs (Data Transfer Objects)
- Dùng **`record`** để đảm bảo bất biến.  
- Đặt trong `Mumii.Shared.Common/DTOs/...`.  
- Tên rõ ràng, ví dụ:  
  - `CreatePostRequest`  
  - `UpdateRestaurantRequest`  
  - `UserDto`

### 🌐 Controllers
- Phân quyền bằng `[Authorize(Roles = "...")]`.  
- Trả về `ApiResponse<T>` để thống nhất định dạng JSON.  
- Lấy user ID qua helper riêng, **không lặp lại** `User.FindFirstValue(...)`.  
- Bắt lỗi bằng `try-catch`, trả HTTP status phù hợp (400, 401, 403, 404, 500).  

### 🧩 Repositories
- Làm việc với **Entity**, không làm việc trực tiếp với DTO.  
- Cung cấp phương thức tối ưu truy vấn như `GetByIdsAsync(IEnumerable<int> ids)` để tránh lỗi N+1 query.  

---

## 🌐 4. Quy tắc về Giao tiếp giữa các Service

### 🔒 Nguyên tắc
- Các service **độc lập** — không được tham chiếu trực tiếp `.csproj` của nhau.  
- **Không** có dependency chéo giữa các service cùng cấp.

### 🔗 Giao tiếp (đơn giản hóa cho dự án này)
- Khi một service cần dữ liệu của service khác, **inject repository** tương ứng.  
- VD: `Social.Api` cần thông tin `User`, `AdminPostsController` sẽ inject `IUserRepository` (từ `Auth.Domain`) và gọi `_userRepository.GetByIdsAsync(...)`.

➡️ Để làm được điều này:  
`Social.Api` phải có **ProjectReference** đến `Auth.Domain` và `Auth.Infrastructure`.

---

## 🚀 5. Ví dụ khi ra lệnh cho AI Agent

### ✅ Tốt:
> "Trong `AdminRestaurantsController`, hãy thêm một endpoint `GET` mới với route `/pending` để lấy danh sách các nhà hàng có trạng thái `'Pending'`.  
> Endpoint này cần có `[Authorize(Roles = "Admin")]` và sử dụng `_restaurantRepository.GetPagedByStatusAsync()`."

### ❌ Xấu:
> "Làm chức năng xem nhà hàng chờ duyệt."  
> → Quá mơ hồ, không rõ vị trí, vai trò và cách triển khai.

---

## 📖 Ghi chú thêm
- AI Agent phải **luôn đọc file này trước khi sinh code mới**.  
- Mọi logic đều phải phù hợp với Clean Architecture và mô hình Microservices.  
- Khi có thay đổi lớn trong kiến trúc, cập nhật lại file `Agents.md` để đảm bảo đồng bộ.

---

**Tác giả:** MumiiAppFood Core Team  
**Phiên bản:** 1.0.0  
**Cập nhật lần cuối:** 2025-10-28
