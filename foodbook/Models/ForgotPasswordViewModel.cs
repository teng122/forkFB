using System.ComponentModel.DataAnnotations;

namespace foodbook.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập username hoặc email")]
        [Display(Name = "Username hoặc Email")]
        public string UsernameOrEmail { get; set; } = string.Empty;
    }
}
