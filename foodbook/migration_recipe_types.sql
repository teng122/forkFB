-- Migration: Tạo bảng trung gian để hỗ trợ nhiều types per recipe
-- Chạy trên Supabase SQL Editor

-- 1. Tạo bảng trung gian Recipe_RecipeType
CREATE TABLE IF NOT EXISTS public."Recipe_RecipeType" (
  recipe_id integer NOT NULL,
  recipe_type_id integer NOT NULL,
  created_at timestamp with time zone DEFAULT now(),
  CONSTRAINT "Recipe_RecipeType_pkey" PRIMARY KEY (recipe_id, recipe_type_id),
  CONSTRAINT "fk_rrt_recipe" FOREIGN KEY (recipe_id) REFERENCES public."Recipe"(recipe_id) ON DELETE CASCADE,
  CONSTRAINT "fk_rrt_recipe_type" FOREIGN KEY (recipe_type_id) REFERENCES public."Recipe_type"(recipe_type_id) ON DELETE CASCADE
);

-- 2. Migrate dữ liệu từ Recipe.recipe_type_id sang bảng trung gian
INSERT INTO public."Recipe_RecipeType" (recipe_id, recipe_type_id)
SELECT recipe_id, recipe_type_id 
FROM public."Recipe" 
WHERE recipe_type_id IS NOT NULL
ON CONFLICT (recipe_id, recipe_type_id) DO NOTHING;

-- 3. Tạo index để tối ưu performance
CREATE INDEX IF NOT EXISTS "idx_recipe_recipetype_recipe_id" ON public."Recipe_RecipeType"(recipe_id);
CREATE INDEX IF NOT EXISTS "idx_recipe_recipetype_type_id" ON public."Recipe_RecipeType"(recipe_type_id);

-- 4. (Tùy chọn) Xóa cột recipe_type_id cũ nếu muốn
-- ALTER TABLE public."Recipe" DROP COLUMN IF EXISTS recipe_type_id;
