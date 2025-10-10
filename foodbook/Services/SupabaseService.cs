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
                    Console.WriteLine("Login successful!");
                    return userResult;
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

    }
}
