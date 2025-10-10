using Microsoft.AspNetCore.Mvc;
using foodbook.Models;
using foodbook.Services;
using Supabase.Gotrue;
using Microsoft.AspNetCore.Http;
using foodbook.Helpers;

namespace foodbook.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupabaseService _supabaseService;
        private readonly EmailService _emailService;

        public AccountController(SupabaseService supabaseService, EmailService emailService)
        {
            _supabaseService = supabaseService;
            _emailService = emailService;
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
                // Đăng nhập trực tiếp từ bảng User (có hỗ trợ admin)
                Console.WriteLine($"Attempting login for: {model.EmailOrPhone}");
                var user = await _supabaseService.LoginFromUserTableAsync(model.EmailOrPhone, model.Password);
                if (user != null)
                {
                    Console.WriteLine($"Login successful for user: {user.username}");
                    // Store user session với thông tin từ bảng User
                    HttpContext.Session.SetString("user_id", user.username);
                    HttpContext.Session.SetString("user_email", user.email);
                    HttpContext.Session.SetString("username", user.username);
                    HttpContext.Session.SetString("full_name", user.full_name ?? "");
                    HttpContext.Session.SetString("role", user.role ?? "user"); // Lưu role để kiểm tra admin
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("Login failed: User not found or password incorrect");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login exception: {ex.Message}");
                // Kiểm tra nếu lỗi là do email chưa xác thực
                if (ex.Message.Contains("chưa được xác thực email"))
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập/Email hoặc mật khẩu không đúng.");
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
                    // Gửi email xác thực bằng EmailService
                    try
                    {
                        var verificationLink = $"{Request.Scheme}://{Request.Host}/Account/VerifyEmail?email={model.Email}";
                        await _emailService.SendEmailVerificationAsync(model.Email, model.Username, verificationLink);
                    }
                    catch (Exception emailEx)
                    {
                        // Log lỗi email nhưng không làm fail đăng ký
                        Console.WriteLine($"Không thể gửi email xác thực: {emailEx.Message}");
                    }
                    
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
                // Tạo link reset password (có thể tạo token và lưu vào database)
                var resetLink = $"{Request.Scheme}://{Request.Host}/Account/ResetPassword?token=temp_token&email={model.UsernameOrEmail}";
                
                // Gửi email reset password
                await _emailService.SendPasswordResetEmailAsync(model.UsernameOrEmail, resetLink);
                
                TempData["SuccessMessage"] = "Chúng tôi đã gửi liên kết đặt lại mật khẩu đến email của bạn.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("ForgotPassword");
        }



        [HttpGet]
        public IActionResult VerifySuccess()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Login");
            }
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string email)
        {
            var model = new EmailVerificationViewModel
            {
                Email = email ?? "",

                IsSuccess = false,
                Message = ""
            };

            try
            {
                if (string.IsNullOrEmpty(email) )
                {
                    model.Message = "Link xác thực không hợp lệ.";
                    return View(model);
                }

                // Tìm tất cả users có email này trong bảng User-Trigger
                if (!string.IsNullOrEmpty(email))
                {
                    // Chuyển email thành lowercase để so sánh
                    var lowercaseEmail = email.ToLower();
                    Console.WriteLine($"Searching for email: {lowercaseEmail}");
                    
                    // Tìm tất cả users có email này (có thể có nhiều hàng)
                    var users = await _supabaseService.Client
                        .From<foodbook.Models.UserTrigger>()
                        .Where(x => x.email == lowercaseEmail)
                        .Get();
                        
                    Console.WriteLine($"Found {users.Models.Count} users with email: {lowercaseEmail}");
                    
                    if (users.Models.Count == 0)
                    {
                        model.Message = "Không tìm thấy tài khoản với email này.";
                        return View(model);
                    }

                    // Cập nhật is_verified = true cho TẤT CẢ users có email này
                    var updateResult = await _supabaseService.Client
                        .From<foodbook.Models.UserTrigger>()
                        .Where(x => x.email == lowercaseEmail)
                        .Set(x => x.is_verified, true)
                        .Update();
                        
                    Console.WriteLine($"Updated {updateResult.Models.Count} users");
                }

                model.IsSuccess = true;
                model.Message = "Email đã được xác thực thành công! Bạn có thể đăng nhập vào tài khoản.";
            }
            catch (Exception ex)
            {
                model.Message = $"Lỗi xác thực email: {ex.Message}";
            }

            return View(model);
        }

    }
}


