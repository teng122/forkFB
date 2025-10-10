using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace foodbook.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(
                    _configuration["EmailSettings:FromName"] ?? "Foodbook",
                    _configuration["EmailSettings:FromEmail"] ?? "huhume147@gmail.com"));
                email.To.Add(new MailboxAddress("", toEmail));
                email.Subject = subject;
                email.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com", 
                    int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"), 
                    false);
                
                await client.AuthenticateAsync(
                    _configuration["EmailSettings:SmtpUsername"] ?? "huhume147@gmail.com", 
                    _configuration["EmailSettings:SmtpPassword"] ?? "");
                
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể gửi email: {ex.Message}");
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string username)
        {
            var subject = "Chào mừng bạn đến với Foodbook!";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #28a745;'>Chào mừng bạn đến với Foodbook!</h2>
                    <p>Xin chào <strong>{username}</strong>,</p>
                    <p>Cảm ơn bạn đã đăng ký tài khoản tại Foodbook. Chúng tôi rất vui được chào đón bạn!</p>
                    <p>Bây giờ bạn có thể:</p>
                    <ul>
                        <li>Khám phá các công thức nấu ăn thú vị</li>
                        <li>Chia sẻ công thức của riêng bạn</li>
                        <li>Kết nối với cộng đồng yêu thích nấu ăn</li>
                    </ul>
                    <p>Chúc bạn có những trải nghiệm tuyệt vời với Foodbook!</p>
                    <br>
                    <p>Trân trọng,<br>Đội ngũ Foodbook</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Đặt lại mật khẩu - Foodbook";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #dc3545;'>Đặt lại mật khẩu</h2>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Foodbook của mình.</p>
                    <p>Vui lòng nhấp vào liên kết bên dưới để đặt lại mật khẩu:</p>
                    <p><a href='{resetLink}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Đặt lại mật khẩu</a></p>
                    <p>Liên kết này sẽ hết hạn sau 24 giờ.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <br>
                    <p>Trân trọng,<br>Đội ngũ Foodbook</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailVerificationAsync(string toEmail, string username, string verificationLink)
        {
            var subject = "Xác thực email - Foodbook";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #28a745;'>Xác thực email của bạn</h2>
                    <p>Xin chào <strong>{username}</strong>,</p>
                    <p>Cảm ơn bạn đã đăng ký tài khoản tại Foodbook!</p>
                    <p>Để hoàn tất quá trình đăng ký, vui lòng xác thực email của bạn bằng cách nhấp vào nút bên dưới:</p>
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{verificationLink}' style='background-color: #28a745; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Xác thực Email</a>
                    </p>
                    <p>Hoặc copy và paste link này vào trình duyệt:</p>
                    <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 5px;'>{verificationLink}</p>
                    <p><strong>Lưu ý:</strong> Link này sẽ hết hạn sau 24 giờ.</p>
                    <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
                    <br>
                    <p>Trân trọng,<br>Đội ngũ Foodbook</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
