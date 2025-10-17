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
                var newsfeedItems = new List<NewfeedViewModel>();
                
                foreach (var r in recipes)
                {
                    var recipeId = r.recipe_id ?? 0;
                    
                    // Load tags từ Ingredient và RecipeType
                    var tags = new List<string>();
                    
                    // Thêm level làm tag đầu tiên
                    if (!string.IsNullOrEmpty(r.level))
                    {
                        tags.Add($"#{r.level}");
                    }
                    
                    // Load ingredients
                    try
                    {
                        var ingredients = await _supabaseService.Client
                            .From<Ingredient>()
                            .Select("name")
                            .Where(x => x.recipe_id == recipeId)
                            .Get();
                            
                        foreach (var ing in ingredients.Models)
                        {
                            if (!string.IsNullOrEmpty(ing.name))
                            {
                                tags.Add($"#{ing.name.Trim()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load ingredients for recipe {RecipeId}", recipeId);
                    }
                    
                    // Load recipe types qua bảng trung gian Recipe_RecipeType
                    try
                    {
                        var recipeTypes = await _supabaseService.Client
                            .From<RecipeRecipeType>()
                            .Select("recipe_type_id")
                            .Where(x => x.recipe_id == recipeId)
                            .Get();

                        foreach (var rrt in recipeTypes.Models)
                        {
                            var typeDetail = await _supabaseService.Client
                                .From<RecipeType>()
                                .Select("content")
                                .Where(x => x.recipe_type_id == rrt.recipe_type_id)
                                .Get();

                            if (typeDetail.Models.Any() && !string.IsNullOrEmpty(typeDetail.Models.First().content))
                            {
                                tags.Add($"#{typeDetail.Models.First().content.Trim()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load recipe types for recipe {RecipeId}", recipeId);
                    }
                    
                    // Load user info
                    string userName = "User";
                    string userAvatarUrl = "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png";
                    
                    if (r.user_id > 0)
                    {
                        try
                        {
                            var userResult = await _supabaseService.Client
                                .From<User>()
                                .Select("full_name, avatar_img")
                                .Where(x => x.user_id == r.user_id)
                                .Get();
                                
                            if (userResult.Models.Any())
                            {
                                var user = userResult.Models.First();
                                userName = user.full_name ?? "User";
                                userAvatarUrl = user.avatar_img ?? "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load user info for recipe {RecipeId}", recipeId);
                        }
                    }
                    
                    var item = new NewfeedViewModel
                    {
                        RecipeId = recipeId,
                        RecipeName = r.name ?? "Chưa có tên",
                        Description = r.description ?? "",
                        ThumbnailImg = r.thumbnail_img ?? "",
                        CreatedAt = r.created_at,
                        Level = r.level ?? "dễ",
                        UserName = userName,
                        UserAvatarUrl = userAvatarUrl,
                        LikesCount = 0, // TODO: Count từ bảng like_dislike
                        CommentsCount = 0, // TODO: Count từ bảng Comment
                        SharesCount = 0, // TODO: Count từ bảng Share
                        Tags = tags
                    };
                    
                    newsfeedItems.Add(item);
                }
                
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
