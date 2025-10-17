using System.Diagnostics;
using foodbook.Models;
using Microsoft.AspNetCore.Mvc;
using foodbook.Attributes;
using foodbook.Services;

namespace foodbook.Controllers
{
    [LoginRequired]
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> _logger;
        private readonly SupabaseService _supabaseService;

        public SearchController(ILogger<SearchController> logger, SupabaseService supabaseService)
        {
            _logger = logger;
            _supabaseService = supabaseService;
        }

        // Search page
        public async Task<IActionResult> Search()
        {
            try
            {
                // Get all ingredients for filter
                var ingredients = await _supabaseService.Client
                    .From<Ingredient>()
                    .Select("name")
                    .Get();

                // Get all recipe types for filter
                var recipeTypes = await _supabaseService.Client
                    .From<RecipeType>()
                    .Select("content")
                    .Get();

                // Get all recipes for display
                var recipes = await _supabaseService.Client
                    .From<Recipe>()
                    .Select("*")
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                // Map to SearchViewModel
                var searchResults = new List<SearchResultViewModel>();
                foreach (var recipe in recipes.Models)
                {
                    // Get user info
                    var user = await _supabaseService.Client
                        .From<User>()
                        .Select("full_name, avatar_img")
                        .Where(x => x.user_id == recipe.user_id)
                        .Get();

                    var userInfo = user.Models.FirstOrDefault();

                    // Get likes count
                    var likesCount = await _supabaseService.Client
                        .From<likeDislike>()
                        .Select("ld_id")
                        .Where(x => x.recipe_id == recipe.recipe_id)
                        .Get();

                    searchResults.Add(new SearchResultViewModel
                    {
                        RecipeId = recipe.recipe_id ?? 0,
                        RecipeName = recipe.name ?? "Chưa có tên",
                        ThumbnailImg = recipe.thumbnail_img ?? "",
                        Level = recipe.level ?? "dễ",
                        UserName = userInfo?.full_name ?? "User",
                        UserAvatarUrl = userInfo?.avatar_img ?? "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png",
                        LikesCount = likesCount.Models.Count
                    });
                }

                var viewModel = new SearchViewModel
                {
                    SearchResults = searchResults,
                    Ingredients = ingredients.Models.Select(x => x.name).Distinct().ToList(),
                    RecipeTypes = recipeTypes.Models.Select(x => x.content).ToList()
                };

                return View("Search", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading search page");
                TempData["Error"] = "Không thể tải trang tìm kiếm: " + ex.Message;
                return View("Search", new SearchViewModel());
            }
        }

        // Search with filters
        [HttpPost]
        public async Task<IActionResult> Search(SearchRequestModel request)
        {
            try
            {
                var query = _supabaseService.Client
                    .From<Recipe>()
                    .Select("*");

                // Apply filters
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(x => x.name.Contains(request.SearchTerm));
                }

                // Apply ingredient filter
                if (request.SelectedIngredients != null && request.SelectedIngredients.Any())
                {
                    var ingredientRecipes = await _supabaseService.Client
                        .From<Ingredient>()
                        .Select("recipe_id")
                        .Get();

                    var filteredIngredientRecipes = ingredientRecipes.Models
                        .Where(x => request.SelectedIngredients.Contains(x.name))
                        .Select(x => x.recipe_id)
                        .ToList();
                    
                    if (filteredIngredientRecipes.Any())
                    {
                        // We'll filter after getting all recipes
                    }
                }

                // Apply recipe type filter
                if (request.SelectedTypes != null && request.SelectedTypes.Any())
                {
                    var typeRecipes = await _supabaseService.Client
                        .From<RecipeRecipeType>()
                        .Select("recipe_id")
                        .Get();

                    var typeDetails = await _supabaseService.Client
                        .From<RecipeType>()
                        .Select("recipe_type_id, content")
                        .Get();

                    var filteredTypeDetails = typeDetails.Models
                        .Where(x => request.SelectedTypes.Contains(x.content))
                        .Select(x => x.recipe_type_id)
                        .ToList();
                    
                    if (filteredTypeDetails.Any())
                    {
                        var filteredTypeRecipes = typeRecipes.Models
                            .Where(x => filteredTypeDetails.Contains(x.recipe_type_id))
                            .Select(x => x.recipe_id)
                            .ToList();

                        if (filteredTypeRecipes.Any())
                        {
                            // We'll filter after getting all recipes
                        }
                    }
                }

                // Apply sorting
                switch (request.SortBy)
                {
                    case "likes":
                        query = query.Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending);
                        break;
                    case "time":
                        query = query.Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending);
                        break;
                    default:
                        query = query.Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending);
                        break;
                }

                var recipes = await query.Get();

                // Map to SearchResultViewModel
                var searchResults = new List<SearchResultViewModel>();
                foreach (var recipe in recipes.Models)
                {
                    // Get user info
                    var user = await _supabaseService.Client
                        .From<User>()
                        .Select("full_name, avatar_img")
                        .Where(x => x.user_id == recipe.user_id)
                        .Get();

                    var userInfo = user.Models.FirstOrDefault();

                    // Get likes count
                    var likesCount = await _supabaseService.Client
                        .From<likeDislike>()
                        .Select("ld_id")
                        .Where(x => x.recipe_id == recipe.recipe_id)
                        .Get();

                    searchResults.Add(new SearchResultViewModel
                    {
                        RecipeId = recipe.recipe_id ?? 0,
                        RecipeName = recipe.name ?? "Chưa có tên",
                        ThumbnailImg = recipe.thumbnail_img ?? "",
                        Level = recipe.level ?? "dễ",
                        UserName = userInfo?.full_name ?? "User",
                        UserAvatarUrl = userInfo?.avatar_img ?? "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png",
                        LikesCount = likesCount.Models.Count
                    });
                }

                return Json(new { success = true, results = searchResults });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search");
                return Json(new { success = false, message = "Không thể thực hiện tìm kiếm: " + ex.Message });
            }
        }
    }
}
