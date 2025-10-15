# Cập nhật: Hỗ trợ nhiều ảnh/video cho mỗi Step

## 🎉 Đã hoàn thành

Đã cập nhật tính năng AddRecipe để hỗ trợ **nhiều ảnh/video cho mỗi bước** (RecipeStep).

## 📋 Thay đổi Database

### Bảng mới: `RecipeStep_Media`
Bảng trung gian để link nhiều media với 1 step:

```sql
CREATE TABLE public.RecipeStep_Media (
  recipe_id integer NOT NULL,
  step integer NOT NULL,
  media_id integer NOT NULL,
  display_order integer DEFAULT 1,
  CONSTRAINT RecipeStep_Media_pkey PRIMARY KEY (recipe_id, step, media_id)
);
```

**Các cột:**
- `recipe_id`: ID công thức
- `step`: Số thứ tự bước
- `media_id`: ID media (ảnh/video)
- `display_order`: Thứ tự hiển thị (1, 2, 3...)

### Bảng RecipeStep đã thay đổi
- **Đã xóa** cột `media_id` 
- Giờ link với media thông qua bảng `RecipeStep_Media`

## 🆕 Files mới

### 1. `Models/RecipeStepMedia.cs`
Model C# để map với bảng `RecipeStep_Media`:

```csharp
[Table("RecipeStep_Media")]
public class RecipeStepMedia : BaseModel
{
    public int recipe_id { get; set; }
    public int step { get; set; }
    public int media_id { get; set; }
    public int display_order { get; set; } = 1;
}
```

## 🔄 Files đã sửa

### 1. `Models/RecipeStep.cs`
- Xóa property `media_id`
- Giờ step không trực tiếp link với media

### 2. `Controllers/HomeController.cs`
**Thay đổi logic lưu steps:**

```csharp
// Trước: Chỉ lưu 1 media
// Giờ: Loop qua tất cả files và lưu

for (int i = 0; i < model.Steps.Count; i++)
{
    // 1. Tạo RecipeStep trước
    var recipeStep = new RecipeStep { ... };
    await supabase.Insert(recipeStep);
    
    // 2. Upload tất cả media files
    foreach (var mediaFile in step.StepMedia)
    {
        // Upload lên Storage
        var mediaUrl = await storageService.UploadFileAsync(...);
        
        // Tạo Media record
        var media = new Media { media_img = mediaUrl };
        var createdMedia = await supabase.Insert(media);
        
        // 3. Link step với media qua bảng trung gian
        var recipeStepMedia = new RecipeStepMedia
        {
            recipe_id = recipeId,
            step = stepNumber,
            media_id = createdMedia.media_id,
            display_order = mediaIndex + 1
        };
        await supabase.Insert(recipeStepMedia);
    }
}
```

### 3. `Views/Home/AddRecipe.cshtml`
**UI mới với Grid Layout:**

- Nút "Thêm ảnh/video" với icon
- Grid hiển thị tất cả ảnh/video đã chọn
- Mỗi item có:
  - Preview ảnh/video
  - Nút X để xóa
  - Số thứ tự (1, 2, 3...)
  - Video có icon play overlay

**JavaScript mới:**
```javascript
// Object lưu files cho từng step
let stepMediaFiles = {};

// Khi chọn files
stepMediaFiles[stepIndex] = files;
renderMediaGrid(stepIndex);

// Render grid với preview
function renderMediaGrid(stepIndex) {
    // Hiển thị tất cả files dạng grid
    // Có nút xóa từng file
    // Có số thứ tự
}

// Khi submit
prepareFormSubmit() {
    // Gán files vào input bằng DataTransfer API
}
```

### 4. `wwwroot/css/site.css`
**CSS mới cho UI đẹp:**

- `.step-media-container`: Container chứa upload button và grid
- `.step-media-upload-btn`: Nút upload dạng dashed border, hover hiệu ứng
- `.step-media-grid`: Grid layout responsive (auto-fill, minmax)
- `.step-media-item`: Item ảnh/video với border, hover effect
- `.btn-remove-media`: Nút X màu đỏ, ẩn/hiện khi hover
- `.media-order`: Badge số thứ tự màu xanh
- `.video-overlay`: Icon play cho video
- Responsive: Mobile 2-3 columns, Desktop 4-5 columns

## 🎨 Tính năng UI

### Upload nhiều files
1. Click nút "Thêm ảnh/video"
2. Chọn nhiều files (Ctrl+Click hoặc Shift+Click)
3. Files hiển thị ngay dạng grid

### Preview
- Ảnh: Hiển thị thumbnail
- Video: Hiển thị frame đầu + icon play

### Xóa file
- Hover vào item → nút X hiện ra
- Click X → file bị xóa khỏi danh sách

### Thứ tự hiển thị
- Số 1, 2, 3... ở góc dưới trái
- Đây là thứ tự file sẽ hiển thị trên UI

### Responsive
- Desktop: Grid 4-5 cột
- Tablet: Grid 3-4 cột  
- Mobile: Grid 2-3 cột

## 🔧 Cách sử dụng

### 1. Thêm công thức với nhiều ảnh

