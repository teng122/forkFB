-- Khôi phục lại bảng User-Trigger
-- Chạy trên Supabase SQL Editor

CREATE TABLE IF NOT EXISTS public."User-Trigger" (
  username character varying NOT NULL,
  full_name character varying,
  email character varying NOT NULL,
  password character varying NOT NULL,
  avatar_img bytea,
  bio text,
  created_at timestamp with time zone DEFAULT now(),
  status USER-DEFINED DEFAULT 'active'::user_status,
  is_verified boolean,
  role USER-DEFINED
);

-- Tạo index cho bảng User-Trigger nếu cần
CREATE INDEX IF NOT EXISTS "idx_user_trigger_username" ON public."User-Trigger"(username);
CREATE INDEX IF NOT EXISTS "idx_user_trigger_email" ON public."User-Trigger"(email);


