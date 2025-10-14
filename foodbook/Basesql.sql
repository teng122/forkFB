-- Xóa tất cả records trong bảng User
DELETE FROM "User";

-- Reset sequence để bắt đầu từ 1
ALTER SEQUENCE "User_user_id_seq" RESTART WITH 1;
