using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("User-Trigger")]
    public class User : BaseModel
    {
        [Required]
        public string username { get; set; } = string.Empty;
        
        public string? full_name { get; set; }
        
        [Required]
        public string email { get; set; } = string.Empty;
        
        [Required]
        public string password { get; set; } = string.Empty;
        
        public byte[]? avatar_img { get; set; }
        
        public string? bio { get; set; }
        
        public DateTime created_at { get; set; }
        
        public string status { get; set; } = "active";
    }
}
