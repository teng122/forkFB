using Microsoft.AspNetCore.Mvc;
using foodbook.Models;
using foodbook.Services;
using Supabase.Gotrue;

namespace foodbook.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupabaseService _supabaseService;

        public AccountController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Đăng nhập trực tiếp từ bảng User
                var user = await _supabaseService.LoginFromUserTableAsync(model.EmailOrPhone, model.Password);
                if (user != null)
                {
                    // Store user session
                    HttpContext.Session.SetString("user_id", user.username);
                    HttpContext.Session.SetString("user_email", user.email);
                    HttpContext.Session.SetString("username", user.username);
                    HttpContext.Session.SetString("full_name", user.full_name ?? "");
                    
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email/Số điện thoại hoặc mật khẩu không đúng.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _supabaseService.SignUpAsync(model.Email, model.Password, model.FullName, model.Username);
                if (success)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _supabaseService.ResetPasswordAsync(model.UsernameOrEmail);
                TempData["SuccessMessage"] = "Chúng tôi đã gửi liên kết đặt lại mật khẩu đến email của bạn.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("ForgotPassword");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _supabaseService.UpdatePasswordAsync(model.NewPassword);
                TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("ChangePassword");
        }

        [HttpGet]
        public async Task<IActionResult> AuthCallback(string access_token, string refresh_token, string expires_in, string token_type, string type)
        {
            try
            {
                if (type == "signup" && !string.IsNullOrEmpty(access_token))
                {
                    // Set session với Supabase Auth
                    await _supabaseService.SetSessionAsync(access_token, refresh_token);
                    
                    // Lấy thông tin user từ Supabase Auth
                    var currentUser = _supabaseService.GetCurrentUser();
                    if (currentUser != null)
                    {
                        // Query thông tin user từ bảng User custom
                        var userResult = await _supabaseService.GetUserByEmailAsync(currentUser.Email);
                        if (userResult != null)
                        {
                            // Store user session
                            HttpContext.Session.SetString("user_id", userResult.username);
                            HttpContext.Session.SetString("user_email", userResult.email);
                            HttpContext.Session.SetString("username", userResult.username);
                            HttpContext.Session.SetString("full_name", userResult.full_name ?? "");
                            HttpContext.Session.SetString("access_token", access_token);
                            
                            TempData["SuccessMessage"] = "Đăng ký thành công! Chào mừng bạn đến với Foodbook!";
                            return RedirectToAction("VerifySuccess");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Xác thực thất bại: {ex.Message}";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult VerifySuccess()
        {
            return View();
        }

    }
}


