using Microsoft.AspNetCore.Mvc;
using foodbook.Services;
using foodbook.Models;
using foodbook.Attributes;
using foodbook.Helpers;

namespace foodbook.Controllers
{
    [AdminRequired]
    public class AdminController : Controller
    {
        private readonly SupabaseService _supabaseService;

        public AdminController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Lấy danh sách tất cả users
                var users = await _supabaseService.Client
                    .From<User>()
                    .Get();

                ViewBag.Users = users.Models;
                ViewBag.CurrentUser = HttpContext.Session.GetUsername();
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dashboard: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            try
            {
                await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.username == userId)
                    .Set(x => x.role, newRole)
                    .Update();

                TempData["SuccessMessage"] = "Cập nhật quyền thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật quyền: {ex.Message}";
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.username == userId)
                    .Delete();

                TempData["SuccessMessage"] = "Xóa user thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa user: {ex.Message}";
            }

            return RedirectToAction("Dashboard");
        }
    }
}
