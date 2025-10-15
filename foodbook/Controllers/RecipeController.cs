using System.Diagnostics;
using foodbook.Models;
using Microsoft.AspNetCore.Mvc;
using foodbook.Attributes;
using foodbook.Services;
using foodbook.Helpers;

namespace foodbook.Controllers
{
    [LoginRequired]
    public class RecipeController : Controller
    {
        private readonly ILogger<RecipeController> _logger;
        private readonly SupabaseService _supabaseService;
        private readonly StorageService _storageService;

        public RecipeController(
            ILogger<RecipeController> logger, 
            SupabaseService supabaseService,
            StorageService storageService)
        {
            _logger = logger;
            _supabaseService = supabaseService;
            _storageService = storageService;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddRecipeViewModel model)
        {
            _logger.LogInformation("=== ADD RECIPE STARTED ===");
            
            // Log validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogError("ModelState invalid: {Errors}", string.Join(", ", errors));
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin! " + string.Join(", ", errors);
                return View(model);
            }

            try
            {
                _logger.LogInformation("Model: Name={Name}, CookTime={CookTime}, Level={Level}, Steps={StepsCount}", 
                    model.Name, model.CookTime, model.Level, model.Steps?.Count ?? 0);
                
                // Lấy user_id từ session
                var userId = HttpContext.Session.GetInt32("UserId");
                
                // Debug session
                var sessionKeys = new[] { "UserId", "user_id", "username", "user_email", "role" };
                foreach (var key in sessionKeys)
                {
                    var value = HttpContext.Session.GetString(key);
                    _logger.LogInformation("Session[{Key}] = {Value}", key, value ?? "NULL");
                }
                _logger.LogInformation("Session.GetInt32('UserId') = {UserId}", userId);
                
                if (userId == null || userId == 0)
                {
                    _logger.LogError("UserId not found in session or = 0");
                    TempData["Error"] = "Vui lòng đăng nhập lại! (Session timeout)";
                    return RedirectToAction("Login", "Account");
                }
                
                _logger.LogInformation("UserId from session: {UserId}", userId);

                // 1. Tạo Recipe trong DB trước (chưa có thumbnail)
                _logger.LogInformation("Creating Recipe record...");
                
                var recipe = new Recipe
                {
                    user_id = userId.Value,
                    name = model.Name,
                    thumbnail_img = null, // Sẽ update sau
                    description = model.Description,
                    cook_time = model.CookTime,
                    level = model.Level,
                    step_number = model.Steps?.Count ?? 0,
                    created_at = DateTime.UtcNow
                };

                _logger.LogInformation("Recipe object: {@Recipe}", recipe);

                var recipeResult = await _supabaseService.Client
                    .From<Recipe>()
                    .Insert(recipe);

                _logger.LogInformation("Recipe insert result: {Count} records", recipeResult.Models.Count);

                var createdRecipe = recipeResult.Models.FirstOrDefault();
                if (createdRecipe == null || createdRecipe.recipe_id == null)
                {
                    _logger.LogError("Failed to create recipe - no recipe_id returned");
                    throw new Exception("Không thể tạo công thức - không nhận được ID");
                }

                var recipeId = createdRecipe.recipe_id.Value;
                _logger.LogInformation("Recipe created successfully with ID: {RecipeId}", recipeId);

                // 2. Upload thumbnail với recipe ID và update lại Recipe
                string? thumbnailUrl = null;
                if (model.MainMedia != null)
                {
                    _logger.LogInformation("Uploading MainMedia as thumbnail: {FileName} ({Size} bytes)", 
                        model.MainMedia.FileName, model.MainMedia.Length);
                    
                    var isVideo = _storageService.IsVideoFile(model.MainMedia);
                    
                    thumbnailUrl = await _storageService.UploadFileAsync(
                        model.MainMedia, 
                        isVideo: isVideo, 
                        folderPath: $"recipes/{recipeId}" // ← CÓ RECIPE ID!
                    );
                    
                    _logger.LogInformation("MainMedia uploaded as thumbnail: {Url}", thumbnailUrl);
                    
                    // Update Recipe với thumbnail URL
                    if (!string.IsNullOrEmpty(thumbnailUrl))
                    {
                        var updateResult = await _supabaseService.Client
                            .From<Recipe>()
                            .Where(x => x.recipe_id == recipeId)
                            .Set(x => x.thumbnail_img, thumbnailUrl!)
                            .Update();
                            
                        _logger.LogInformation("Updated {Count} recipe records with thumbnail", updateResult.Models.Count);
                    }
                        
                    _logger.LogInformation("Recipe updated with thumbnail URL");
                }
                else
                {
                    _logger.LogInformation("No MainMedia provided for thumbnail");
                }

                // 3. Lưu Ingredients
                if (model.Ingredients != null && model.Ingredients.Any())
                {
                    _logger.LogInformation("Saving {Count} ingredients", model.Ingredients.Count);
                    
                    foreach (var ingredientName in model.Ingredients)
                    {
                        var ingredient = new Ingredient
                        {
                            recipe_id = recipeId,
                            name = ingredientName,
                            created_at = DateTime.UtcNow
                        };

                        await _supabaseService.Client
                            .From<Ingredient>()
                            .Insert(ingredient);
                            
                        _logger.LogInformation("  - Saved ingredient: {Name}", ingredientName);
                    }
                }
                else
                {
                    _logger.LogInformation("No ingredients to save");
                }

                // 4. Lưu Recipe Types (nếu có bảng này)
                // TODO: Implement nếu cần

                // 5. Lưu Recipe Steps với nhiều Media
                if (model.Steps != null && model.Steps.Any())
                {
                    _logger.LogInformation("Saving {Count} steps", model.Steps.Count);
                    
                    for (int i = 0; i < model.Steps.Count; i++)
                    {
                        var step = model.Steps[i];
                        var stepNumber = i + 1;

                        var instructionPreview = step.Instruction != null && step.Instruction.Length > 50 
                            ? step.Instruction.Substring(0, 50) + "..." 
                            : step.Instruction ?? "";
                        _logger.LogInformation("Step {StepNumber}: {Instruction}", stepNumber, instructionPreview);

                        // Tạo RecipeStep trước
                        var recipeStep = new RecipeStep
                        {
                            recipe_id = recipeId,
                            step = stepNumber,
                            instruction = step.Instruction ?? ""
                        };

                        await _supabaseService.Client
                            .From<RecipeStep>()
                            .Insert(recipeStep);
                            
                        _logger.LogInformation("  - RecipeStep saved");

                        // Lấy danh sách files cần upload
                        var mediaFiles = new List<IFormFile>();
                        
                        // Ưu tiên StepMedia (nhiều files)
                        if (step.StepMedia != null && step.StepMedia.Any())
                        {
                            mediaFiles.AddRange(step.StepMedia);
                        }
                        // Fallback sang StepImage (1 file) nếu không có StepMedia
                        else if (step.StepImage != null)
                        {
                            mediaFiles.Add(step.StepImage);
                        }

                        // Upload và link tất cả media files với step này
                        if (mediaFiles.Any())
                        {
                            _logger.LogInformation("  - Processing {Count} media files", mediaFiles.Count);
                            
                            for (int mediaIndex = 0; mediaIndex < mediaFiles.Count; mediaIndex++)
                            {
                                var mediaFile = mediaFiles[mediaIndex];
                                
                                _logger.LogInformation("    [{Index}] {FileName} ({Size} bytes)", 
                                    mediaIndex + 1, mediaFile.FileName, mediaFile.Length);
                                
                                // Kiểm tra loại file
                                var isVideo = _storageService.IsVideoFile(mediaFile);
                                _logger.LogInformation("      Type: {Type}", isVideo ? "Video" : "Image");
                                
                                // Upload lên storage
                                var mediaUrl = await _storageService.UploadFileAsync(
                                    mediaFile, 
                                    isVideo: isVideo, 
                                    folderPath: $"recipes/{recipeId}/steps/{stepNumber}"
                                );
                                
                                _logger.LogInformation("      Uploaded to: {Url}", mediaUrl);

                                // Tạo Media record trong DB
                                var media = new Media
                                {
                                    media_img = isVideo ? null : mediaUrl,
                                    media_video = isVideo ? mediaUrl : null
                                };

                                var mediaResult = await _supabaseService.Client
                                    .From<Media>()
                                    .Insert(media);

                                var createdMedia = mediaResult.Models.FirstOrDefault();
                                if (createdMedia?.media_id != null)
                                {
                                    _logger.LogInformation("      Media record created: ID={MediaId}", createdMedia.media_id);
                                    
                                    // Tạo RecipeStep_Media để link step với media
                                    var recipeStepMedia = new RecipeStepMedia
                                    {
                                        recipe_id = recipeId,
                                        step = stepNumber,
                                        media_id = createdMedia.media_id.Value,
                                        display_order = mediaIndex + 1  // Thứ tự hiển thị
                                    };

                                    await _supabaseService.Client
                                        .From<RecipeStepMedia>()
                                        .Insert(recipeStepMedia);
                                        
                                    _logger.LogInformation("      RecipeStep_Media link created");
                                }
                                else
                                {
                                    _logger.LogWarning("      Failed to create Media record");
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("  - No media files for this step");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No steps to save");
                }

                _logger.LogInformation("=== ADD RECIPE COMPLETED SUCCESSFULLY ===");
                TempData["Success"] = $"Đã thêm công thức '{model.Name}' thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ADD RECIPE FAILED ===");
                _logger.LogError("Exception Type: {Type}", ex.GetType().Name);
                _logger.LogError("Message: {Message}", ex.Message);
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
                }
                
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                
                // Nếu là lỗi Supabase, log thêm
                if (ex.Message.Contains("Supabase") || ex.Message.Contains("Postgrest"))
                {
                    TempData["Error"] += " - Kiểm tra kết nối Database hoặc quyền truy cập.";
                }
                
                return View(model);
            }
        }

        // TODO: Thêm các actions khác sau
        // [HttpGet] public IActionResult Edit(int id)
        // [HttpPost] public IActionResult Edit(AddRecipeViewModel model)
        // [HttpGet] public IActionResult Details(int id) 
        // [HttpPost] public IActionResult Delete(int id)
        // [HttpGet] public IActionResult List() // My recipes
    }
}
