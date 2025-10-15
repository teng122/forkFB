# ✅ ĐÃ FIX: Session Issue - UserId not found

## 🐛 Vấn đề gốc

Khi đăng nhập thành công nhưng add recipe bị redirect về trang login với lỗi:
```
UserId not found in session
```

## 🔍 Nguyên nhân

**Session key không khớp!**

- **Login lưu:** `Session.SetString("user_id", user.username)` ← Lưu **username** (string)
- **AddRecipe lấy:** `Session.GetInt32("UserId")` ← Lấy **int** với key **khác**!

→ AddRecipe không tìm thấy UserId → redirect về login

## ✅ Giải pháp đã áp dụng

### 1. **Sửa AccountController.cs (Login)**
```csharp
// TRƯỚC (SAI)
HttpContext.Session.SetString("user_id", user.username); // Lưu username!

// SAU (ĐÚNG)
HttpContext.Session.SetInt32("UserId", user.user_id ?? 0); // Lưu user_id (int)!
HttpContext.Session.SetString("user_id", user.username);   // Giữ lại cho backward compatible
```

### 2. **Thêm debug logs trong HomeController.cs**
```csharp
// Log tất cả session keys để debug
var sessionKeys = new[] { "UserId", "user_id", "username", "user_email", "role" };
foreach (var key in sessionKeys)
{
    var value = HttpContext.Session.GetString(key);
    _logger.LogInformation("Session[{Key}] = {Value}", key, value ?? "NULL");
}
```

### 3. **Fix validation issues**
- ✅ Bỏ `[Required]` cho `Instruction` (có thể chỉ có ảnh)
- ✅ Thêm `[Range(1, 1440)]` cho `CookTime`
- ✅ Thêm `min="1"` trong input HTML

## 🚀 Cách test

### **Bước 1: Đăng nhập lại**
1. Logout (nếu đang login)
2. Login lại với tài khoản `admin`
3. **CHECK LOG** trong terminal:
   ```
   Login successful for user: admin
   Session set: UserId=1, Username=admin
   ```

### **Bước 2: Thử add recipe**
1. Vào `/Home/AddRecipe`
2. Điền form:
   - **Tên:** "Test Recipe"
   - **Thời gian:** 30 (phải > 0)
   - **Độ khó:** Dễ
   - **Thêm ít nhất 1 step** (có thể không cần nhập text, chỉ cần upload ảnh)
3. Submit

### **Bước 3: Xem logs**

#### ✅ Nếu thành công
```
=== ADD RECIPE STARTED ===
Model: Name=Test Recipe, CookTime=30, Level=dễ, Steps=1
Session[UserId] = NULL
Session[user_id] = admin
Session[username] = admin
Session.GetInt32('UserId') = 1  ← ĐÂY NÀY!
UserId from session: 1
Creating Recipe record...
Recipe created successfully with ID: 123
=== ADD RECIPE COMPLETED SUCCESSFULLY ===
```

#### ❌ Nếu vẫn lỗi
```
Session[UserId] = NULL         ← Không có giá trị!
Session.GetInt32('UserId') = NULL
UserId not found in session or = 0
```

→ **Chưa logout/login lại!** Session cũ vẫn còn.

## 🔧 Troubleshooting

### 1. **Vẫn báo "UserId not found"**

**Nguyên nhân:** Session cũ vẫn còn từ lần login trước

**Giải pháp:**
```
1. Logout
2. Clear browser cookies/cache (Ctrl+Shift+Del)
3. Đóng browser hoàn toàn
4. Mở browser mới
5. Login lại
```

### 2. **CookTime = 0**

**Nguyên nhân:** Input không có giá trị

**Giải pháp:**
- Đảm bảo input có `value="30"`
- Hoặc nhập số > 0 trước khi submit

### 3. **Validation error: "Vui lòng nhập mô tả bước"**

**Giải pháp:** ✅ Đã fix - giờ không bắt buộc nhập text cho step

## 📊 Cấu trúc Session mới

Sau khi login thành công:

```javascript
Session = {
    "UserId": 1,              // ← INT - Dùng cho AddRecipe
    "user_id": "admin",       // ← STRING - Backward compatible
    "username": "admin",
    "user_email": "admin@gmail.com",
    "full_name": "Admin",
    "role": "admin"
}
```

## ✅ Checklist

Trước khi test lại:

- [ ] Đã pull code mới nhất
- [ ] Đã logout
- [ ] Đã clear cookies
- [ ] Đã login lại
- [ ] Check terminal log có "Session set: UserId=..."
- [ ] Mở F12 Console
- [ ] Điền form đầy đủ (CookTime > 0)

## 🎯 Kết quả mong đợi

1. ✅ Login thành công → Log "Session set: UserId=1"
2. ✅ Vào AddRecipe → Không redirect về login
3. ✅ Submit form → Có log chi tiết
4. ✅ Upload ảnh → Files lên bucket `img`
5. ✅ Redirect về Index → Thông báo success

## 📝 Note

- **Session timeout:** 30 phút
- **UserId = 0:** Cũng được coi là không có session
- **Cần login lại** sau mỗi lần update code về session

