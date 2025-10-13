using System.ComponentModel.DataAnnotations;

namespace foodbook.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Tên đăng nhập hoặc Email")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email")]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}


