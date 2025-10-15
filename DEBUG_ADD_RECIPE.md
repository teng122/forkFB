# Hướng dẫn Debug AddRecipe

## ✅ Đã thêm Error Handling và Logging

Tôi đã thêm chi tiết error handling và logging để debug lỗi khi add recipe không thành công.

## 🔍 Cách xem lỗi

### 1. **Trên UI (Browser)**

Khi submit form, bạn sẽ thấy:

#### ✅ Thành công
```
┌───────────────────────────────────┐
│ ✓ Đã thêm công thức 'Cơm chiên'  │
│   thành công!                     │
└───────────────────────────────────┘
```

#### ❌ Lỗi
```
┌───────────────────────────────────┐
│ ⚠ Lỗi: Có lỗi xảy ra: [Chi tiết] │
│                                   │
│ - Kiểm tra kết nối Database...   │
└───────────────────────────────────┘
```

#### ⚠ Validation Errors
```
┌───────────────────────────────────┐
│ ⚠ Validation Errors:              │
│   • Vui lòng nhập tên công thức  │
│   • Vui lòng nhập thời gian nấu  │
└───────────────────────────────────┘
```

### 2. **Console Browser (F12)**

Mở Console trong DevTools (F12) để xem:

```javascript
=== PREPARING FORM SUBMIT ===
Ingredients: ["Gà", "Trứng", "Cơm"]
Categories: ["Việt Nam", "Món chính"]
Step Media Files: {0: [File, File], 1: [File]}
Step 0: 2 files
  - anh1.jpg (245678 bytes)
  - anh2.jpg (198765 bytes)
=== FORM DATA ===
Name: Cơm chiên
CookTime: 30
Level: dễ
Ingredients[0]: Gà
Ingredients[1]: Trứng
Steps[0].Instruction: Đun nóng chảo...
Steps[0].StepMedia: [File] anh1.jpg (245678 bytes)
Steps[0].StepMedia: [File] anh2.jpg (198765 bytes)
Form is valid, submitting...
```

### 3. **Server Logs (Terminal/Output)**

Chạy app với:
```bash
dotnet run
```

Logs sẽ hiện trong terminal:

```
=== ADD RECIPE STARTED ===
Model: Name=Cơm chiên, CookTime=30, Level=dễ, Steps=2
UserId from session: 123

Uploading thumbnail: thumb.jpg (156789 bytes)
UploadFileAsync called: thumb.jpg, isVideo=False, folder=recipes/thumbnails
Using bucket: img
File path: recipes/thumbnails/abc-123-def.jpg
File read: 156789 bytes
Uploading to Supabase Storage...
Upload successful!
Public URL: https://...supabase.co/storage/v1/object/public/img/...
Thumbnail uploaded: https://...

Creating Recipe record...
Recipe object: {"user_id":123,"name":"Cơm chiên",...}
Recipe insert result: 1 records
Recipe created successfully with ID: 456

Saving 3 ingredients
  - Saved ingredient: Gà
  - Saved ingredient: Trứng
  - Saved ingredient: Cơm

Saving 2 steps
Step 1: Đun nóng chảo với dầu ăn...
  - RecipeStep saved
  - Processing 2 media files
    [1] anh1.jpg (245678 bytes)
      Type: Image
      Uploaded to: https://...
      Media record created: ID=789
      RecipeStep_Media link created
    [2] anh2.jpg (198765 bytes)
      Type: Image
      Uploaded to: https://...
      Media record created: ID=790
      RecipeStep_Media link created

=== ADD RECIPE COMPLETED SUCCESSFULLY ===
```

## 🐛 Các lỗi thường gặp

### 1. **Không có UserId trong session**

**Lỗi:**
```
UserId not found in session
Lỗi: Vui lòng đăng nhập!
```

**Nguyên nhân:** Chưa đăng nhập hoặc session hết hạn

**Giải pháp:** Đăng nhập lại

### 2. **Bucket không tồn tại**

**Lỗi:**
```
Upload failed: Bucket not found
Không thể upload file 'anh.jpg': Bucket not found
```

**Nguyên nhân:** Chưa tạo bucket `img` hoặc `videos` trong Supabase Storage

**Giải pháp:**
1. Vào Supabase Dashboard
2. Storage → Buckets
3. Tạo bucket mới:
   - Name: `img` (cho ảnh)
   - Name: `videos` (cho video)
   - Public: ✅ **BẮT BUỘC**

### 3. **Permission denied**

**Lỗi:**
```
Permission denied
```

**Nguyên nhân:** Bucket không public hoặc RLS policy chặn

**Giải pháp:**
1. Vào Storage → Bucket Settings
2. Set Public = true
3. Hoặc tắt RLS policies

