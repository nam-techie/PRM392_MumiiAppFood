# 📱 Mumii Flutter API Integration Guide

## 📋 Mục lục
- [Giới thiệu về API Gateway](#giới-thiệu-về-api-gateway)
- [Kiến trúc hệ thống](#kiến-trúc-hệ-thống)
- [Endpoint Mapping](#endpoint-mapping)
- [Setup cho Flutter](#setup-cho-flutter)
- [Authentication Flow](#authentication-flow)
- [API Client Implementation](#api-client-implementation)
- [Code Examples](#code-examples)
- [Error Handling](#error-handling)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)

---

## 🌐 Giới thiệu về API Gateway

### **Vì sao cần API Gateway?**

API Gateway đóng vai trò là **cổng vào duy nhất (Single Entry Point)** cho tất cả requests từ Flutter app đến backend services. 

#### **Lợi ích:**
1. ✅ **Một Base URL**: Flutter chỉ cần nhớ 1 URL duy nhất
2. ✅ **Load Balancing**: Phân tải tự động giữa các instances
3. ✅ **Security**: Authentication, rate limiting tập trung
4. ✅ **Monitoring**: Theo dõi tất cả requests
5. ✅ **Service Discovery**: Gateway tự động route đến service đúng

#### **Ảnh hưởng đến Endpoint: KHÔNG!**

⚠️ **Quan trọng**: Khi bạn gọi API qua Gateway, **endpoint KHÔNG thay đổi**, chỉ có base URL thay đổi!

| Tình huống | Base URL | Full Endpoint |
|-----------|----------|---------------|
| **Không dùng Gateway** | `http://localhost:8081` | `http://localhost:8081/api/auth/login` |
| **Dùng Gateway** | `http://localhost:8080` | `http://localhost:8080/api/auth/login` |

👉 **Endpoint vẫn là `/api/auth/login`**, chỉ khác base URL!

---

## 🏗️ Kiến trúc hệ thống

### **Flow Request từ Flutter**

```
┌─────────────────────────────────────────────────────────────┐
│  📱 Flutter App                                             │
│                                                             │
│  API Client:                                                │
│  - Base URL: http://localhost:8080 (Gateway)              │
│  - Endpoint: /api/auth/login                               │
│  - Full URL: http://localhost:8080/api/auth/login          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  🌐 API Gateway (YARP) - Port 8080                          │
│                                                             │
│  Request: POST /api/auth/login                              │
│  ↓ Match route pattern: "/api/auth/{**catch-all}"         │
│  ↓ Route to: auth-cluster                                  │
│  ↓ Forward to: http://localhost:8081/api/auth/login        │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  🔐 Auth Service - Port 8081                               │
│                                                             │
│  - Process login logic                                     │
│  - Generate JWT token                                      │
│  - Return response                                            │
└─────────────────────────────────────────────────────────────┘
```

### **Routing Rules trong Gateway**

Gateway sử dụng pattern matching để route requests:

| Route Pattern | Target Service | Destination | Description |
|--------------|----------------|-------------|-------------|
| `/api/auth/*` | Auth Service | `:8081` | Authentication & User Management |
| `/api/restaurants/*` | Discovery Service | `:8082` | Restaurant Discovery |
| `/api/reviews/*` | Discovery Service | `:8082` | Restaurant Reviews |
| `/api/favorites/*` | Discovery Service | `:8082` | User Favorites |
| `/api/posts/*` | Social Service | `:8083` | Social Posts |
| `/api/moods/*` | Social Service | `:8083` | Mood Management |
| `/api/chat/*` | AI Service | `:8084` | AI Chat & Suggestions |

---

## 🗺️ Endpoint Mapping

### **Local Development**

| Service | Direct URL | Via Gateway | Swagger |
|---------|-----------|-------------|---------|
| **API Gateway** | `http://localhost:8080` | - | `http://localhost:8080` |
| **Auth** | `http://localhost:8081` | `http://localhost:8080/api/auth/*` | `http://localhost:8081/swagger` |
| **Discovery** | `http://localhost:8082` | `http://localhost:8080/api/restaurants/*` | `http://localhost:8082/swagger` |
| **Social** | `http://localhost:8083` | `http://localhost:8080/api/posts/*` | `http://localhost:8083/swagger` |
| **AI** | `http://localhost:8084` | `http://localhost:8080/api/chat/*` | `http://localhost:8084/swagger` |

### **Production/Deployed**

Production URLs sẽ có dạng:
- **API Gateway**: `https://your-domain.com` hoặc `https://mumii-xxx-production.up.railway.app`
- **Services**: Không nên gọi trực tiếp, luôn qua Gateway!

---

## 🚀 Setup cho Flutter

### **Bước 1: Add Dependencies**

Thêm vào `pubspec.yaml`:

```yaml
dependencies:
  # HTTP client
  http: ^1.1.0
  
  # State management (optional, recommend)
  provider: ^6.1.1
  
  # Local storage (cho JWT token)
  shared_preferences: ^2.2.2
  
  # JSON serialization
  json_annotation: ^4.8.1

dev_dependencies:
  json_serializable: ^6.7.1
  build_runner: ^2.4.6
```

### **Bước 2: Tạo Environment Configuration**

Tạo file `lib/config/app_config.dart`:

```dart
class AppConfig {
  // Local development
  static const String localBaseUrl = 'http://localhost:8080';
  
  // Production (thay bằng domain thật)
  static const String productionBaseUrl = 'https://mumii-production.up.railway.app';
  
  // Flag để switch giữa local và production
  static const bool isProduction = false; // Change to true when deploy
  
  // Get current base URL
  static String get baseUrl => isProduction ? productionBaseUrl : localBaseUrl;
  
  // API endpoints constants
  static const String auth = '/api/auth';
  static const String restaurants = '/api/restaurants';
  static const String posts = '/api/posts';
  static const String chat = '/api/chat';
}
```

---

## 🔐 Authentication Flow

### **1. Register User**

```dart
// POST http://localhost:8080/api/auth/register
final response = await http.post(
  Uri.parse('${AppConfig.baseUrl}${AppConfig.auth}/register'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'email': 'user@example.com',
    'password': 'password123',
    'fullname': 'John Doe',
  }),
);

final data = jsonDecode(response.body);
final token = data['data']['accessToken'];
```

### **2. Login**

```dart
// POST http://localhost:8080/api/auth/login
final response = await http.post(
  Uri.parse('${AppConfig.baseUrl}${AppConfig.auth}/login'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'email': 'user@example.com',
    'password': 'password123',
  }),
);

if (response.statusCode == 200) {
  final data = jsonDecode(response.body);
  final token = data['data']['accessToken'];
  
  // Save token to local storage
  await saveToken(token);
}
```

### **3. Save Token**

```dart
import 'package:shared_preferences/shared_preferences.dart';

Future<void> saveToken(String token) async {
  final prefs = await SharedPreferences.getInstance();
  await prefs.setString('access_token', token);
}

Future<String?> getToken() async {
  final prefs = await SharedPreferences.getInstance();
  return prefs.getString('access_token');
}

Future<void> deleteToken() async {
  final prefs = await SharedPreferences.getInstance();
  await prefs.remove('access_token');
}
```

### **4. API Calls với Token**

Tất cả protected endpoints cần header Authorization:

```dart
final token = await getToken();
final response = await http.get(
  Uri.parse('${AppConfig.baseUrl}${AppConfig.auth}/profile'),
  headers: {
    'Authorization': 'Bearer $token',
    'Content-Type': 'application/json',
  },
);
```

---

## 📦 API Client Implementation

### **File: `lib/services/api_client.dart`**

```dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/app_config.dart';

class ApiClient {
  // Singleton pattern
  static final ApiClient _instance = ApiClient._internal();
  factory ApiClient() => _instance;
  ApiClient._internal();
  
  // Get base URL
  String get baseUrl => AppConfig.baseUrl;
  
  // Get headers with auth token
  Future<Map<String, String>> getHeaders({bool includeAuth = false}) async {
    final headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };
    
    if (includeAuth) {
      final token = await getToken();
      if (token != null) {
        headers['Authorization'] = 'Bearer $token';
      }
    }
    
    return headers;
  }
  
  // GET request
  Future<http.Response> get(String endpoint, {bool includeAuth = false}) async {
    final headers = await getHeaders(includeAuth: includeAuth);
    final uri = Uri.parse('$baseUrl$endpoint');
    
    return await http.get(uri, headers: headers);
  }
  
  // POST request
  Future<http.Response> post(
    String endpoint, 
    Map<String, dynamic> body, 
    {bool includeAuth = false}
  ) async {
    final headers = await getHeaders(includeAuth: includeAuth);
    final uri = Uri.parse('$baseUrl$endpoint');
    
    return await http.post(
      uri, 
      headers: headers,
      body: jsonEncode(body),
    );
  }
  
  // PUT request
  Future<http.Response> put(
    String endpoint,
    Map<String, dynamic> body,
    {bool includeAuth = false}
  ) async {
    final headers = await getHeaders(includeAuth: includeAuth);
    final uri = Uri.parse('$baseUrl$endpoint');
    
    return await http.put(
      uri,
      headers: headers,
      body: jsonEncode(body),
    );
  }
  
  // DELETE request
  Future<http.Response> delete(String endpoint, {bool includeAuth = false}) async {
    final headers = await getHeaders(includeAuth: includeAuth);
    final uri = Uri.parse('$baseUrl$endpoint');
    
    return await http.delete(uri, headers: headers);
  }
  
  // Token management
  Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('access_token', token);
  }
  
  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('access_token');
  }
  
  Future<void> deleteToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('access_token');
  }
}
```

---

## 💡 Code Examples

### **1. Auth Service Examples**

#### **Register**

```dart
class AuthService {
  final apiClient = ApiClient();
  
  Future<Map<String, dynamic>> register({
    required String email,
    required String password,
    required String fullname,
  }) async {
    try {
      final response = await apiClient.post(
        '${AppConfig.auth}/register',
        {
          'email': email,
          'password': password,
          'fullname': fullname,
        },
      );
      
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        
        // Save token
        await apiClient.saveToken(data['data']['accessToken']);
        
        return {'success': true, 'data': data};
      } else {
        final error = jsonDecode(response.body);
        return {'success': false, 'error': error['message']};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
  
  Future<Map<String, dynamic>> login({
    required String email,
    required String password,
  }) async {
    try {
      final response = await apiClient.post(
        '${AppConfig.auth}/login',
        {
          'email': email,
          'password': password,
        },
      );
      
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        await apiClient.saveToken(data['data']['accessToken']);
        return {'success': true, 'data': data};
      } else {
        final error = jsonDecode(response.body);
        return {'success': false, 'error': error['message']};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
  
  Future<Map<String, dynamic>> getProfile() async {
    try {
      final response = await apiClient.get(
        '${AppConfig.auth}/profile',
        includeAuth: true,
      );
      
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {'success': true, 'data': data['data']};
      } else {
        return {'success': false, 'error': 'Failed to get profile'};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
  
  Future<void> logout() async {
    await apiClient.deleteToken();
  }
}
```

### **2. Restaurant Service Examples**

```dart
class RestaurantService {
  final apiClient = ApiClient();
  
  Future<Map<String, dynamic>> getRestaurants({
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final response = await apiClient.get(
        '${AppConfig.restaurants}?page=$page&pageSize=$pageSize',
      );
      
      if (response.statusCode == 200) {
        return {
          'success': true,
          'data': jsonDecode(response.body)['data'],
        };
      } else {
        return {'success': false, 'error': 'Failed to fetch restaurants'};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
  
  Future<Map<String, dynamic>> searchRestaurants({
    String? query,
    double? lat,
    double? lng,
    double? radiusKm,
    int minRating = 0,
  }) async {
    try {
      final queryParams = {
        if (query != null) 'q': query,
        if (lat != null) 'lat': lat.toString(),
        if (lng != null) 'lng': lng.toString(),
        if (radiusKm != null) 'radiusKm': radiusKm.toString(),
        'minRating': minRating.toString(),
      };
      
      final uri = Uri.parse('${AppConfig.baseUrl}${AppConfig.restaurants}/search')
          .replace(queryParameters: queryParams);
      
      final response = await http.get(
        uri,
        headers: await apiClient.getHeaders(),
      );
      
      if (response.statusCode == 200) {
        return {
          'success': true,
          'data': jsonDecode(response.body)['data'],
        };
      } else {
        return {'success': false, 'error': 'Failed to search restaurants'};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
}
```

### **3. Social Service Examples**

```dart
class PostService {
  final apiClient = ApiClient();
  
  Future<Map<String, dynamic>> createPost({
    required String content,
    String? mood,
    String? imageUrl,
    String? restaurantId,
  }) async {
    try {
      final response = await apiClient.post(
        AppConfig.posts,
        {
          'content': content,
          if (mood != null) 'mood': mood,
          if (imageUrl != null) 'imageUrl': imageUrl,
          if (restaurantId != null) 'restaurantId': restaurantId,
        },
        includeAuth: true,
      );
      
      if (response.statusCode == 200 || response.statusCode == 201) {
        final data = jsonDecode(response.body);
        return {'success': true, 'data': data['data']};
      } else {
        final error = jsonDecode(response.body);
        return {'success': false, 'error': error['message']};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
  
  Future<Map<String, dynamic>> getPosts({int page = 1}) async {
    try {
      final response = await apiClient.get(
        '${AppConfig.posts}?page=$page&pageSize=20',
        includeAuth: false, // Posts có thể public
      );
      
      if (response.statusCode == 200) {
        return {
          'success': true,
          'data': jsonDecode(response.body)['data'],
        };
      } else {
        return {'success': false, 'error': 'Failed to fetch posts'};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
}
```

### **4. AI Chat Service Examples**

```dart
class AIService {
  final apiClient = ApiClient();
  
  Future<Map<String, dynamic>> chatAboutFood(String message) async {
    try {
      final response = await apiClient.post(
        '${AppConfig.chat}/food',
        {'message': message},
      );
      
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {'success': true, 'data': data['data']};
      } else {
        return {'success': false, 'error': 'Failed to chat'};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
  
  Future<Map<String, dynamic>> suggestByMood({
    required String mood,
    String? location,
  }) async {
    try {
      final response = await apiClient.post(
        '${AppConfig.chat}/suggest-by-mood',
        {
          'mood': mood,
          if (location != null) 'location': location,
        },
      );
      
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {'success': true, 'data': data['data']};
      } else {
        return {'success': false, 'error': 'Failed to get suggestion'};
      }
    } catch (e) {
      return {'success': false, 'error': e.toString()};
    }
  }
}
```

---

## ⚠️ Error Handling

### **Standard Error Response**

```dart
class ApiResponse<T> {
  final bool success;
  final String? message;
  final T? data;
  final List<String> errors;
  
  ApiResponse({
    required this.success,
    this.message,
    this.data,
    this.errors = const [],
  });
  
  factory ApiResponse.fromJson(Map<String, dynamic> json) {
    return ApiResponse(
      success: json['success'] ?? false,
      message: json['message'],
      data: json['data'],
      errors: json['errors']?.cast<String>() ?? [],
    );
  }
}
```

### **Error Handler Service**

```dart
class ErrorHandler {
  static String handleError(http.Response response) {
    switch (response.statusCode) {
      case 400:
        final error = jsonDecode(response.body);
        return error['message'] ?? 'Bad Request';
        
      case 401:
        return 'Unauthorized - Please login again';
        
      case 403:
        return 'Forbidden - You don\'t have permission';
        
      case 404:
        return 'Not Found - Resource doesn\'t exist';
        
      case 500:
        return 'Server Error - Please try again later';
        
      default:
        return 'Unknown error occurred';
    }
  }
  
  static Future<void> handleAuthError(BuildContext context) async {
    // Clear token if unauthorized
    await ApiClient().deleteToken();
    
    // Navigate to login
    Navigator.of(context).pushReplacementNamed('/login');
    
    // Show error message
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Session expired. Please login again')),
    );
  }
}
```

---

## 🧪 Testing

### **1. Test API Connection**

```dart
void main() async {
  // Test Gateway connection
  final response = await http.get(Uri.parse('http://localhost:8080'));
  print('Gateway Status: ${response.statusCode}');
  
  // Test Auth endpoint
  final authResponse = await http.post(
    Uri.parse('http://localhost:8080/api/auth/login'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode({
      'email': 'test@example.com',
      'password': 'password123',
    }),
  );
  
  print('Auth Status: ${authResponse.statusCode}');
  print('Response: ${authResponse.body}');
}
```

### **2. Test với Emulator**

⚠️ **Lưu ý quan trọng cho Android Emulator:**

Android Emulator không thể truy cập `localhost`, phải dùng `10.0.2.2`:

```dart
class AppConfig {
  // Android Emulator
  static const String androidEmulatorBaseUrl = 'http://10.0.2.2:8080';
  
  // iOS Simulator
  static const String iosSimulatorBaseUrl = 'http://localhost:8080';
  
  // Production
  static const String productionBaseUrl = 'https://your-domain.com';
  
  // Auto-detect platform
  static String get baseUrl {
    if (kDebugMode) {
      if (Platform.isAndroid) return androidEmulatorBaseUrl;
      if (Platform.isIOS) return iosSimulatorBaseUrl;
    }
    return productionBaseUrl;
  }
}
```

---

## 🔧 Troubleshooting

### **Problem 1: Connection Refused**

**Symptom**: `Connection refused` hoặc `Failed host lookup`

**Solution**:
- ✅ Check backend services đang chạy: `docker-compose ps`
- ✅ Check Gateway đang chạy: `curl http://localhost:8080`
- ✅ Thử gọi trực tiếp service: `curl http://localhost:8081/health`

### **Problem 2: CORS Error**

**Symptom**: `CORS policy blocked`

**Solution**:
- ✅ Gateway đã config CORS `AllowAll` policy
- ✅ Kiểm tra `appsettings.json` của Gateway
- ✅ Nếu gọi trực tiếp service, check CORS config

### **Problem 3: 401 Unauthorized**

**Symptom**: `Unauthorized` với token hợp lệ

**Solution**:
- ✅ Check token format: phải bắt đầu với `Bearer `
- ✅ Check token chưa expire
- ✅ Login lại để lấy token mới

### **Problem 4: 404 Not Found**

**Symptom**: `404 Not Found` khi gọi endpoint

**Solution**:
- ✅ Check endpoint pattern trong Gateway routes
- ✅ Thử gọi trực tiếp service để test
- ✅ Check Swagger UI để xem endpoints có sẵn

### **Problem 5: Android Emulator không connect được**

**Symptom**: Android emulator không thể connect đến localhost

**Solution**:
- ✅ Thay `localhost` bằng `10.0.2.2` cho Android
- ✅ Hoặc dùng máy ảo Mac/Windows thật để test

---

## 📚 API Reference

### **Complete API Endpoint List**

#### **Auth APIs**

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/api/auth/register` | ❌ | Register new user |
| POST | `/api/auth/login` | ❌ | Login |
| POST | `/api/auth/logout` | ✅ | Logout |
| GET | `/api/auth/profile` | ✅ | Get user profile |
| PUT | `/api/auth/profile` | ✅ | Update profile |
| POST | `/api/auth/change-password` | ✅ | Change password |
| POST | `/api/auth/refresh` | ❌ | Refresh token |

#### **Discovery APIs**

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| GET | `/api/restaurants` | ❌ | Get all restaurants |
| GET | `/api/restaurants/{id}` | ❌ | Get restaurant detail |
| GET | `/api/restaurants/search` | ❌ | Search restaurants |
| GET | `/api/restaurants/nearby` | ❌ | Find nearby restaurants |

#### **Social APIs**

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| GET | `/api/posts` | ❌ | Get all posts |
| POST | `/api/posts` | ✅ | Create post |
| GET | `/api/posts/{id}` | ❌ | Get post detail |
| GET | `/api/moods` | ❌ | Get all moods |

#### **AI Chat APIs**

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/api/chat/food` | ❌ | Chat about food |
| POST | `/api/chat/suggest-by-mood` | ❌ | Suggest by mood |
| POST | `/api/chat/analyze-image` | ❌ | Analyze food image |

---

## 🎯 Tổng kết

### **Câu trả lời cho câu hỏi của bạn:**

> **"FE call api thì phải qua gateway đúng chứ, thế thì ảnh hưởng gì tới endpoint không?"**

✅ **Trả lời**: 

1. **Đúng**, Flutter FE phải gọi qua Gateway (base URL)
2. **KHÔNG ảnh hưởng gì** đến endpoint!
3. Ví dụ:
   - Gọi trực tiếp Auth Service: `http://localhost:8081/api/auth/login`
   - Gọi qua Gateway: `http://localhost:8080/api/auth/login`
   - **Endpoint giữ nguyên**: `/api/auth/login`
   - **Chỉ khác base URL**: `8081` vs `8080`

### **Best Practices:**

1. ✅ **Luôn dùng Gateway**: Dễ maintain, có security & monitoring
2. ✅ **Centralized API Client**: Sử dụng singleton pattern
3. ✅ **Token Management**: Lưu token, xử lý refresh tự động
4. ✅ **Error Handling**: Handle tất cả error cases
5. ✅ **Platform-specific URL**: Handle iOS vs Android emulator

---

**🎉 Happy Coding! Chúc bạn phát triển thành công với Mumii!**

