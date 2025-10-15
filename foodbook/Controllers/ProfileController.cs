using foodbook.Models;
using foodbook.Services;
using Microsoft.AspNetCore.Mvc;
using static Supabase.Postgrest.Constants;

namespace foodbook.Controllers
{
    public class ProfileController : Controller
    {
        private readonly SupabaseService _supabaseService;
        private readonly StorageService _storageService;

        public ProfileController(SupabaseService supabaseService, StorageService storageService)
        {
            _supabaseService = supabaseService;
            _storageService = storageService;
        }
       
        // GET: /Profile hoặc /Profile?id=123
        public async Task<IActionResult> Index(int? id)
        {
            try
            {
                var sessionEmail = HttpContext.Session.GetString("user_email");

                // Nếu không có ID → Xem profile của chính mình
                if (!id.HasValue)
                {
                    if (string.IsNullOrEmpty(sessionEmail))
                    {
                        TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem hồ sơ.";
                        return RedirectToAction("Login", "Auth");
                    }

                    // Lấy user theo email trực tiếp từ bảng User
                    var currentUserResp = await _supabaseService.Client
                        .From<User>()
                        .Filter("email", Operator.Equals, sessionEmail)
                        .Single();

                    if (currentUserResp == null)
                    {
                        return View("~/Views/Account/Profile.cshtml", new ProfileViewModel());
                    }

                    id = currentUserResp.user_id;
                }

                // Lấy user theo user_id
                var userResp = await _supabaseService.Client
                    .From<User>()
                    .Filter("user_id", Operator.Equals, id.Value)
                    .Single();

                if (userResp == null)
                    return View("~/Views/Account/Profile.cshtml", new ProfileViewModel());

                // Lấy các recipe của user
                var recipesResp = await _supabaseService.Client
                    .From<Recipe>()
                    .Filter("user_id", Operator.Equals, userResp.user_id)
                    .Order("created_at", Ordering.Descending)
                    .Get();

                var recipeModels = recipesResp.Models ?? new List<Recipe>();

                // Followers / Following
                var followersResp = await _supabaseService.Client
                    .From<Follow>()
                    .Filter("following_id", Operator.Equals, userResp.user_id)
                    .Get();

                var followingResp = await _supabaseService.Client
                    .From<Follow>()
                    .Filter("follower_id", Operator.Equals, userResp.user_id)
                    .Get();

                int followersCount = followersResp?.Models?.Count ?? 0;
                int followingCount = followingResp?.Models?.Count ?? 0;

                // Map sang RecipeViewModel
                var recipeVMs = new List<RecipeViewModel>();
                foreach (var r in recipeModels)
                {
                    var likesResp = await _supabaseService.Client
                        .From<likeDislike>()
                        .Filter("recipe_id", Operator.Equals, r.recipe_id)
                        .Get();

                    int likes = likesResp?.Models?.Count ?? 0;

                    recipeVMs.Add(new RecipeViewModel
                    {
                        RecipeId = r.recipe_id ?? 0,
                        Name = r.name,
                        ThumbnailImg = r.thumbnail_img,
                        CookTime = r.cook_time ?? 0,
                        Likes = likes,
                        CreatedAt = r.created_at,
                        Level = r.level ?? "dễ"
                    });
                }

                var viewModel = new ProfileViewModel
                {
                    User = userResp,
                    Recipes = recipeVMs,
                    RecipeCount = recipeVMs.Count,
                    FollowersCount = followersCount,
                    FollowingCount = followingCount
                };

                return View("~/Views/Account/Profile.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải hồ sơ: " + ex.Message;
                return View("~/Views/Account/Profile.cshtml", new ProfileViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sessionEmail = HttpContext.Session.GetString("user_email");
            if (string.IsNullOrEmpty(sessionEmail))
                return RedirectToAction("Login", "Auth");

            var user = await _supabaseService.Client
                .From<User>()
                .Filter("user_id", Operator.Equals, id)
                .Single();

            if (user == null)
                return NotFound();

            if (!string.Equals(user.email, sessionEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa hồ sơ này.";
                return RedirectToAction("Index", new { id = id });
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model, IFormFile? avatar)
        {
            try
            {
                var sessionEmail = HttpContext.Session.GetString("user_email");
                if (string.IsNullOrEmpty(sessionEmail))
                    return RedirectToAction("Login", "Auth");

                if (!string.Equals(model.email, sessionEmail, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền cập nhật hồ sơ này.";
                    return RedirectToAction("Index", new { id = model.user_id });
                }

                // Lấy user hiện tại để tránh ghi đè các trường nhạy cảm (password, email, username)
                var existingUser = await _supabaseService.Client
                    .From<User>()
                    .Filter("user_id", Operator.Equals, model.user_id)
                    .Single();

                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index", new { id = model.user_id });
                }

                // Upload avatar nếu có
                string? avatarUrl = existingUser.avatar_img;
                if (avatar != null && avatar.Length > 0)
                {
                    // Chỉ cho phép ảnh, lưu vào bucket 'img'
                    avatarUrl = await _storageService.UploadFileAsync(avatar, isVideo: false, folderPath: "avatars");
                }

                // Cập nhật chỉ các trường cho phép
                var updateResult = await _supabaseService.Client
                    .From<User>()
                    .Filter("user_id", Operator.Equals, model.user_id)
                    .Set(x => x.full_name, model.full_name)
                    .Set(x => x.bio, model.bio)
                    .Set(x => x.avatar_img, avatarUrl)
                    .Update();

                // Cập nhật lại session avatar để navbar thay đổi ngay
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    HttpContext.Session.SetString("avatar_img", avatarUrl);
                }

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công";
                return RedirectToAction("Index", new { id = model.user_id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Cập nhật thất bại: " + ex.Message;
                return View(model);
            }
        }
    }
}