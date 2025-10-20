# 🔐 Mumii Auth API - Complete Guide (Updated)

## 📋 **Tổng quan**

Tài liệu này hướng dẫn cách sử dụng tất cả Auth API của Mumii App Food trong Postman.

**Base URL**: `http://localhost:8081` (Auth Service trực tiếp)  
**API Gateway**: `http://localhost:8080` (Qua Gateway)

---

## 🚀 **Setup Postman**

### **1. Tạo Environment**

Tạo environment mới với các variables:

| Variable        | Value                   | Description               |
| --------------- | ----------------------- | ------------------------- |
| `base_url`      | `http://localhost:8081` | Base URL của Auth Service |
| `gateway_url`   | `http://localhost:8080` | Base URL của API Gateway  |
| `access_token`  | `{{access_token}}`      | JWT Access Token          |
| `refresh_token` | `{{refresh_token}}`     | Refresh Token             |

---

## 📝 **Module 1: Authentication APIs**

### **1. Register User**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/auth/register`
- **Headers**: `Content-Type: application/json`
- **Body**:

```json
{
  "email": "test@example.com",
  "password": "test123456",
  "fullname": "Test User"
}
```

### **2. Login**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/auth/login`
- **Headers**: `Content-Type: application/json`
- **Body**:

```json
{
  "email": "test@example.com",
  "password": "test123456"
}
```

### **3. Google Login** ⭐ **NEW**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/auth/google`
- **Headers**: `Content-Type: application/json`
- **Body**:

```json
{
  "idToken": "google_id_token_from_flutter_app"
}
```

- **Response**: Tương tự Login thường
- **Status**: Chưa triển khai đầy đủ

### **4. Forgot Password** ⭐ **NEW**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/auth/forgot-password`
- **Headers**: `Content-Type: application/json`
- **Body**:

```json
{
  "email": "user@example.com"
}
```

- **Response**:

```json
{
  "success": true,
  "message": "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn."
}
```

### **5. Reset Password** ⭐ **NEW**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/auth/reset-password`
- **Headers**: `Content-Type: application/json`
- **Body**:

```json
{
  "email": "user@example.com",
  "token": "reset_token_from_email",
  "newPassword": "new_strong_password"
}
```

- **Status**: Chưa triển khai đầy đủ

### **6. Refresh Token**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/auth/refresh`
- **Headers**: `Content-Type: application/json`
- **Body**:

```json
{
  "refreshToken": "{{refresh_token}}"
}
```

- **Status**: Chưa triển khai đầy đủ

### **7. Change Password**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/auth/change-password`
- **Headers**:
  - `Authorization: Bearer {{access_token}}`
  - `Content-Type: application/json`
- **Body**:

```json
{
  "currentPassword": "test123456",
  "newPassword": "newpassword123"
}
```

### **8. Logout**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/auth/logout`
- **Headers**: `Authorization: Bearer {{access_token}}`

---

## 👤 **Module 2: Profile Management APIs**

### **1. Get Profile** ⭐ **NEW**

- **Method**: `GET`
- **URL**: `{{base_url}}/api/profile/me`
- **Headers**: `Authorization: Bearer {{access_token}}`
- **Response**:

```json
{
  "success": true,
  "data": {
    "id": 1,
    "email": "test@example.com",
    "fullname": "Test User",
    "role": "User",
    "isActive": true,
    "loginMethod": "Email",
    "createdAt": "2024-01-01T00:00:00Z",
    "profile": null
  }
}
```

### **2. Update Profile** ⭐ **NEW**

- **Method**: `PUT`
- **URL**: `{{base_url}}/api/profile/me`
- **Headers**:
  - `Authorization: Bearer {{access_token}}`
  - `Content-Type: application/json`
- **Body**:

```json
{
  "fullname": "New Test User",
  "phoneNumber": "0123456789",
  "address": "456 Lê Lợi, Quận 3",
  "gender": "Nữ"
}
```

