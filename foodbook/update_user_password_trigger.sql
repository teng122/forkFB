-- Trigger function để cập nhật password trong bảng User khi User-Trigger được cập nhật
CREATE OR REPLACE FUNCTION update_user_password_trigger()
RETURNS TRIGGER AS $$
BEGIN
    -- Cập nhật password trong bảng User cho TẤT CẢ các hàng có cùng email hoặc username
    -- Vì trong User-Trigger có thể có nhiều hàng cùng email/username
    UPDATE "User" 
    SET password = NEW.password
    WHERE email = NEW.email OR username = NEW.username;
    
    -- Log để debug (có thể xóa sau)
    RAISE NOTICE 'Updated password for email: % and username: %', NEW.email, NEW.username;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Tạo trigger trên bảng User-Trigger
DROP TRIGGER IF EXISTS trigger_update_user_password ON "User-Trigger";

CREATE TRIGGER trigger_update_user_password
    AFTER UPDATE OF password ON "User-Trigger"
    FOR EACH ROW
    EXECUTE FUNCTION update_user_password_trigger();

-- Test trigger (optional)
-- UPDATE "User-Trigger" SET password = 'newpassword123' WHERE email = 'test@example.com';
