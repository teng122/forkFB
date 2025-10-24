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
                    .From<IngredientMaster>()
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

                // Get TẤT CẢ user reports (tất cả status)
                var allUserReports = await _supabaseService.Client
                    .From<UserReport>()
                    .Get();

                var userList = new List<dynamic>();
                foreach (var user in users.Models ?? new List<User>())
                {
                    // Tổng TẤT CẢ reports cho user này
                    var totalReportCount = allUserReports.Models?
                        .Count(r => r.reported_user_id == user.user_id) ?? 0;

                    // Chỉ đếm các báo cáo đang xử lý
                    var pendingReportCount = allUserReports.Models?
                        .Count(r => r.reported_user_id == user.user_id && r.status == "Đang xử lý") ?? 0;

                    userList.Add(new
                    {
                        UserId = user.user_id,
                        Username = user.username,
                        Email = user.email,
                        Status = user.status,
                        CreatedAt = user.created_at,
                        PendingReportCount = pendingReportCount, // Reports đang xử lý
                        TotalReportCount = totalReportCount // Tổng TẤT CẢ reports
                    });
                }

                return View("UserManagement", userList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user management");
                TempData["Error"] = "Không thể tải danh sách người dùng: " + ex.Message;
                return View("UserManagement", new List<dynamic>());
            }
        }


        // Content Moderation (alternative route for /Admin/Content)
        public async Task<IActionResult> Content()
        {
            return RedirectToAction("ContentModeration");
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
                    .From<IngredientMaster>()
                    .Select("ingredient_id, name, created_at")
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                return View("IngredientManagement", ingredients.Models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ingredient management");
                TempData["Error"] = "Không thể tải danh sách nguyên liệu: " + ex.Message;
                return View("IngredientManagement", new List<IngredientMaster>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddIngredient(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    TempData["Error"] = "Tên nguyên liệu không được để trống";
                    return RedirectToAction("IngredientManagement");
                }

                var newIngredient = new IngredientMaster
                {
                    name = name.Trim()
                };

                await _supabaseService.Client
                    .From<IngredientMaster>()
                    .Insert(newIngredient);

                TempData["Success"] = "Thêm nguyên liệu thành công";
                return RedirectToAction("IngredientManagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ingredient");
                TempData["Error"] = "Không thể thêm nguyên liệu: " + ex.Message;
                return RedirectToAction("IngredientManagement");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIngredient(int ingredientId)
        {
            try
            {
                await _supabaseService.Client
                    .From<IngredientMaster>()
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
                    .Set(x => x.status, "ban")
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

        // Content Moderation Actions
        [HttpGet]
        public async Task<IActionResult> ContentModeration()
        {
            try
            {
                // Lấy admin hiện tại
                var sessionEmail = HttpContext.Session.GetString("user_email");
                var currentAdmin = await _supabaseService.Client
                    .From<User>()
                    .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, sessionEmail)
                    .Single();
                
                var adminId = currentAdmin?.user_id ?? 0;

                // Lấy TẤT CẢ reports (tất cả status) từ DB
                var allReportsResponse = await _supabaseService.Client
                    .From<Report>()
                    .Get();

                var allReports = allReportsResponse.Models ?? new List<Report>();

                // Group TẤT CẢ reports theo recipe_id
                var allReportsByRecipe = allReports
                    .GroupBy(r => r.recipe_id)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Group chỉ pending reports theo recipe_id
                var pendingReportsByRecipe = allReports
                    .Where(r => r.status == "Đang xử lý")
                    .GroupBy(r => r.recipe_id)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Lấy danh sách recipes
                var recipes = await _supabaseService.Client
                    .From<Recipe>()
                    .Get();

                var recipeList = new List<dynamic>();

                foreach (var recipe in recipes.Models ?? new List<Recipe>())
                {
                    // Skip nếu recipe_id null
                    if (!recipe.recipe_id.HasValue) continue;
                    
                    var recipeId = recipe.recipe_id.Value;

                    // Lấy thông tin user
                    var user = await _supabaseService.Client
                        .From<User>()
                        .Where(x => x.user_id == recipe.user_id)
                        .Single();

                    // Lấy TẤT CẢ reports cho recipe này
                    var allRecipeReports = allReportsByRecipe.ContainsKey(recipeId) 
                        ? allReportsByRecipe[recipeId] 
                        : new List<Report>();

                    // Lấy chỉ pending reports cho recipe này
                    var pendingRecipeReports = pendingReportsByRecipe.ContainsKey(recipeId) 
                        ? pendingReportsByRecipe[recipeId] 
                        : new List<Report>();

                    // Đếm số lượng reports
                    var totalReportCount = allRecipeReports.Count; // Tổng TẤT CẢ reports
                    var pendingReportCount = pendingRecipeReports.Count; // Chỉ reports đang xử lý
                    var userReportCount = pendingRecipeReports.Count(r => r.user_id != adminId);
                    var adminReportCount = pendingRecipeReports.Count(r => r.user_id == adminId);
                    var isFlaggedByAdmin = adminReportCount > 0;

                    // Status từ Recipe table: 'active', 'banned', 'pending', 'deleted'
                    string displayStatus = recipe.status switch
                    {
                        "banned" => "Banned",
                        "active" => "Approved",
                        "pending" => "Pending",
                        "deleted" => "Deleted",
                        _ => "Approved"
                    };

                    recipeList.Add(new
                    {
                        RecipeId = recipeId,
                        Title = recipe.name,
                        Author = user?.username ?? "Unknown",
                        Status = displayStatus,
                        RecipeStatus = recipe.status, // Status gốc từ DB
                        Date = recipe.created_at,
                        PendingReportCount = pendingReportCount, // Số reports đang xử lý (chưa xử lý)
                        TotalReportCount = totalReportCount, // Tổng TẤT CẢ reports
                        UserReportCount = userReportCount,
                        AdminReportCount = adminReportCount,
                        IsFlaggedByAdmin = isFlaggedByAdmin,
                        Recipe = recipe,
                        User = user
                    });
                }

                return View(recipeList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading content moderation");
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu: " + ex.Message;
                return View(new List<dynamic>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewRecipeDetails(int recipeId)
        {
            try
            {
                // Lấy thông tin recipe
                var recipe = await _supabaseService.Client
                    .From<Recipe>()
                    .Where(x => x.recipe_id == recipeId)
                    .Single();

                if (recipe == null)
                {
                    return NotFound();
                }

                // Lấy thông tin user
                var user = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.user_id == recipe.user_id)
                    .Single();

                // Lấy các bước nấu
                var steps = await _supabaseService.Client
                    .From<RecipeStep>()
                    .Where(x => x.recipe_id == recipeId)
                    .Order("step", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                // Lấy nguyên liệu từ Recipe_Ingredient
                var recipeIngredients = await _supabaseService.Client
                    .From<RecipeIngredient>()
                    .Where(x => x.recipe_id == recipeId)
                    .Get();

                var ingredients = new List<IngredientMaster>();
                foreach (var ri in recipeIngredients.Models ?? new List<RecipeIngredient>())
                {
                    var ingredient = await _supabaseService.Client
                        .From<IngredientMaster>()
                        .Where(x => x.ingredient_id == ri.ingredient_id)
                        .Single();
                    if (ingredient != null)
                        ingredients.Add(ingredient);
                }

                // Lấy media cho các bước
                var stepMedia = new Dictionary<int, List<Media>>();
                foreach (var step in steps.Models ?? new List<RecipeStep>())
                {
                    var media = await _supabaseService.Client
                        .From<RecipeStepMedia>()
                        .Where(x => x.recipe_id == recipeId && x.step == step.step)
                        .Get();

                    var mediaList = new List<Media>();
                    foreach (var mediaItem in media.Models ?? new List<RecipeStepMedia>())
                    {
                        var mediaData = await _supabaseService.Client
                            .From<Media>()
                            .Where(x => x.media_id == mediaItem.media_id)
                            .Single();
                        if (mediaData != null)
                            mediaList.Add(mediaData);
                    }
                    stepMedia[step.step] = mediaList;
                }

                var viewModel = new
                {
                    Recipe = recipe,
                    User = user,
                    Steps = steps.Models ?? new List<RecipeStep>(),
                    Ingredients = ingredients,
                    StepMedia = stepMedia
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết công thức: " + ex.Message;
                return RedirectToAction("ContentModeration");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkReportProcessed(int userId, int recipeId)
        {
            try
            {
                await _supabaseService.Client
                    .From<Report>()
                    .Where(x => x.user_id == userId && x.recipe_id == recipeId)
                    .Set(x => x.status, "Đã xử lý")
                    .Update();

                TempData["SuccessMessage"] = "Đã đánh dấu report đã xử lý";
                return RedirectToAction("ContentModeration");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật trạng thái report: " + ex.Message;
                return RedirectToAction("ContentModeration");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewReportDetails(int recipeId)
        {
            try
            {
                // Lấy tất cả reports cho recipe này
                var reports = await _supabaseService.Client
                    .From<Report>()
                    .Where(x => x.recipe_id == recipeId)
                    .Get();

                var reportDetails = new List<dynamic>();

                foreach (var report in reports.Models ?? new List<Report>())
                {
                    var reporter = await _supabaseService.Client
                        .From<User>()
                        .Where(x => x.user_id == report.user_id)
                        .Single();

                    reportDetails.Add(new
                    {
                        Report = report,
                        Reporter = reporter
                    });
                }

                return View(reportDetails);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết báo cáo: " + ex.Message;
                return RedirectToAction("ContentModeration");
            }
        }

        // POST: /Admin/FlagRecipe
        [HttpPost]
        public async Task<IActionResult> FlagRecipe(int recipeId)
        {
            try
            {
                // Set recipe status = 'banned'
                await _supabaseService.Client
                    .From<Recipe>()
                    .Where(x => x.recipe_id == recipeId)
                    .Set(x => x.status, "banned")
                    .Update();

                return RedirectToAction("ContentModeration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flagging recipe {RecipeId}", recipeId);
                return RedirectToAction("ContentModeration");
            }
        }

        // POST: /Admin/UnflagRecipe
        [HttpPost]
        public async Task<IActionResult> UnflagRecipe(int recipeId)
        {
            try
            {
                // Set recipe status = 'active'
                await _supabaseService.Client
                    .From<Recipe>()
                    .Where(x => x.recipe_id == recipeId)
                    .Set(x => x.status, "active")
                    .Update();

                return RedirectToAction("ContentModeration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unflagging recipe {RecipeId}", recipeId);
                return RedirectToAction("ContentModeration");
            }
        }

        // GET: /Admin/UserReportManagement
        [HttpGet]
        public async Task<IActionResult> UserReportManagement()
        {
            try
            {
                // Lấy tất cả user reports
                var userReports = await _supabaseService.Client
                    .From<UserReport>()
                    .Get();

                var reportList = new List<dynamic>();

                foreach (var report in userReports.Models ?? new List<UserReport>())
                {
                    // Lấy thông tin người báo cáo
                    var reporter = await _supabaseService.Client
                        .From<User>()
                        .Where(x => x.user_id == report.reporter_id)
                        .Single();

                    // Lấy thông tin người bị báo cáo
                    var reportedUser = await _supabaseService.Client
                        .From<User>()
                        .Where(x => x.user_id == report.reported_user_id)
                        .Single();

                    reportList.Add(new
                    {
                        ReporterId = report.reporter_id,
                        ReportedUserId = report.reported_user_id,
                        ReporterName = reporter?.username ?? "Unknown",
                        ReportedUserName = reportedUser?.username ?? "Unknown",
                        Reason = report.body,
                        Status = report.status,
                        Date = report.created_at,
                        Reporter = reporter,
                        ReportedUser = reportedUser
                    });
                }

                return View(reportList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu: " + ex.Message;
                return View(new List<dynamic>());
            }
        }

        // POST: /Admin/ResolveUserReport
        [HttpPost]
        public async Task<IActionResult> ResolveUserReport(int reporterId, int reportedUserId, string action)
        {
            try
            {
                string newStatus = action == "approve" ? "Đã xử lý" : "Từ chối";

                await _supabaseService.Client
                    .From<UserReport>()
                    .Where(x => x.reporter_id == reporterId && x.reported_user_id == reportedUserId)
                    .Set(x => x.status, newStatus)
                    .Update();

                TempData["SuccessMessage"] = $"Đã {(action == "approve" ? "xử lý" : "từ chối")} báo cáo thành công";
                return RedirectToAction("ViewUserReportDetails", new { userId = reportedUserId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xử lý báo cáo: " + ex.Message;
                return RedirectToAction("ViewUserReportDetails", new { userId = reportedUserId });
            }
        }

        // POST: /Admin/DeleteUserReport
        [HttpPost]
        public async Task<IActionResult> DeleteUserReport(int reporterId, int reportedUserId)
        {
            try
            {
                await _supabaseService.Client
                    .From<UserReport>()
                    .Where(x => x.reporter_id == reporterId && x.reported_user_id == reportedUserId)
                    .Delete();

                TempData["SuccessMessage"] = "Đã xóa báo cáo thành công";
                return RedirectToAction("ViewUserReportDetails", new { userId = reportedUserId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa báo cáo: " + ex.Message;
                return RedirectToAction("ViewUserReportDetails", new { userId = reportedUserId });
            }
        }

        // GET: /Admin/ViewUserReportDetails
        [HttpGet]
        public async Task<IActionResult> ViewUserReportDetails(int userId)
        {
            try
            {
                // Lấy thông tin user bị báo cáo
                var reportedUser = await _supabaseService.Client
                    .From<User>()
                    .Where(x => x.user_id == userId)
                    .Single();

                // Lấy tất cả reports cho user này
                var reports = await _supabaseService.Client
                    .From<UserReport>()
                    .Where(x => x.reported_user_id == userId)
                    .Get();

                var reportDetails = new List<dynamic>();

                foreach (var report in reports.Models ?? new List<UserReport>())
                {
                    var reporter = await _supabaseService.Client
                        .From<User>()
                        .Where(x => x.user_id == report.reporter_id)
                        .Single();

                    reportDetails.Add(new
                    {
                        Report = report,
                        Reporter = reporter,
                        ReportedUser = reportedUser
                    });
                }

                ViewBag.ReportedUser = reportedUser;
                return View(reportDetails);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết báo cáo: " + ex.Message;
                return RedirectToAction("Users");
            }
        }
    }
}