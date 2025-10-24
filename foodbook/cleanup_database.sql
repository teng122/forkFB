-- Query để xóa các bảng/cột không cần thiết trong Basesql.sql
-- Chạy trên Supabase SQL Editor

-- 1. Xóa bảng Ingredient cũ (vì đã có Ingredient_Master)
DROP TABLE IF EXISTS public."Ingredient" CASCADE;

-- 2. Xóa cột recipe_type_id cũ trong bảng Recipe (vì đã có bảng trung gian Recipe_RecipeType)
ALTER TABLE public."Recipe" DROP COLUMN IF EXISTS recipe_type_id;

-- 3. Xóa constraint cũ liên quan đến recipe_type_id
ALTER TABLE public."Recipe" DROP CONSTRAINT IF EXISTS fk_recipe_type;

-- 4. User-Trigger là cần thiết - KHÔNG XÓA
-- DROP TABLE IF EXISTS public."User-Trigger" CASCADE;

-- 5. Kiểm tra và xóa các bảng trùng lặp hoặc không sử dụng
-- (Có thể cần kiểm tra thêm trước khi xóa)

-- 6. Xóa các index không cần thiết (nếu có)
-- DROP INDEX IF EXISTS idx_recipe_recipe_type_id;

-- 7. Cập nhật các view nếu cần
-- (Các view đã được tạo trong migration script)

-- Lưu ý: 
-- - Chạy từng query một để kiểm tra
-- - Backup dữ liệu trước khi chạy
-- - Kiểm tra foreign key constraints trước khi xóa
