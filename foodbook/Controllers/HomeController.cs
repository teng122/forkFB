using System.Diagnostics;
using foodbook.Models;
using Microsoft.AspNetCore.Mvc;
using foodbook.Attributes;
using foodbook.Services;

namespace foodbook.Controllers
{
    [LoginRequired]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SupabaseService _supabaseService;

        public HomeController(ILogger<HomeController> logger, SupabaseService supabaseService)
        {
            _logger = logger;
            _supabaseService = supabaseService;
        }

        public IActionResult Index()
        {
            // Redirect to Newsfeed
            return RedirectToAction("Newsfeed");
        }

        public async Task<IActionResult> Newsfeed()
        {
            try
            {
                _logger.LogInformation("Loading newsfeed...");
                
                // Lấy tất cả recipes từ DB
                var recipesResult = await _supabaseService.Client
                    .From<Recipe>()
                    .Select("*")
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();
                
                var recipes = recipesResult.Models;
                _logger.LogInformation("Loaded {Count} recipes from DB", recipes.Count);
                
                // Map sang NewfeedViewModel
                var newsfeedItems = recipes.Select(r => new NewfeedViewModel
                {
                    RecipeId = r.recipe_id ?? 0,
                    RecipeName = r.name ?? "Chưa có tên",
                    Description = r.description ?? "",
                    ThumbnailImg = r.thumbnail_img ?? "", // Đã là URL từ Supabase Storage
                    CreatedAt = r.created_at,
                    Level = r.level ?? "dễ",
                    UserName = "User", // TODO: Join với bảng User
                    UserAvatarUrl = "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png",
                    LikesCount = 0, // TODO: Count từ bảng like_dislike
                    CommentsCount = 0, // TODO: Count từ bảng Comment
                    SharesCount = 0 // TODO: Count từ bảng Share
                }).ToList();
                
                _logger.LogInformation("Mapped {Count} items to NewfeedViewModel", newsfeedItems.Count);
                
                return View(newsfeedItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading newsfeed");
                TempData["Error"] = "Không thể tải bảng tin: " + ex.Message;
                return View(new List<NewfeedViewModel>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // AddRecipe đã được move sang RecipeController

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
