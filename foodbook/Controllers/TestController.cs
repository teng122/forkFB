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
                    .From<UserLogin>()
                    .Get();

                // Test kết nối database - User-Trigger table (cho đăng ký)
                var registerUsers = await _supabaseService.Client
                    .From<User>()
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
                    Console.WriteLine($"User: {user.username}, Email: {user.email}, Role: {user.role}, Password: {user.password}");
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
    }
}
