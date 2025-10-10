using Microsoft.AspNetCore.Mvc;
using foodbook.Services;
using foodbook.Models;

namespace foodbook.Controllers
{
    public class TestController : Controller
    {
        private readonly SupabaseService _supabaseService;

        public TestController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpGet]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Test kết nối database - User table (cho đăng nhập)
                var loginUsers = await _supabaseService.Client
                    .From<User>()
                    .Get();

                // Test kết nối database - User-Trigger table (cho đăng ký)
                var registerUsers = await _supabaseService.Client
                    .From<UserTrigger>()
                    .Get();

                ViewBag.Message = $"Kết nối thành công! User table: {loginUsers.Models.Count} users, User-Trigger table: {registerUsers.Models.Count} users";
                ViewBag.LoginUsers = loginUsers.Models;
                ViewBag.RegisterUsers = registerUsers.Models;
                
                // Log chi tiết từng user
                Console.WriteLine("=== USER TABLE (Login) ===");
                foreach (var user in loginUsers.Models)
                {
                    Console.WriteLine($"User: {user.username}, Email: {user.email}, Role: {user.role}, Password: {user.password}");
                }
                
                Console.WriteLine("=== USER-TRIGGER TABLE (Register) ===");
                foreach (var user in registerUsers.Models)
                {
                    Console.WriteLine($"User: {user.username}, Email: {user.email}, Role: User, Password: {user.password}");
                }
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Lỗi kết nối database: {ex.Message}";
                Console.WriteLine($"Database error: {ex.Message}");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestLogin(string emailOrPhone, string password)
        {
            try
            {
                var user = await _supabaseService.LoginFromUserTableAsync(emailOrPhone, password);
                if (user != null)
                {
                    ViewBag.Message = $"Đăng nhập thành công! User: {user.username}, Role: {user.role}";
                }
                else
                {
                    ViewBag.Message = "Đăng nhập thất bại: User không tồn tại hoặc password sai";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Lỗi đăng nhập: {ex.Message}";
            }

            return View("TestDatabase");
        }

        [HttpGet]
        public async Task<IActionResult> TestEmailVerification(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    ViewBag.Message = "Email không hợp lệ.";
                    return View("TestDatabase");
                }

                Console.WriteLine($"Testing email verification for: {email}");

                // Chuyển email thành lowercase để so sánh
                var lowercaseEmail = email.ToLower();
                Console.WriteLine($"Searching for email: {lowercaseEmail}");

                // Tìm tất cả users có email này (có thể có nhiều hàng)
                var users = await _supabaseService.Client
                    .From<UserTrigger>()
                    .Where(x => x.email == lowercaseEmail)
                    .Get();

                Console.WriteLine($"Tìm thấy {users.Models.Count} users với email: {lowercaseEmail}");

                if (users.Models.Count == 0)
                {
                    ViewBag.Message = $"Không tìm thấy tài khoản nào với email: {email}";
                    return View("TestDatabase");
                }

                // Hiển thị thông tin tất cả users tìm thấy
                foreach (var user in users.Models)
                {
                    Console.WriteLine($"User: Username='{user.username}', Email='{user.email}', Verified={user.is_verified}");
                }

                // Cập nhật is_verified = true cho TẤT CẢ users có email này
                var updateResult = await _supabaseService.Client
                    .From<UserTrigger>()
                    .Where(x => x.email == lowercaseEmail)
                    .Set(x => x.is_verified, true)
                    .Update();

                Console.WriteLine($"Đã cập nhật {updateResult.Models.Count} users");

                ViewBag.Message = $"Xác thực thành công! Đã cập nhật {users.Models.Count} tài khoản với email: {email}";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Lỗi xác thực: {ex.Message}";
                Console.WriteLine($"Email verification error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return View("TestDatabase");
        }
    }
}
