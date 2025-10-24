# Sửa lỗi nguyên liệu và phân loại trong FoodBook

## Tóm tắt các vấn đề đã được giải quyết

### 1. **Vấn đề xóa nguyên liệu khi xóa công thức**
**Trước:** Khi xóa một công thức, tất cả nguyên liệu trong bảng `Ingredient` cũng bị xóa theo.
**Sau:** Nguyên liệu được lưu trong bảng `Ingredient_Master` riêng biệt và chỉ xóa link trong bảng trung gian `Recipe_Ingredient`.

### 2. **Vấn đề không tạo phân loại mới**
**Trước:** Khi tạo công thức với phân loại mới, phân loại không được thêm vào database.
**Sau:** Hệ thống tự động kiểm tra và tạo phân loại mới nếu chưa tồn tại.

### 3. **Vấn đề trùng lặp nguyên liệu**
**Trước:** Cùng một nguyên liệu có thể có nhiều ID khác nhau trong database.
**Sau:** Mỗi nguyên liệu chỉ có một ID duy nhất trong bảng `Ingredient_Master`.

## ⚠️ Lưu ý quan trọng
**Giao diện người dùng được giữ nguyên hoàn toàn.** Chỉ có logic xử lý backend được thay đổi để sử dụng cấu trúc database mới.

## 🔄 Thay đổi cấu trúc database

### Trước khi sửa:
- Bảng `Recipe` có cột `recipe_type_id` (chỉ hỗ trợ 1 phân loại)
- Bảng `Ingredient` liên kết trực tiếp với `Recipe` (xóa recipe = xóa nguyên liệu)

### Sau khi sửa:
- **Xóa cột `recipe_type_id`** khỏi bảng `Recipe` (vì đã có bảng trung gian)
- **Tạo bảng `Ingredient_Master`** riêng biệt (nguyên liệu không bị xóa)
- **Sử dụng bảng trung gian** `Recipe_RecipeType` và `Recipe_Ingredient`
- **Một công thức có thể có nhiều phân loại và nhiều nguyên liệu**

## Cấu trúc database mới

### Bảng `Ingredient_Master`
```sql
CREATE TABLE public."Ingredient_Master" (
  ingredient_id integer GENERATED ALWAYS AS IDENTITY NOT NULL,
  name character varying NOT NULL UNIQUE,
  created_at timestamp with time zone DEFAULT now(),
  CONSTRAINT "Ingredient_Master_pkey" PRIMARY KEY (ingredient_id)
);
```

### Bảng `Recipe_Ingredient` (trung gian)
```sql
CREATE TABLE public."Recipe_Ingredient" (
  recipe_id integer NOT NULL,
  ingredient_id integer NOT NULL,
  quantity character varying,
  unit character varying,
  created_at timestamp with time zone DEFAULT now(),
  CONSTRAINT "Recipe_Ingredient_pkey" PRIMARY KEY (recipe_id, ingredient_id),
  CONSTRAINT "fk_ri_recipe" FOREIGN KEY (recipe_id) REFERENCES public."Recipe"(recipe_id) ON DELETE CASCADE,
  CONSTRAINT "fk_ri_ingredient" FOREIGN KEY (ingredient_id) REFERENCES public."Ingredient_Master"(ingredient_id) ON DELETE CASCADE
);
```

## Các file đã được tạo/cập nhật

### 1. **Migration Script**
- `fix_ingredient_recipe_structure.sql` - Script migration để chuyển đổi cấu trúc database

### 2. **Models mới**
- `Models/IngredientMaster.cs` - Model cho bảng Ingredient_Master
- `Models/RecipeIngredient.cs` - Model cho bảng trung gian Recipe_Ingredient
- `Models/AddRecipeViewModel.cs` - Cập nhật để hỗ trợ quantity và unit cho nguyên liệu

### 3. **Controller cập nhật**
- `Controllers/RecipeController.cs` - Cập nhật logic xử lý nguyên liệu và phân loại

### 4. **View và CSS**
- Giao diện người dùng được giữ nguyên hoàn toàn
- Không có thay đổi về CSS hay giao diện

## Cách sử dụng

### 1. **Chạy Migration**
```sql
-- Chạy script migration trên Supabase SQL Editor
-- File: fix_ingredient_recipe_structure.sql
```

### 2. **Sử dụng như bình thường**
- URL: `/Recipe/Add`
- Giao diện hoàn toàn giống như trước
- Chỉ có logic xử lý backend được cải thiện

### 3. **Tính năng được cải thiện**
- **Gợi ý nguyên liệu:** Tự động gợi ý từ database (không trùng lặp)
- **Phân loại:** Tự động tạo phân loại mới nếu chưa có
- **Xóa an toàn:** Xóa công thức không ảnh hưởng đến nguyên liệu gốc
- **Không trùng lặp:** Mỗi nguyên liệu chỉ có một ID duy nhất

## Lợi ích

### 1. **Tính nhất quán dữ liệu**
- Nguyên liệu không bị trùng lặp
- Phân loại được tạo tự động
- Dữ liệu được chuẩn hóa

### 2. **Hiệu suất**
- Index được tạo cho các truy vấn thường xuyên
- View được tạo để query dễ dàng

### 3. **Trải nghiệm người dùng**
- Giao diện giữ nguyên như cũ (không thay đổi)
- Gợi ý thông minh từ database
- Validation đầy đủ

## Views hỗ trợ

### 1. **Recipe_Ingredients_View**
```sql
SELECT 
    r.recipe_id,
    r.name as recipe_name,
    ri.quantity,
    ri.unit,
    im.name as ingredient_name,
    im.ingredient_id
FROM public."Recipe" r
JOIN public."Recipe_Ingredient" ri ON r.recipe_id = ri.recipe_id
JOIN public."Ingredient_Master" im ON ri.ingredient_id = im.ingredient_id
ORDER BY r.recipe_id, im.name;
```

### 2. **Recipe_Types_View**
```sql
SELECT 
    r.recipe_id,
    r.name as recipe_name,
    rt.content as type_name,
    rt.recipe_type_id
FROM public."Recipe" r
JOIN public."Recipe_RecipeType" rrt ON r.recipe_id = rrt.recipe_id
JOIN public."Recipe_type" rt ON rrt.recipe_type_id = rt.recipe_type_id
ORDER BY r.recipe_id, rt.content;
```

## Lưu ý quan trọng

1. **Backup dữ liệu** trước khi chạy migration
2. **Test kỹ** trên môi trường development trước khi deploy
3. **Kiểm tra** các view cũ có thể cần cập nhật để sử dụng cấu trúc mới
4. **Cập nhật** các API endpoint nếu cần thiết

## Troubleshooting

### Nếu gặp lỗi khi chạy migration:
1. Kiểm tra quyền truy cập database
2. Đảm bảo không có foreign key constraint conflicts
3. Kiểm tra dữ liệu trùng lặp trước khi chạy

### Nếu có lỗi khi tạo công thức:
1. Kiểm tra migration đã chạy thành công chưa
2. Kiểm tra các model mới có được import đúng không
3. Kiểm tra log để xem lỗi cụ thể
