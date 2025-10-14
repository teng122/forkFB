using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Recipe")]
    public class Recipe : BaseModel
    {
        [PrimaryKey("recipe_id")]
        public int? recipe_id { get; set; }
        
        [Required]
        public int user_id { get; set; }
        
        public int? recipe_type_id { get; set; }
        
        [Required]
        public string name { get; set; } = string.Empty;
        
        public byte[]? thumbnail_img { get; set; }
        
        public int? step_number { get; set; }
        
        public string? description { get; set; }
        
        public int? cook_time { get; set; }
        
        public DateTime created_at { get; set; }
        
        public string level { get; set; } = "dễ"; // dễ, trung bình, khó
    }
}

