# 📚 Mumii Microservices - API Documentation

Tài liệu chi tiết về tất cả các API endpoints đã được triển khai trong hệ thống Mumii Microservices.

## 📋 Mục lục

- [Tổng quan](#tổng-quan)
- [ Auth Service APIs](#-auth-service-apis)
- [ Discovery Service APIs](#-discovery-service-apis)
- [ Social Service APIs](#-social-service-apis)
- [ AI Service APIs](#-ai-service-apis)
- [ API Gateway](#-api-gateway)
- [ Response Format](#-response-format)
- [ Testing APIs](#-testing-apis)

---

## Tổng quan

### Service URLs
| Service | Base URL | Port | Swagger UI |
|---------|----------|------|------------|
| **API Gateway** | `http://localhost:8080` | 8080 | - |
| **Auth Service** | `http://localhost:8081` | 8081 | http://localhost:8081/swagger |
| **Discovery Service** | `http://localhost:8082` | 8082 | http://localhost:8082/swagger |
| **Social Service** | `http://localhost:8083` | 8083 | http://localhost:8083/swagger |
| **AI Service** | `http://localhost:8084` | 8084 | http://localhost:8084/swagger |

### Authentication
- Tất cả API được bảo vệ bằng JWT Bearer Token (trừ các endpoint public)
- Format: `Authorization: Bearer <access_token>`
- Token được lấy từ endpoint `/api/auth/login`

---

##  Auth Service APIs

**Base URL:** `http://localhost:8081/api/auth`

### 1. Đăng ký tài khoản
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "fullname": "John Doe"
}
```

**Response:**
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
      "fullname": "John Doe",
      "role": "User",
      "isActive": true,
      "loginMethod": "Email",
      "createdAt": "2024-01-01T00:00:00Z",
      "profile": null
    }
  },
  "errors": [],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### 2. Đăng nhập
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:** Tương tự như đăng ký

### 3. Lấy thông tin profile
```http
GET /api/auth/profile
Authorization: Bearer <access_token>
```

**Response:**
```json
{
  "success": true,
  "message": null,
  "data": {
    "id": 1,
    "email": "user@example.com",
    "fullname": "John Doe",
    "role": "User",
    "isActive": true,
    "loginMethod": "Email",
    "createdAt": "2024-01-01T00:00:00Z",
    "profile": null
  }
}
```

### 4. Cập nhật profile
```http
PUT /api/auth/profile
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "fullname": "John Smith"
}
```

### 5. Đổi mật khẩu
```http
POST /api/auth/change-password
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "currentPassword": "old_password",
  "newPassword": "new_password"
}
```

### 6. Đăng xuất
```http
POST /api/auth/logout
Authorization: Bearer <access_token>
```

### 7. Đăng nhập với Google
```http
POST /api/auth/google
Content-Type: application/json

{
  "idToken": "google_id_token_here"
}
```

**Response:** Tương tự như đăng ký/đăng nhập thông thường

### 8. Quên mật khẩu
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu",
  "data": null,
  "errors": [],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### 9. Đặt lại mật khẩu
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "123456",
  "newPassword": "new_password123"
}
```

### 10. Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "refresh_token_here"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Refresh token thành công",
  "data": {
    "accessToken": "new_access_token_here"
  },
  "errors": [],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### 11. Lấy thông tin profile chi tiết
```http
GET /api/auth/profile/me
Authorization: Bearer <access_token>
```

**Response:**
```json
{
  "success": true,
  "message": null,
  "data": {
    "id": 1,
    "email": "user@example.com",
    "fullname": "John Doe",
    "role": "User",
    "isActive": true,
    "loginMethod": "Email",
    "createdAt": "2024-01-01T00:00:00Z",
    "profile": {
      "id": 1,
      "userId": 1,
      "gender": "Male",
      "avatar": "https://cloudinary.com/avatar.jpg",
      "phoneNumber": "0123456789",
      "address": "123 Đường ABC, Quận 1, TP.HCM",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  }
}
```

### 12. Cập nhật profile
```http
PUT /api/auth/profile/me
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "fullname": "John Smith",
  "gender": "Male",
  "phoneNumber": "0987654321",
  "address": "456 Đường XYZ, Quận 2, TP.HCM"
}
```

### 13. Upload avatar
```http
POST /api/auth/profile/avatar
Authorization: Bearer <access_token>
Content-Type: multipart/form-data

avatar: [file]
```

**Response:**
```json
{
  "success": true,
  "message": "Upload avatar thành công",
  "data": {
    "avatarUrl": "https://cloudinary.com/avatars/unique_filename.jpg"
  },
  "errors": [],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

---

## MongoDB Test APIs
**Base URL:** `http://localhost:8081/api/mongo`

#### Ping MongoDB
```http
GET /api/mongo/ping
```

#### Seed Test User
```http
POST /api/mongo/seed-user
```

#### Get Users
```http
GET /api/mongo/users
```

---

##  Discovery Service APIs

**Base URL:** `http://localhost:8082/api/restaurants`

### 1. Lấy danh sách nhà hàng
```http
GET /api/restaurants?page=1&pageSize=20
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "partnerId": 1,
        "name": "Nhà hàng ABC",
        "address": "123 Đường ABC, Quận 1, TP.HCM",
        "longitude": 106.6297,
        "latitude": 10.8231,
        "description": "Nhà hàng chuyên món Việt Nam",
        "avgPrice": 150000,
        "rating": 4.5,
        "status": "Active",
        "createdAt": "2024-01-01T00:00:00Z",
        "images": [],
        "reviews": [],
        "favoriteCount": 0
      }
    ],
    "totalCount": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  }
}
```

### 2. Lấy thông tin nhà hàng theo ID
```http
GET /api/restaurants/{id}
```

### 3. Tìm kiếm nhà hàng
```http
GET /api/restaurants/search?q=phở&lat=10.8231&lng=106.6297&radiusKm=5&minPrice=50000&maxPrice=200000&minRating=4&page=1&pageSize=20
```

**Query Parameters:**
- `q`: Từ khóa tìm kiếm
- `lat`: Vĩ độ
- `lng`: Kinh độ
- `radiusKm`: Bán kính tìm kiếm (km)
- `minPrice`: Giá tối thiểu
- `maxPrice`: Giá tối đa
- `minRating`: Đánh giá tối thiểu
- `page`: Trang hiện tại
- `pageSize`: Số item mỗi trang

### 4. Tìm nhà hàng gần vị trí
```http
GET /api/restaurants/nearby?lat=10.8231&lng=106.6297&radiusKm=5&limit=50
```

### 5. Tạo nhà hàng mới (Admin)
```http
POST /api/restaurants
Content-Type: application/json

{
  "name": "Nhà hàng mới",
  "address": "456 Đường XYZ, Quận 2, TP.HCM",
  "latitude": 10.8231,
  "longitude": 106.6297,
  "description": "Mô tả nhà hàng",
  "avgPrice": 200000
}
```

### 6. Cập nhật nhà hàng (Admin)
```http
PUT /api/restaurants/{id}
Content-Type: application/json

{
  "name": "Tên mới",
  "address": "Địa chỉ mới",
  "latitude": 10.8231,
  "longitude": 106.6297,
  "description": "Mô tả mới",
  "avgPrice": 180000,
  "status": "Active"
}
```

### 7. Xóa nhà hàng (Admin)
```http
DELETE /api/restaurants/{id}
```

---

##  Reviews APIs

**Base URL:** `http://localhost:8082/api/reviews`

### 1. Lấy reviews theo nhà hàng
```http
GET /api/reviews/by-restaurant/{restaurantId}?skip=0&limit=50
```

### 2. Tạo review mới
```http
POST /api/reviews?id=1&userId=1&restaurantId=1
Content-Type: application/json

{
  "rating": 5,
  "comment": "Nhà hàng rất ngon!"
}
```

### 3. Xóa review
```http
DELETE /api/reviews/{id}
```

---

##  Favorites APIs

**Base URL:** `http://localhost:8082/api/favorites`

### 1. Lấy danh sách yêu thích của user
```http
GET /api/favorites/by-user/{userId}?skip=0&limit=50
```

### 2. Thêm vào yêu thích
```http
POST /api/favorites?id=1&userId=1&restaurantId=1
```

### 3. Bỏ yêu thích
```http
DELETE /api/favorites/{id}
```

---

## Social Service APIs

**Base URL:** `http://localhost:8083/api/posts`

### 1. Lấy danh sách bài đăng
```http
GET /api/posts?page=1&pageSize=20&partnerId=1&restaurantId=1
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "partnerId": 1,
        "restaurantId": 1,
        "title": "Bữa tối tuyệt vời",
        "content": "Hôm nay ăn phở rất ngon!",
        "imageUrl": "https://example.com/image.jpg",
        "createdAt": "2024-01-01T00:00:00Z",
        "moods": [],
        "restaurant": null,
        "partner": {
          "id": 1,
          "email": "user@example.com",
          "fullname": "John Doe",
          "role": "User",
          "isActive": true,
          "loginMethod": "Email",
          "createdAt": "2024-01-01T00:00:00Z",
          "profile": null
        }
      }
    ],
    "totalCount": 50,
    "page": 1,
    "pageSize": 20,
    "totalPages": 3
  }
}
```

### 2. Lấy thông tin bài đăng theo ID
```http
GET /api/posts/{id}
```

### 3. Tạo bài đăng mới
```http
POST /api/posts
Content-Type: application/json

{
  "title": "Tiêu đề bài đăng",
  "content": "Nội dung bài đăng",
  "imageUrl": "https://example.com/image.jpg",
  "restaurantId": 1,
  "moodIds": [1, 2]
}
```

### 4. Cập nhật bài đăng
```http
PUT /api/posts/{id}
Content-Type: application/json

{
  "title": "Tiêu đề mới",
  "content": "Nội dung mới",
  "imageUrl": "https://example.com/new-image.jpg",
  "restaurantId": 2,
  "moodIds": [3, 4]
}
```

### 5. Xóa bài đăng
```http
DELETE /api/posts/{id}
```

### 6. Gán mood cho bài đăng
```http
POST /api/posts/{id}/moods/{moodId}
```

### 7. Bỏ mood khỏi bài đăng
```http
DELETE /api/posts/{id}/moods/{moodId}
```

---

##  Moods APIs

**Base URL:** `http://localhost:8083/api/moods`

### 1. Lấy danh sách tất cả moods
```http
GET /api/moods
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "HAPPY",
      "description": "Vui vẻ",
      "createdAt": "2024-01-01T00:00:00Z"
    },
    {
      "id": 2,
      "name": "EXCITED",
      "description": "Hào hứng",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 2. Tạo mood mới
```http
POST /api/moods
Content-Type: application/json

{
  "name": "RELAXED",
  "description": "Thư giãn"
}
```

### 3. Xóa mood
```http
DELETE /api/moods/{id}
```

---

##  AI Service APIs

**Base URL:** `http://localhost:8084/api/chat`

### 1. Chat về đồ ăn
```http
POST /api/chat/food
Content-Type: application/json

{
  "message": "Hôm nay tôi nên ăn gì?"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Chat thành công",
  "data": "Dựa trên thời tiết hôm nay, tôi gợi ý bạn nên thử món phở nóng hổi hoặc bún bò Huế. Đây là những món ăn phù hợp với không khí se lạnh...",
  "errors": [],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### 2. Gợi ý món ăn theo mood
```http
POST /api/chat/suggest-by-mood
Content-Type: application/json

{
  "mood": "HAPPY",
  "location": "Hà Nội"
}
```

### 3. Phân tích hình ảnh đồ ăn
```http
POST /api/chat/analyze-image
Content-Type: application/json

{
  "imageUrl": "https://example.com/food-image.jpg"
}
```

### 4. Gợi ý nhà hàng
```http
POST /api/chat/suggest-restaurants
Content-Type: application/json

{
  "preferences": "Tôi thích món ăn Việt Nam, giá cả hợp lý",
  "location": "Quận 1, TP.HCM"
}
```

---

## 🌐 API Gateway

**Base URL:** `http://localhost:8080`

### Routing Rules
API Gateway sử dụng YARP để route các request đến các services tương ứng:

| Route Pattern | Target Service | Description |
|---------------|----------------|-------------|
| `/api/auth/*` | Auth Service (8081) | Authentication & User Management |
| `/api/restaurants/*` | Discovery Service (8082) | Restaurant Discovery |
| `/api/reviews/*` | Discovery Service (8082) | Restaurant Reviews |
| `/api/favorites/*` | Discovery Service (8082) | User Favorites |
| `/api/posts/*` | Social Service (8083) | Social Posts |
| `/api/moods/*` | Social Service (8083) | Mood Management |
| `/api/chat/*` | AI Service (8084) | AI Chat & Suggestions |

### Health Check
```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "auth-service": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "discovery-service": {
      "status": "Healthy", 
      "duration": "00:00:00.0234567"
    }
  }
}
```

---

## 📊 Response Format

Tất cả API responses đều tuân theo format chuẩn:

```json
{
  "success": boolean,
  "message": "string | null",
  "data": "object | array | null",
  "errors": ["string"],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Success Response
```json
{
  "success": true,
  "message": "Thành công",
  "data": { /* response data */ },
  "errors": [],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Lỗi xảy ra",
  "data": null,
  "errors": ["Chi tiết lỗi"],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Pagination Format
```json
{
  "items": [/* array of items */],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

---

## 🔧 Testing APIs

### 1. Sử dụng cURL

#### Đăng ký user mới
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@mumii.com",
    "password": "test123",
    "fullname": "Test User"
  }'
```

#### Đăng nhập và lấy token
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@mumii.com",
    "password": "test123"
  }'
```

#### Lấy danh sách nhà hàng
```bash
curl http://localhost:8080/api/restaurants
```

#### Tạo bài đăng (cần token)
```bash
curl -X POST http://localhost:8080/api/posts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "title": "Bữa tối ngon",
    "content": "Hôm nay ăn phở rất ngon!",
    "imageUrl": "https://example.com/pho.jpg",
    "restaurantId": 1,
    "moodIds": [1, 2]
  }'
```

#### Chat với AI
```bash
curl -X POST http://localhost:8080/api/chat/food \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Hôm nay tôi nên ăn gì?"
  }'
```

### 2. Sử dụng Postman

1. **Import Collection:** Có thể tạo Postman collection từ Swagger UI
2. **Environment Variables:**
   - `base_url`: `http://localhost:8080`
   - `auth_token`: JWT token từ login response
3. **Pre-request Script:** Tự động thêm Authorization header

### 3. Sử dụng Swagger UI

Truy cập các Swagger UI để test trực tiếp:
- Auth: http://localhost:8081/swagger
- Discovery: http://localhost:8082/swagger  
- Social: http://localhost:8083/swagger
- AI: http://localhost:8084/swagger

---

## 📝 Notes

### Authentication Flow
1. Đăng ký/Đăng nhập để lấy `access_token`
2. Sử dụng `access_token` trong header `Authorization: Bearer <token>`
3. Token có thời hạn, cần refresh khi hết hạn

### Error Codes
- `400`: Bad Request - Dữ liệu không hợp lệ
- `401`: Unauthorized - Chưa đăng nhập hoặc token không hợp lệ
- `403`: Forbidden - Không có quyền truy cập
- `404`: Not Found - Không tìm thấy resource
- `500`: Internal Server Error - Lỗi hệ thống

### Rate Limiting
Hiện tại chưa implement rate limiting, sẽ được thêm trong tương lai.

### CORS
CORS được cấu hình để cho phép requests từ frontend applications.

---

**📖 Tài liệu này sẽ được cập nhật thường xuyên khi có thêm API endpoints mới.**
