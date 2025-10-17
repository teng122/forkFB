using System.Diagnostics;
using foodbook.Models;
using Microsoft.AspNetCore.Mvc;
using foodbook.Attributes;
using foodbook.Services;

namespace foodbook.Controllers
{
    [LoginRequired]
    [AdminRequired]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly SupabaseService _supabaseService;

        public AdminController(ILogger<AdminController> logger, SupabaseService supabaseService)
        {
            _logger = logger;
            _supabaseService = supabaseService;
        }

        // Admin Dashboard Overview
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Get total users
                var totalUsers = await _supabaseService.Client
                    .From<User>()
                    .Select("user_id")
                    .Get();
                
                // Get total recipes
                var totalRecipes = await _supabaseService.Client
                    .From<Recipe>()
                    .Select("recipe_id")
                    .Get();
                
                // Get total likes
                var totalLikes = await _supabaseService.Client
                    .From<likeDislike>()
                    .Select("ld_id")
                    .Get();
                
                // Get total comments
                var totalComments = await _supabaseService.Client
                    .From<Comment>()
                    .Select("comment_id")
                    .Get();

                // Get recent users (last 7 days)
                var recentUsers = await _supabaseService.Client
                    .From<User>()
                    .Select("user_id, username, created_at")
                    .Where(x => x.created_at >= DateTime.UtcNow.AddDays(-7))
                    .Get();

                // Get recent recipes (last 7 days)
                var recentRecipes = await _supabaseService.Client
                    .From<Recipe>()
                    .Select("recipe_id, name, created_at")
                    .Where(x => x.created_at >= DateTime.UtcNow.AddDays(-7))
                    .Get();

                // Get flagged content
                var flaggedContent = await _supabaseService.Client
                    .From<Report>()
                    .Select("user_id, recipe_id, body, status, created_at")
                    .Where(x => x.status == "Đang xử lý")
                    .Get();

                // Get categories
                var categories = await _supabaseService.Client
                    .From<RecipeType>()
                    .Select("recipe_type_id, content, created_at")
                    .Get();

                // Get ingredients
                var ingredients = await _supabaseService.Client
                    .From<Ingredient>()
                    .Select("ingredient_id, name, created_at")
                    .Get();

                var dashboard = new AdminDashboardViewModel
                {
                    TotalUsers = totalUsers.Models.Count,
                    TotalRecipes = totalRecipes.Models.Count,
                    TotalLikes = totalLikes.Models.Count,
                    TotalComments = totalComments.Models.Count,
                    RecentUsers = recentUsers.Models.Take(5).ToList(),
                    RecentRecipes = recentRecipes.Models.Take(5).ToList(),
                    FlaggedContent = flaggedContent.Models.Take(5).ToList(),
                    Categories = categories.Models.Take(5).ToList(),
                    Ingredients = ingredients.Models.Take(5).ToList()
                };