### 4. **Không thể tạo Recipe**

**Lỗi:**
```
Failed to create recipe - no recipe_id returned
Không thể tạo công thức - không nhận được ID
```

**Nguyên nhân:** 
- Bảng Recipe không có IDENTITY/AUTO_INCREMENT trên recipe_id
- RLS policy chặn INSERT
- Foreign key constraint fail (user_id không tồn tại)

**Giải pháp:**
1. Check DB schema:
   ```sql
   SELECT column_name, is_identity, data_type 
   FROM information_schema.columns 
   WHERE table_name = 'Recipe';
   ```
2. Tắt RLS tạm thời để test:
   ```sql
   ALTER TABLE "Recipe" DISABLE ROW LEVEL SECURITY;
   ```

### 5. **Foreign key constraint**

**Lỗi:**
```
insert or update on table "RecipeStep" violates foreign key constraint
```

**Nguyên nhân:** recipe_id không tồn tại trong bảng Recipe

**Giải pháp:** Check xem Recipe có được tạo thành công không

### 6. **File quá lớn**

**Lỗi:**
```
File size exceeds maximum allowed
```

**Nguyên nhân:** File > limit của Supabase (mặc định 50MB)

**Giải pháp:** Resize/compress file trước khi upload

## 🔧 Checklist Debug

Khi gặp lỗi, check theo thứ tự:

- [ ] **Browser Console (F12)** - Có log form data không?
- [ ] **Network Tab** - Request có gửi đi không? Status code là gì?
- [ ] **Server Logs** - Có log "ADD RECIPE STARTED" không?
- [ ] **Session** - UserId có trong session không?
- [ ] **Supabase Buckets** - Có buckets `img` và `videos` chưa?
- [ ] **Bucket Public** - Buckets có public = true không?
- [ ] **Database** - Bảng Recipe, RecipeStep, Media có tồn tại không?
- [ ] **Foreign Keys** - user_id có tồn tại trong bảng User không?

## 📊 Kiểm tra Database sau khi add

### 1. Check Recipe đã tạo chưa
```sql
SELECT * FROM "Recipe" ORDER BY created_at DESC LIMIT 1;
```

### 2. Check Ingredients
```sql
SELECT * FROM "Ingredient" WHERE recipe_id = <ID>;
```

### 3. Check Steps
```sql
SELECT * FROM "RecipeStep" WHERE recipe_id = <ID> ORDER BY step;
```

### 4. Check Media
```sql
SELECT 
    rs.step,
    m.media_id,
    m.media_img,
    m.media_video,
    rsm.display_order
FROM "RecipeStep" rs
LEFT JOIN "RecipeStep_Media" rsm ON rsm.recipe_id = rs.recipe_id 
                                  AND rsm.step = rs.step
LEFT JOIN "Media" m ON m.media_id = rsm.media_id
WHERE rs.recipe_id = <ID>
ORDER BY rs.step, rsm.display_order;
```

## 🎯 Test Case

### Test thành công

1. **Đăng nhập**
2. **Vào /Home/AddRecipe**
3. **Điền form:**
   - Tên: "Test Recipe"
   - Thời gian: 30
   - Độ khó: Dễ
   - Ingredients: Thêm 2-3 nguyên liệu
   - Categories: Thêm 1-2 phân loại
4. **Thêm 2 steps:**
   - Step 1: Nhập mô tả + upload 2 ảnh
   - Step 2: Nhập mô tả + upload 1 ảnh
5. **Submit**
6. **Kiểm tra:**
   - ✅ Có thông báo success
   - ✅ Redirect về Index
   - ✅ Check DB có dữ liệu
   - ✅ Check Supabase Storage có files

### Test validation

1. **Submit form trống** → Có lỗi validation
2. **Chỉ điền tên, không điền thời gian** → Có lỗi
3. **Upload file không phải ảnh** → Log warning

### Test edge cases

1. **Upload 10 ảnh cho 1 step** → Thành công
2. **Upload file 20MB** → Có thể fail nếu vượt limit
3. **Không upload ảnh nào** → Vẫn tạo recipe được
4. **Logout giữa chừng** → Redirect về login

## 💡 Tips

1. **Luôn mở Console (F12)** khi test
2. **Check cả Browser Console và Server Logs**
3. **Test từng bước**: Tạo recipe đơn giản trước, sau đó mới thêm ảnh
4. **Check Supabase Dashboard** xem files có upload lên không
5. **Dùng Postman** test API nếu cần

## 📞 Support

Nếu vẫn lỗi, cung cấp:
1. Screenshot error trên UI
2. Browser console logs
3. Server logs (terminal output)
4. Supabase Dashboard screenshots (Buckets, RLS policies)

