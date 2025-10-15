using Supabase;
using foodbook.Models;
using UserModel = foodbook.Models.UserTrigger;

namespace foodbook.Services
{
    public class SupabaseService
    {
        private readonly Supabase.Client _client;

        public SupabaseService(IConfiguration configuration)
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            // Use environment variables for production, fallback to configuration
            var url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? configuration["Supabase:Url"] ?? "";
            var anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? configuration["Supabase:AnonKey"] ?? "";

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(anonKey))
            {
                throw new InvalidOperationException("Supabase URL and AnonKey must be configured");
            }

            _client = new Supabase.Client(url, anonKey, options);
        }

        public Supabase.Client Client => _client;

        public async Task<bool> SignUpAsync(string email, string password, string fullName, string username)
        {
            try
            {
                // 1. Chuyển username thành lowercase
                var lowercaseUsername = username.ToLower();
                
                // 2. Validate trước khi tạo user
                await ValidateUserDataAsync(email, lowercaseUsername);
                
                // 3. Thêm vào bảng User-Trigger (không dùng Supabase Auth)
                var userData = new UserModel
                {
                    username = lowercaseUsername,
                    full_name = fullName,
                    email = email,
                    password = password,
                    created_at = DateTime.UtcNow,
                    status = "active"
                };
                
                // Sử dụng PostgREST để insert
                var response = await _client
                    .From<UserModel>()
                    .Insert(userData);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Đăng ký thất bại: {ex.Message}");
            }
        }

        private async Task ValidateUserDataAsync(string email, string username)
        {
            try
            {
                // Kiểm tra username đã tồn tại chưa từ bảng User (không phải User-Trigger)
                var existingUser = await _client
                    .From<foodbook.Models.User>()
                    .Where(x => x.username == username)
                    .Single();

                if (existingUser != null)
                {
                    throw new Exception("Tên đăng nhập đã được sử dụng. Vui lòng chọn tên khác.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Tên đăng nhập đã được sử dụng"))
                {
                    throw;
                }
                // Nếu không tìm thấy user thì tiếp tục
            }

            try
            {
                // Kiểm tra email đã tồn tại chưa từ bảng User (không phải User-Trigger)
                var existingEmail = await _client
                    .From<foodbook.Models.User>()
                    .Where(x => x.email == email)
                    .Single();

                if (existingEmail != null)
                {
                    throw new Exception("Email đã được sử dụng. Vui lòng chọn email khác.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Email đã được sử dụng"))
                {
                    throw;
                }
                // Nếu không tìm thấy email thì tiếp tục
            }
        }

        public async Task<dynamic?> SignInAsync(string email, string password)
        {
            try
            {
                // Query thông tin user từ bảng User (không dùng Supabase Auth)
                var userResult = await _client
                    .From<foodbook.Models.User>()
                    .Where(x => x.email == email || x.username == email)
                    .Single();

                // Kiểm tra password
                if (userResult != null && userResult.password == password)
                {
                    return new
                    {
                        user = userResult,
                        access_token = "dummy_token", // Không dùng Supabase Auth
                        session = (object?)null
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Đăng nhập thất bại: {ex.Message}");
            }
        }

        public async Task SignOutAsync()
        {
            // Không cần gọi Supabase Auth SignOut
            await Task.CompletedTask;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                // Không dùng Supabase Auth, chỉ trả về true
                // Logic reset password sẽ được xử lý bởi EmailService
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Gửi email reset mật khẩu thất bại: {ex.Message}");
            }
        }


        public object? GetCurrentUser()
        {
            // Không dùng Supabase Auth
            return null;
        }

        public async Task SetSessionAsync(string accessToken, string refreshToken)
        {
            try
            {
                // Không dùng Supabase Auth
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể thiết lập session: {ex.Message}");
            }
        }

        public async Task<UserModel?> GetUserByEmailAsync(string email)
        {
            try
            {
                // Tìm trong bảng User-Trigger trước
                var userResult = await _client
                    .From<UserModel>()
                    .Where(x => x.email == email)
                    .Single();

                return userResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể lấy thông tin user: {ex.Message}");
            }
        }


        public async Task<foodbook.Models.User?> LoginFromUserTableAsync(string emailOrPhone, string password)
        {
            try
            {
                // Chuyển input thành lowercase để so sánh
                var lowercaseInput = emailOrPhone.ToLower();
                Console.WriteLine($"Searching for user in User table: {lowercaseInput}");
                
                // Query user từ bảng User (không phải User-Trigger)
                var userResult = await _client
                    .From<foodbook.Models.User>()
                    .Where(x => x.email == lowercaseInput || x.username == lowercaseInput)
                    .Single();

                Console.WriteLine($"Found user: {userResult?.username}, Email: {userResult?.email}");
                Console.WriteLine($"Stored password: {userResult?.password}");
                Console.WriteLine($"Input password: {password}");
                Console.WriteLine($"Password match: {userResult?.password == password}");

                // Kiểm tra password (so sánh trực tiếp vì password được lưu plain text)
                if (userResult != null && userResult.password == password)
                {
                    // Kiểm tra email đã được xác thực chưa
                    if (userResult.is_verified == true)
                    {
                        Console.WriteLine("Login successful! Email verified.");
                        return userResult;
                    }
                    else
                    {
                        Console.WriteLine("Login failed: Email not verified");
                        throw new Exception("Tài khoản chưa được xác thực email. Vui lòng kiểm tra email và xác thực trước khi đăng nhập.");
                    }
                }

                Console.WriteLine("Login failed: Password mismatch");
                return null;
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Đăng nhập thất bại: {ex.Message}");
            }
        }

        // Tìm user theo email hoặc username từ bảng User (cho forgot password)
        public async Task<foodbook.Models.User?> GetUserByEmailOrUsernameAsync(string emailOrUsername)
        {
            try
            {
                var lowercaseInput = emailOrUsername.ToLower();
                
                // Tìm user từ bảng User
                var userResult = await _client
                    .From<foodbook.Models.User>()
                    .Where(x => x.email == lowercaseInput || x.username == lowercaseInput)
                    .Single();

                return userResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserByEmailOrUsername error: {ex.Message}");
                return null;
            }
        }

        // Lưu token reset password vào memory
        public async Task SavePasswordResetTokenAsync(string email, string token)
        {
            try
            {
                var resetData = new
                {
                    email = email.ToLower(),
                    token = token,
                    created_at = DateTime.UtcNow,
                    expires_at = DateTime.UtcNow.AddHours(1) // Token hết hạn sau 1 giờ
                };

                // Lưu vào static dictionary
                PasswordResetTokens[email.ToLower()] = resetData;
                
                Console.WriteLine($"Saved reset token for email: {email}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể lưu token reset password: {ex.Message}");
            }
        }

        // Kiểm tra token reset password có hợp lệ không
        public async Task<bool> ValidatePasswordResetTokenAsync(string email, string token)
        {
            try
            {
                var lowercaseEmail = email.ToLower();
                
                if (PasswordResetTokens.ContainsKey(lowercaseEmail))
                {
                    var resetData = PasswordResetTokens[lowercaseEmail];
                    
                    if (resetData.token == token && resetData.expires_at > DateTime.UtcNow)
                    {
                        return true;
                    }
                    else if (resetData.expires_at <= DateTime.UtcNow)
                    {
                        // Token hết hạn, xóa khỏi cache
                        PasswordResetTokens.Remove(lowercaseEmail);
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ValidatePasswordResetToken error: {ex.Message}");
                return false;
            }
        }

        // Tìm user trong bảng User-Trigger để reset password
        public async Task<foodbook.Models.UserTrigger?> GetUserTriggerByEmailAsync(string email)
        {
            try
            {
                var lowercaseEmail = email.ToLower();
                
                // Tìm user từ bảng User-Trigger
                var userResult = await _client
                    .From<foodbook.Models.UserTrigger>()
                    .Where(x => x.email == lowercaseEmail)
                    .Single();

                return userResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserTriggerByEmail error: {ex.Message}");
                return null;
            }
        }

        // Đặt lại mật khẩu mới trong bảng User-Trigger (trigger sẽ tự động cập nhật bảng User)
        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            try
            {
                var lowercaseEmail = email.ToLower();
                
                // Cập nhật password trong bảng User-Trigger cho TẤT CẢ các hàng có cùng email
                // Trigger sẽ tự động cập nhật password trong bảng User cho tất cả hàng có cùng email/username
                var updateResult = await _client
                    .From<foodbook.Models.UserTrigger>()
                    .Where(x => x.email == lowercaseEmail)
                    .Set(x => x.password, newPassword)
                    .Update();

                Console.WriteLine($"Reset password for email: {email} in User-Trigger table - Updated {updateResult.Models.Count} rows");
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể đặt lại mật khẩu: {ex.Message}");
            }
        }

        // Xóa token reset password
        public async Task RemovePasswordResetTokenAsync(string email)
        {
            try
            {
                var lowercaseEmail = email.ToLower();
                
                if (PasswordResetTokens.ContainsKey(lowercaseEmail))
                {
                    PasswordResetTokens.Remove(lowercaseEmail);
                }
                
                Console.WriteLine($"Removed reset token for email: {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RemovePasswordResetToken error: {ex.Message}");
            }
        }

        // Static dictionary để lưu token tạm thời
        private static Dictionary<string, dynamic> PasswordResetTokens = new Dictionary<string, dynamic>();

    }
}