- **Status**: Chưa triển khai đầy đủ

### **3. Upload Avatar** ⭐ **NEW**

- **Method**: `POST`
- **URL**: `{{base_url}}/api/profile/avatar`
- **Headers**: `Authorization: Bearer {{access_token}}`
- **Body**: `multipart/form-data` với field `avatar` chứa file ảnh
- **Response**:

```json
{
  "success": true,
  "data": {
    "avatarUrl": "new_url_to_avatar.jpg"
  }
}
```

- **Status**: Chưa triển khai đầy đủ

---

## 🔧 **Postman Collection Setup**

### **1. Tạo Collection Structure**

```
📁 Mumii Auth API
├── 📁 1. Authentication
│   ├── POST Register User
│   ├── POST Login
│   ├── POST Google Login ⭐
│   ├── POST Forgot Password ⭐
│   ├── POST Reset Password ⭐
│   ├── POST Refresh Token
│   ├── POST Change Password
│   └── POST Logout
├── 📁 2. Profile Management ⭐
│   ├── GET Profile Me
│   ├── PUT Update Profile
│   └── POST Upload Avatar
└── 📁 3. Error Examples
    ├── POST Register - Email Exists
    ├── POST Login - Wrong Password
    └── GET Profile - No Token
```

### **2. Tests Scripts (Auto-save Token)**

Thêm vào các request Login và Register:

```javascript
if (pm.response.code === 200) {
  const response = pm.response.json();
  pm.environment.set("access_token", response.data.accessToken);
  pm.environment.set("refresh_token", response.data.refreshToken);
  pm.environment.set("user_id", response.data.user.id);
  pm.environment.set("user_email", response.data.user.email);
  pm.environment.set("user_name", response.data.user.fullname);
}
```

---

## 🧪 **Testing Workflow**

### **Complete User Journey**

1. **Register** → Lưu token vào environment
2. **Login** → Verify token được lưu
3. **Get Profile** → Verify user info
4. **Update Profile** → Test profile update
5. **Upload Avatar** → Test file upload
6. **Change Password** → Test password change
7. **Logout** → Verify logout

### **Error Testing**

1. **Register with existing email** → Expect 400 error
2. **Login with wrong password** → Expect 400 error
3. **Get Profile without token** → Expect 401 error
4. **Forgot Password with non-existent email** → Still returns success (security)

---

## 📊 **Response Status Codes**

| Status Code | Description           | Example                              |
| ----------- | --------------------- | ------------------------------------ |
| `200`       | Success               | Login, Profile operations            |
| `400`       | Bad Request           | Validation errors, wrong credentials |
| `401`       | Unauthorized          | Missing or invalid token             |
| `404`       | Not Found             | User not found                       |
| `500`       | Internal Server Error | Server errors                        |

---

## ⚠️ **Implementation Status**

### **✅ Fully Implemented**

- Register User
- Login
- Change Password
- Logout
- Get Profile (basic)

### **🚧 Partially Implemented**

- Forgot Password (returns success message, no email sending)
- Get Profile (no profile details yet)

### **❌ Not Implemented**

- Google Login (returns "not implemented")
- Reset Password (returns "not implemented")
- Refresh Token (returns "not implemented")
- Update Profile (returns "not implemented")
- Upload Avatar (returns "not implemented")

---

## 🔗 **Next Steps for Full Implementation**

### **1. Google Login**

- Integrate Google OAuth verification
- Handle Google user creation/login

### **2. Password Reset**

- Implement token generation
- Add email service integration
- Create reset token validation

### **3. Profile Management**

- Create Profile entity and repository
- Implement profile update logic
- Add file upload service (S3/Cloudinary)

### **4. Refresh Token**

- Implement refresh token storage
- Add token validation logic

---

**🎯 Tất cả API endpoints đã được tạo và có thể test được! Một số chức năng cần implement thêm logic business nhưng structure đã sẵn sàng.**
