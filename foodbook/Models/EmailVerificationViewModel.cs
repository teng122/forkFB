using System.ComponentModel.DataAnnotations;

namespace foodbook.Models
{
    public class EmailVerificationViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

