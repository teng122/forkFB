using Microsoft.AspNetCore.Mvc;
using foodbook.Attributes;
using foodbook.Services;
using foodbook.Models;
using foodbook.Helpers;

namespace foodbook.Controllers
{
    [LoginRequired]
    public class RecipeController : Controller
    {
        private readonly SupabaseService _supabase;

        public RecipeController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string name,
            string? description,
            int? cookTime,
            string level,
            string? recipeType,
            List<string>? ingredientTags,
            IFormFile? thumbnail,
            List<int>? stepNumbers,
            List<string>? stepInstructions,
            List<IFormFile?>? stepImages)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetUserId();
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    TempData["ErrorMessage"] = "Không xác định được người dùng.";
                    return RedirectToAction("Index", "Home");
                }

                // Ensure Recipe Type
                int? recipeTypeId = null;
                if (!string.IsNullOrWhiteSpace(recipeType))
                {
                    recipeTypeId = await _supabase.EnsureRecipeTypeAsync(recipeType.Trim());
                }

                // Convert thumbnail to byte[]
                byte[]? thumbnailBytes = null;
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await thumbnail.CopyToAsync(ms);
                    thumbnailBytes = ms.ToArray();
                }

                // Create Recipe
                var recipe = new Recipe
                {
                    user_id = userId,
                    recipe_type_id = recipeTypeId,
                    name = name,
                    description = description,
                    cook_time = cookTime,
                    level = level,
                    thumbnail_img = thumbnailBytes,
                    created_at = DateTime.UtcNow
                };

                var inserted = await _supabase.Client.From<Recipe>().Insert(recipe);
                var newRecipe = inserted.Models.First();

                // Ingredients (insert per recipe)
                if (ingredientTags != null)
                {
                    foreach (var ing in ingredientTags.Distinct().Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        await _supabase.Client.From<Ingredient>().Insert(new Ingredient
                        {
                            recipe_id = newRecipe.recipe_id!.Value,
                            name = ing.Trim(),
                            created_at = DateTime.UtcNow
                        });
                    }
                }

                // Steps
                if (stepNumbers != null && stepInstructions != null)
                {
                    for (int i = 0; i < stepNumbers.Count; i++)
                    {
                        var mediaId = (int?)null;
                        if (stepImages != null && i < stepImages.Count && stepImages[i] != null && stepImages[i]!.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            await stepImages[i]!.CopyToAsync(ms);
                            var mediaRes = await _supabase.Client.From<Media>().Insert(new Media { media_img = ms.ToArray() });
                            mediaId = mediaRes.Models.First().media_id;
                        }

                        await _supabase.Client.From<RecipeStep>().Insert(new RecipeStep
                        {
                            recipe_id = newRecipe.recipe_id!.Value,
                            step = stepNumbers[i],
                            instruction = stepInstructions.ElementAtOrDefault(i) ?? string.Empty,
                            media_id = mediaId
                        });
                    }
                }

                TempData["SuccessMessage"] = "Đã lưu công thức thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi lưu công thức: {ex.Message}";
                return View();
            }
        }

        // Suggest tags for Ingredients (distinct names from Ingredient table)
        [HttpGet]
        public async Task<IActionResult> SuggestIngredients(string query)
        {
            var list = await _supabase.SearchIngredientsAsync(query ?? string.Empty);
            return Json(list);
        }

        // Suggest tags for Recipe Types (from Recipe_type)
        [HttpGet]
        public async Task<IActionResult> SuggestTypes(string query)
        {
            var list = await _supabase.SearchRecipeTypesAsync(query ?? string.Empty);
            return Json(list);
        }
    }
}