```text
1. Điền thông tin công thức (tên, mô tả, nguyên liệu...)
2. Với mỗi bước:
   - Nhập mô tả bước
   - Click "Thêm ảnh/video"
   - Chọn nhiều files (có thể chọn cả ảnh lẫn video)
   - Preview hiển thị dạng grid
   - Có thể xóa file nào không muốn
   - Có thể thêm file mới (click lại nút)
3. Click "Lưu công thức"
```

### 2. Dữ liệu được lưu

```text
Recipe (id=1)
├── RecipeStep (recipe_id=1, step=1)
│   ├── RecipeStep_Media (media_id=10, display_order=1)
│   │   └── Media (id=10, media_img="url1.jpg")
│   ├── RecipeStep_Media (media_id=11, display_order=2)
│   │   └── Media (id=11, media_img="url2.jpg")
│   └── RecipeStep_Media (media_id=12, display_order=3)
│       └── Media (id=12, media_video="url3.mp4")
└── RecipeStep (recipe_id=1, step=2)
    └── RecipeStep_Media (media_id=13, display_order=1)
        └── Media (id=13, media_img="url4.jpg")
```

## 📊 Luồng dữ liệu

### Upload và lưu
```
User chọn files
    ↓
JavaScript lưu vào stepMediaFiles[stepIndex]
    ↓
Render grid preview
    ↓
User submit form
    ↓
prepareFormSubmit() gán files vào input
    ↓
POST /Home/AddRecipe
    ↓
Controller:
    1. Tạo Recipe
    2. Tạo RecipeStep
    3. Loop qua từng file:
       - Upload → Storage (bucket img/videos)
       - Tạo Media với URL
       - Tạo RecipeStep_Media link
    ↓
Redirect về Index với Success message
```

## 🔍 Truy vấn dữ liệu

### Lấy tất cả media của 1 step

```sql
SELECT 
    m.media_id,
    m.media_img,
    m.media_video,
    rsm.display_order
FROM RecipeStep_Media rsm
JOIN Media m ON m.media_id = rsm.media_id
WHERE rsm.recipe_id = 1 AND rsm.step = 1
ORDER BY rsm.display_order;
```

### Lấy tất cả steps với media của recipe

```sql
SELECT 
    rs.step,
    rs.instruction,
    m.media_id,
    m.media_img,
    m.media_video,
    rsm.display_order
FROM RecipeStep rs
LEFT JOIN RecipeStep_Media rsm ON rsm.recipe_id = rs.recipe_id 
                                AND rsm.step = rs.step
LEFT JOIN Media m ON m.media_id = rsm.media_id
WHERE rs.recipe_id = 1
ORDER BY rs.step, rsm.display_order;
```

## ✅ Lợi ích

### 1. User Experience
- Upload nhiều ảnh cùng lúc (không phải upload từng ảnh)
- Preview ngay lập tức
- Xóa/sắp xếp dễ dàng
- UI đẹp, responsive

### 2. Database Design
- Chuẩn hóa tốt (bảng trung gian)
- Dễ mở rộng (thêm metadata cho từng media)
- Hiệu suất tốt (index trên composite key)

### 3. Storage
- Files được organize theo folder: `recipes/{recipeId}/steps/{stepNumber}/`
- Dễ cleanup khi xóa recipe
- CDN friendly (public URLs)

## 🐛 Troubleshooting

### Files không upload được
**Nguyên nhân:** DataTransfer API không support trên browser cũ
**Giải pháp:** Test trên Chrome/Firefox/Edge mới nhất

### Grid không hiển thị
**Nguyên nhân:** CSS chưa load
**Giải pháp:** 
- Clear browser cache
- Check `site.css` đã có CSS mới
- F12 → Network → Check css file

### Files bị mất khi thêm step mới
**Nguyên nhân:** `stepMediaFiles` không persist khi DOM thay đổi
**Giải pháp:** ✅ Đã fix - lưu trong object global

### Upload bị lỗi 413 (Payload too large)
**Nguyên nhân:** Files quá lớn
**Giải pháp:** 
- Giới hạn số files (max 5-10 per step)
- Giới hạn file size (max 10MB ảnh, 50MB video)
- Thêm validation trước khi upload

## 🚀 Mở rộng tương lai

### 1. Drag & Drop
- Kéo thả files vào grid
- Sắp xếp lại thứ tự bằng drag & drop

### 2. Image Editor
- Crop, resize ảnh trước khi upload
- Thêm text, sticker

### 3. Video Thumbnail
- Tự động tạo thumbnail cho video
- Chọn frame làm thumbnail

### 4. Lazy Upload
- Upload từng file ngay khi chọn
- Không phải đợi submit form
- Progress bar cho mỗi file

### 5. Cloud Processing
- Resize ảnh tự động (thumbnail, medium, large)
- Convert video sang format tối ưu
- Compress file

## 📝 Notes

- ✅ Buckets `img` và `videos` phải tạo sẵn trong Supabase Storage
- ✅ Buckets phải set **Public = true**
- ✅ Giới hạn file size trong Supabase Settings
- ✅ Test trên nhiều browser
- ✅ Test upload nhiều files lớn cùng lúc

