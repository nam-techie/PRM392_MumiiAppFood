# 🚀 Flutter Quick Start với Mumii API

## ⚡ TL;DR - Quick Reference

### **Base URLs**

```dart
// Local Development
static const String LOCAL_URL = 'http://localhost:8080';  // iOS Simulator
static const String ANDROID_LOCAL_URL = 'http://10.0.2.2:8080'; // Android Emulator

// Production
static const String PROD_URL = 'https://your-domain.com';
```

### **Quick Setup (5 phút)**

#### **1. Dependencies**

```yaml
dependencies:
  http: ^1.1.0
  shared_preferences: ^2.2.2
```

#### **2. API Client Singleton**

```dart
class ApiClient {
  static final ApiClient _instance = ApiClient._internal();
  factory ApiClient() => _instance;
  ApiClient._internal();
  
  final baseUrl = Platform.isAndroid 
    ? 'http://10.0.2.2:8080' 
    : 'http://localhost:8080';
  
  Future<Map<String, String>> _getHeaders({bool auth = false}) async {
    final headers = {'Content-Type': 'application/json'};
    if (auth) {
      final token = await _getToken();
      headers['Authorization'] = 'Bearer $token';
    }
    return headers;
  }
  
  Future<Response> get(String path, {bool auth = false}) async {
    final headers = await _getHeaders(auth: auth);
    return await http.get(Uri.parse('$baseUrl$path'), headers: headers);
  }
  
  Future<Response> post(String path, Map<String, dynamic> body, {bool auth = false}) async {
    final headers = await _getHeaders(auth: auth);
    return await http.post(
      Uri.parse('$baseUrl$path'),
      headers: headers,
      body: jsonEncode(body),
    );
  }
}
```

#### **3. Login Example**

```dart
final api = ApiClient();

// Login
final loginResponse = await api.post(
  '/api/auth/login',
  {
    'email': 'user@example.com',
    'password': 'password123',
  },
);

if (loginResponse.statusCode == 200) {
  final data = jsonDecode(loginResponse.body);
  final token = data['data']['accessToken'];
  
  // Save token
  final prefs = await SharedPreferences.getInstance();
  await prefs.setString('access_token', token);
}

// Get Profile
final profileResponse = await api.get('/api/auth/profile', auth: true);
```

---

## 📍 Endpoint Mapping

### **Quan trọng: ĐỌC CẨN THẬN!**

| Bạn nghĩ | Thực tế |
|----------|---------|
| ❌ Gọi: `http://localhost:8081/api/auth/login` | ✅ Gọi: `http://localhost:8080/api/auth/login` |
| ❌ Endpoint: `/auth/login` | ✅ Endpoint: `/api/auth/login` |
| ❌ Port: `8081, 8082, 8083` | ✅ Port: `8080` (chỉ Gateway!) |

**👉 TẤT CẢ API đều gọi qua Gateway port 8080!**

---

## 🔗 Complete Endpoint List

### **Auth Service** → `http://localhost:8080/api/auth/*`

```dart
POST   /api/auth/register      // Register
POST   /api/auth/login         // Login
POST   /api/auth/logout        // Logout (requires auth)
GET    /api/auth/profile       // Get profile (requires auth)
PUT    /api/auth/profile       // Update profile (requires auth)
POST   /api/auth/change-password // Change password (requires auth)
```

### **Discovery Service** → `http://localhost:8080/api/restaurants/*`

```dart
GET    /api/restaurants              // Get all
GET    /api/restaurants/{id}         // Get by id
GET    /api/restaurants/search        // Search
GET    /api/restaurants/nearby        // Find nearby
GET    /api/reviews/by-restaurant/{id} // Get reviews
POST   /api/reviews                   // Create review (requires auth)
GET    /api/favorites/by-user/{id}    // Get favorites (requires auth)
POST   /api/favorites                 // Add favorite (requires auth)
```

### **Social Service** → `http://localhost:8080/api/posts/*`

```dart
GET    /api/posts              // Get all posts
POST   /api/posts              // Create post (requires auth)
GET    /api/posts/{id}          // Get post detail
PUT    /api/posts/{id}          // Update post (requires auth)
DELETE /api/posts/{id}          // Delete (requires auth)
GET    /api/moods               // Get all moods
```

### **AI Service** → `http://localhost:8080/api/chat/*`

```dart
POST   /api/chat/food                 // Chat about food
POST   /api/chat/suggest-by-mood      // Suggest by mood
POST   /api/chat/analyze-image        // Analyze image
POST   /api/chat/suggest-restaurants  // Suggest restaurants
```

---

## ⚠️ Android Emulator - CRITICAL!

**Android Emulator KHÔNG thể dùng `localhost`!**

```dart
// ❌ SẼ LỖI trên Android
final url = 'http://localhost:8080/api/auth/login';

// ✅ ĐÚNG cho Android
final url = 'http://10.0.2.2:8080/api/auth/login';

// ✅ Code an toàn
final baseUrl = Platform.isAndroid 
  ? 'http://10.0.2.2:8080'
  : 'http://localhost:8080';
```

---

## 🧪 Test nhanh

### **1. Test Gateway**

```dart
void main() async {
  final response = await http.get(Uri.parse('http://localhost:8080'));
  print(response.statusCode); // Should be 200
}
```

### **2. Test Auth**

```dart
final response = await http.post(
  Uri.parse('http://localhost:8080/api/auth/login'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'email': 'test@example.com',
    'password': 'test123',
  }),
);

print(response.statusCode); // 200 hoặc 401
print(response.body);
```

---

## 📚 Xem thêm

- 📖 [FLUTTER_API_GUIDE.md](FLUTTER_API_GUIDE.md) - Hướng dẫn chi tiết đầy đủ
- 📖 [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - Tất cả API endpoints
- 📖 [README.md](README.md) - Backend architecture

---

**💡 Pro Tips:**

1. ✅ Dùng Gateway URL cho mọi API call
2. ✅ Handle Android emulator đặc biệt
3. ✅ Lưu token sau login
4. ✅ Add token vào mọi protected requests
5. ✅ Check response status code

**🎯 Happy Coding!**

