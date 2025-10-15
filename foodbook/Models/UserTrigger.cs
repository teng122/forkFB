using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("User-Trigger")]
    public class UserTrigger : BaseModel
    {
        [Required]
        [Column("username")]
        public string username { get; set; } = string.Empty;
        
        [Column("full_name")]
        public string? full_name { get; set; }
        
        [Required]
        [Column("email")]
        public string email { get; set; } = string.Empty;
        
        [Required]
        [Column("password")]
        public string password { get; set; } = string.Empty;
        
        [Column("avatar_img")]
        public string? avatar_img { get; set; }  // URL từ Supabase Storage
        
        [Column("bio")]
        public string? bio { get; set; }
        
        [Column("created_at")]
        public DateTime created_at { get; set; }
        
        [Column("status")]
        public string status { get; set; } = "active";
        
        [Column("role")]
        public string role { get; set; } = "user";
       
        [Column("is_verified")]
        public bool? is_verified { get; set; } = false;
    }
}
