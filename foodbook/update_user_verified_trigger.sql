-- Trigger function để cập nhật is_verified trong bảng User khi User-Trigger được cập nhật
CREATE OR REPLACE FUNCTION update_user_verified_trigger()
RETURNS TRIGGER AS $$
BEGIN
    -- Cập nhật is_verified trong bảng User dựa trên email
    UPDATE "User" 
    SET is_verified = NEW.is_verified
    WHERE email = NEW.email;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Tạo trigger trên bảng User-Trigger
DROP TRIGGER IF EXISTS trigger_update_user_verified ON "User-Trigger";

CREATE TRIGGER trigger_update_user_verified
    AFTER UPDATE OF is_verified ON "User-Trigger"
    FOR EACH ROW
    EXECUTE FUNCTION update_user_verified_trigger();

-- Test trigger (optional)
-- UPDATE "User-Trigger" SET is_verified = true WHERE email = 'test@example.com';
