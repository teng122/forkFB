using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("User-Trigger")]
    public class UserTrigger : BaseModel
    {
        [Required]
        public string username { get; set; } = string.Empty;
        
        public string? full_name { get; set; }
        
        [Required]
        public string email { get; set; } = string.Empty;
        
        [Required]
        [Column("password")]
        public string password { get; set; } = string.Empty;
        
        public byte[]? avatar_img { get; set; }
        
        public string? bio { get; set; }
        
        public DateTime created_at { get; set; }
        
        public string status { get; set; } = "active";
        
        public string role { get; set; } = "user";
       
        [Column("is_verified")]
        public bool? is_verified { get; set; } = false;
    }
}