                return View("Dashboard", dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["Error"] = "Không thể tải dashboard: " + ex.Message;
                return View("Dashboard", new AdminDashboardViewModel());
            }
        }

        // User Management
        public async Task<IActionResult> UserManagement()
        {
            return await Users();
        }

        // User Management (alternative route)
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _supabaseService.Client
                    .From<User>()
                    .Select("user_id, username, email, status, created_at")
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                return View("UserManagement", users.Models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user management");
                TempData["Error"] = "Không thể tải danh sách người dùng: " + ex.Message;
                return View("UserManagement", new List<User>());
            }
        }

        // Content Moderation
        public async Task<IActionResult> ContentModeration()
        {
            return await Reports();
        }

        // Content Moderation (alternative route)
        public async Task<IActionResult> Reports()
        {
            try
            {
                var reports = await _supabaseService.Client
                    .From<Report>()
                    .Select("user_id, recipe_id, body, status, created_at")
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                return View("ContentModeration", reports.Models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading content moderation");
                TempData["Error"] = "Không thể tải danh sách báo cáo: " + ex.Message;
                return View("ContentModeration", new List<Report>());
            }
        }

        // Content Moderation (alternative route for /Admin/Content)
        public async Task<IActionResult> Content()
        {
            return await Reports();
        }

        // Category Management
        public async Task<IActionResult> CategoryManagement()
        {
            return await Categories();
        }

        // Category Management (alternative route)
        public async Task<IActionResult> Categories()
        {
            try
            {
                var categories = await _supabaseService.Client
                    .From<RecipeType>()
                    .Select("recipe_type_id, content, created_at")
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                return View("CategoryManagement", categories.Models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category management");
                TempData["Error"] = "Không thể tải danh sách phân loại: " + ex.Message;
                return View("CategoryManagement", new List<RecipeType>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                {
                    TempData["Error"] = "Tên phân loại không được để trống";
                    return RedirectToAction("CategoryManagement");
                }

                var newCategory = new RecipeType
                {
                    content = content.Trim()
                };

                await _supabaseService.Client
                    .From<RecipeType>()
                    .Insert(newCategory);

                TempData["Success"] = "Thêm phân loại thành công";
                return RedirectToAction("CategoryManagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category");
                TempData["Error"] = "Không thể thêm phân loại: " + ex.Message;
                return RedirectToAction("CategoryManagement");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int recipeTypeId)
        {
            try
            {
                await _supabaseService.Client
                    .From<RecipeType>()
                    .Where(x => x.recipe_type_id == recipeTypeId)
                    .Delete();

                TempData["Success"] = "Xóa phân loại thành công";
                return RedirectToAction("CategoryManagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                TempData["Error"] = "Không thể xóa phân loại: " + ex.Message;
                return RedirectToAction("CategoryManagement");
            }
        }

        // Ingredient Management
        public async Task<IActionResult> IngredientManagement()
        {
            return await Ingredients();
        }

        // Ingredient Management (alternative route)
        public async Task<IActionResult> Ingredients()
        {
            try
            {
                var ingredients = await _supabaseService.Client
                    .From<Ingredient>()
                    .Select("ingredient_id, name, created_at")
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                return View("IngredientManagement", ingredients.Models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ingredient management");
                TempData["Error"] = "Không thể tải danh sách nguyên liệu: " + ex.Message;
                return View("IngredientManagement", new List<Ingredient>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIngredient(int ingredientId)
        {
            try
            {
                await _supabaseService.Client
                    .From<Ingredient>()
                    .Where(x => x.ingredient_id == ingredientId)
                    .Delete();

                TempData["Success"] = "Xóa nguyên liệu thành công";
                return RedirectToAction("IngredientManagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ingredient");
                TempData["Error"] = "Không thể xóa nguyên liệu: " + ex.Message;
                return RedirectToAction("IngredientManagement");
            }
        }

        // User Actions
        [HttpPost]
        public async Task<IActionResult> BanUser(int userId)
        {
            try
            {
                await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.user_id == userId)
                    .Set(x => x.status, "banned")
                    .Update();

                TempData["Success"] = "Cấm người dùng thành công";
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user");
                TempData["Error"] = "Không thể cấm người dùng: " + ex.Message;
                return RedirectToAction("UserManagement");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            try
            {
                await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.user_id == userId)
                    .Set(x => x.status, "active")
                    .Update();

                TempData["Success"] = "Bỏ cấm người dùng thành công";
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user");
                TempData["Error"] = "Không thể bỏ cấm người dùng: " + ex.Message;
                return RedirectToAction("UserManagement");
            }
        }

        // Content Moderation Actions
        [HttpPost]
        public async Task<IActionResult> ApproveContent(int userId, int recipeId)
        {
            try
            {
                await _supabaseService.Client
                    .From<Report>()
                    .Where(x => x.user_id == userId && x.recipe_id == recipeId)
                    .Set(x => x.status, "Đã xử lý")
                    .Update();

                TempData["Success"] = "Duyệt nội dung thành công";
                return RedirectToAction("ContentModeration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving content");
                TempData["Error"] = "Không thể duyệt nội dung: " + ex.Message;
                return RedirectToAction("ContentModeration");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectContent(int userId, int recipeId)
        {
            try
            {
                await _supabaseService.Client
                    .From<Report>()
                    .Where(x => x.user_id == userId && x.recipe_id == recipeId)
                    .Set(x => x.status, "Từ chối")
                    .Update();

                TempData["Success"] = "Từ chối nội dung thành công";
                return RedirectToAction("ContentModeration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting content");
                TempData["Error"] = "Không thể từ chối nội dung: " + ex.Message;
                return RedirectToAction("ContentModeration");
            }
        }
    }
}