-- Create User_Report table for reporting users
CREATE TABLE IF NOT EXISTS public."User_Report" (
  reporter_id integer NOT NULL,
  reported_user_id integer NOT NULL,
  body text,
  status text NOT NULL DEFAULT 'Đang xử lý',
  created_at timestamp with time zone DEFAULT now(),
  CONSTRAINT User_Report_pkey PRIMARY KEY (reporter_id, reported_user_id),
  CONSTRAINT fk_reporter FOREIGN KEY (reporter_id) REFERENCES public."User"(user_id) ON DELETE CASCADE,
  CONSTRAINT fk_reported_user FOREIGN KEY (reported_user_id) REFERENCES public."User"(user_id) ON DELETE CASCADE,
  CONSTRAINT check_not_self_report CHECK (reporter_id != reported_user_id)
);

-- Add index for faster queries
CREATE INDEX IF NOT EXISTS idx_user_report_status ON public."User_Report"(status);
CREATE INDEX IF NOT EXISTS idx_user_report_created_at ON public."User_Report"(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_user_report_reported_user ON public."User_Report"(reported_user_id);

