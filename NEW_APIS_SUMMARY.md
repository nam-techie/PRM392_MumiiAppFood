# 🔐 Mumii Auth API - New Endpoints Summary

## ⭐ **New Authentication APIs**

### **POST /api/auth/google**

```json
{
  "idToken": "google_id_token_from_flutter_app"
}
```

**Status**: Chưa triển khai đầy đủ

### **POST /api/auth/forgot-password**

```json
{
  "email": "user@example.com"
}
```

**Response**: `{"success": true, "message": "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn."}`

### **POST /api/auth/reset-password**

```json
{
  "email": "user@example.com",
  "token": "reset_token_from_email",
  "newPassword": "new_strong_password"
}
```

**Status**: Chưa triển khai đầy đủ

---

## 👤 **New Profile APIs**

### **GET /api/profile/me**

**Headers**: `Authorization: Bearer {{access_token}}`
**Response**: User profile data

### **PUT /api/profile/me**

**Headers**:

- `Authorization: Bearer {{access_token}}`
- `Content-Type: application/json`

```json
{
  "fullname": "New Test User",
  "phoneNumber": "0123456789",
  "address": "456 Lê Lợi, Quận 3",
  "gender": "Nữ"
}
```

**Status**: Chưa triển khai đầy đủ

### **POST /api/profile/avatar**

**Headers**: `Authorization: Bearer {{access_token}}`
**Body**: `multipart/form-data` với field `avatar`
**Status**: Chưa triển khai đầy đủ

---

## 📋 **Implementation Status**

| API             | Status             | Notes                             |
| --------------- | ------------------ | --------------------------------- |
| Google Login    | ❌ Not Implemented | Returns "not implemented"         |
| Forgot Password | 🚧 Partial         | Returns success, no email sending |
| Reset Password  | ❌ Not Implemented | Returns "not implemented"         |
| Get Profile     | ✅ Working         | Basic user info only              |
| Update Profile  | ❌ Not Implemented | Returns "not implemented"         |
| Upload Avatar   | ❌ Not Implemented | Returns "not implemented"         |

---

**🎯 All endpoints are created and testable! Some need business logic implementation but the structure is ready.**
