-- Migration: Sửa cấu trúc nguyên liệu và phân loại
-- Chạy trên Supabase SQL Editor

-- 1. Tạo bảng Ingredient riêng biệt (không liên kết trực tiếp với recipe)
CREATE TABLE IF NOT EXISTS public."Ingredient_Master" (
  ingredient_id integer GENERATED ALWAYS AS IDENTITY NOT NULL,
  name character varying NOT NULL UNIQUE,
  created_at timestamp with time zone DEFAULT now(),
  CONSTRAINT "Ingredient_Master_pkey" PRIMARY KEY (ingredient_id)
);

-- 2. Tạo bảng trung gian Recipe_Ingredient
CREATE TABLE IF NOT EXISTS public."Recipe_Ingredient" (
  recipe_id integer NOT NULL,
  ingredient_id integer NOT NULL,
  quantity character varying,
  unit character varying,
  created_at timestamp with time zone DEFAULT now(),
  CONSTRAINT "Recipe_Ingredient_pkey" PRIMARY KEY (recipe_id, ingredient_id),
  CONSTRAINT "fk_ri_recipe" FOREIGN KEY (recipe_id) REFERENCES public."Recipe"(recipe_id) ON DELETE CASCADE,
  CONSTRAINT "fk_ri_ingredient" FOREIGN KEY (ingredient_id) REFERENCES public."Ingredient_Master"(ingredient_id) ON DELETE CASCADE
);

-- 3. Migrate dữ liệu từ bảng Ingredient cũ sang cấu trúc mới
-- Lấy tất cả nguyên liệu unique từ bảng cũ
INSERT INTO public."Ingredient_Master" (name, created_at)
SELECT DISTINCT name, MIN(created_at) as created_at
FROM public."Ingredient" 
WHERE name IS NOT NULL AND name != ''
GROUP BY name
ON CONFLICT (name) DO NOTHING;

-- Tạo bảng trung gian Recipe_Ingredient từ dữ liệu cũ
INSERT INTO public."Recipe_Ingredient" (recipe_id, ingredient_id, created_at)
SELECT 
    i.recipe_id,
    im.ingredient_id,
    i.created_at
FROM public."Ingredient" i
JOIN public."Ingredient_Master" im ON im.name = i.name
WHERE i.name IS NOT NULL AND i.name != ''
ON CONFLICT (recipe_id, ingredient_id) DO NOTHING;

-- 4. Tạo index để tối ưu performance
CREATE INDEX IF NOT EXISTS "idx_ingredient_master_name" ON public."Ingredient_Master"(name);
CREATE INDEX IF NOT EXISTS "idx_recipe_ingredient_recipe_id" ON public."Recipe_Ingredient"(recipe_id);
CREATE INDEX IF NOT EXISTS "idx_recipe_ingredient_ingredient_id" ON public."Recipe_Ingredient"(ingredient_id);

-- 5. Xóa cột recipe_type_id cũ vì đã có bảng trung gian Recipe_RecipeType
ALTER TABLE public."Recipe" DROP COLUMN IF EXISTS recipe_type_id;

-- 6. Backup và xóa bảng Ingredient cũ (sau khi đã migrate thành công)
-- Lưu ý: Chỉ chạy dòng này sau khi đã test và đảm bảo dữ liệu đã được migrate đúng
-- DROP TABLE IF EXISTS public."Ingredient";

-- 7. Đảm bảo RecipeType được tạo đúng cách (đã có sẵn nhưng kiểm tra lại)
-- Tạo index cho RecipeType nếu chưa có
CREATE INDEX IF NOT EXISTS "idx_recipe_type_content" ON public."Recipe_type"(content);

-- 8. Kiểm tra và sửa các RecipeType trùng lặp (nếu có)
-- Xóa các RecipeType trùng lặp, giữ lại cái có ID nhỏ nhất
WITH duplicates AS (
    SELECT recipe_type_id, 
           ROW_NUMBER() OVER (PARTITION BY LOWER(TRIM(content)) ORDER BY recipe_type_id) as rn
    FROM public."Recipe_type"
    WHERE content IS NOT NULL
)
DELETE FROM public."Recipe_type" 
WHERE recipe_type_id IN (
    SELECT recipe_type_id 
    FROM duplicates 
    WHERE rn > 1
);

-- 9. Cập nhật Recipe_RecipeType để sử dụng RecipeType đã được clean up
-- Xóa các link không hợp lệ
DELETE FROM public."Recipe_RecipeType" 
WHERE recipe_type_id NOT IN (SELECT recipe_type_id FROM public."Recipe_type");

-- 10. Tạo view để dễ dàng query nguyên liệu của recipe
CREATE OR REPLACE VIEW public."Recipe_Ingredients_View" AS
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

-- 11. Tạo view để dễ dàng query phân loại của recipe
CREATE OR REPLACE VIEW public."Recipe_Types_View" AS
SELECT 
    r.recipe_id,
    r.name as recipe_name,
    rt.content as type_name,
    rt.recipe_type_id
FROM public."Recipe" r
JOIN public."Recipe_RecipeType" rrt ON r.recipe_id = rrt.recipe_id
JOIN public."Recipe_type" rt ON rrt.recipe_type_id = rt.recipe_type_id
ORDER BY r.recipe_id, rt.content;
