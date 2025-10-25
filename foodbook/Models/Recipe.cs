using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Recipe")]
    public class Recipe : BaseModel
    {
        [PrimaryKey("recipe_id", shouldInsert: false)]
        public int? recipe_id { get; set; }
        
        [Required]
        [Column("user_id")]
        public int user_id { get; set; }
        
        [Required]
        [Column("name")]
        public string name { get; set; } = string.Empty;
        
        [Column("thumbnail_img")]
        public string? thumbnail_img { get; set; }  // URL từ Supabase Storage
        
        [Column("step_number")]
        public int? step_number { get; set; }
        
        [Column("description")]
        public string? description { get; set; }
        
        [Column("cook_time")]
        public int? cook_time { get; set; }
        
        [Column("created_at")]
        public DateTime created_at { get; set; }
        
        [Column("level")]
        public string level { get; set; } = "dễ"; // dễ, trung bình, khó
        
        [Column("status")]
        public string status { get; set; } = "active"; // active, banned, pending, deleted
    }
}

