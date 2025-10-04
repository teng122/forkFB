using Supabase;
using Supabase.Gotrue;
using foodbook.Models;
using UserModel = foodbook.Models.User;

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
            var url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? configuration["Supabase:Url"];
            var anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? configuration["Supabase:AnonKey"];

            _client = new Supabase.Client(url, anonKey, options);
        }

        public Supabase.Client Client => _client;

        public async Task<bool> SignUpAsync(string email, string password, string fullName, string username)
        {
            try
            {
                // 1. Chuyển username thành lowercase
                var lowercaseUsername = username.ToLower();
                
                // 2. Validate trước khi gửi lên Supabase Auth
                await ValidateUserDataAsync(email, lowercaseUsername);
                
                // 3. Tạo user trong Supabase Auth để lấy access token
                var authResponse = await _client.Auth.SignUp(email, password);
                
                if (authResponse != null)
                {
                    // 4. Thêm vào bảng User-Trigger
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
                
                return false;
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
                    .From<UserForValidation>()
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
                    .From<UserForValidation>()
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
                // 1. Đăng nhập qua Supabase Auth để lấy access token
                var authResponse = await _client.Auth.SignIn(email, password);
                
                if (authResponse != null)
                {
                    // 2. Query thông tin user từ bảng User custom
                    var userResult = await _client
                        .From<UserModel>()
                        .Where(x => x.email == email)
                        .Single();

                    // 3. Trả về thông tin user + access token
                    return new
                    {
                        user = userResult,
                        access_token = authResponse.AccessToken,
                        session = authResponse
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
            await _client.Auth.SignOut();
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                await _client.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Gửi email reset mật khẩu thất bại: {ex.Message}");
            }
        }

        public async Task<bool> UpdatePasswordAsync(string newPassword)
        {
            try
            {
                // For now, just return true - this would need proper implementation
                // based on Supabase documentation for password updates
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cập nhật mật khẩu thất bại: {ex.Message}");
            }
        }

        public Supabase.Gotrue.User? GetCurrentUser()
        {
            return _client.Auth.CurrentUser;
        }

        public async Task SetSessionAsync(string accessToken, string refreshToken)
        {
            try
            {
                await _client.Auth.SetSession(accessToken, refreshToken);
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

        public async Task<UserModel?> LoginFromUserTableAsync(string emailOrPhone, string password)
        {
            try
            {
                // Chuyển input thành lowercase để so sánh
                var lowercaseInput = emailOrPhone.ToLower();
                
                // Query user từ bảng User custom
                var userResult = await _client
                    .From<UserModel>()
                    .Where(x => x.email == lowercaseInput || x.username == lowercaseInput)
                    .Single();

                // Kiểm tra password (có thể hash password nếu cần)
                if (userResult != null && userResult.password == password)
                {
                    return userResult;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Đăng nhập thất bại: {ex.Message}");
            }
        }

    }
}
