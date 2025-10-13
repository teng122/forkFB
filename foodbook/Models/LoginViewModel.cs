using System.ComponentModel.DataAnnotations;

namespace foodbook.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Email hoặc Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại")]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}


