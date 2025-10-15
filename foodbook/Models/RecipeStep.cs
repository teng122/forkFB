using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("RecipeStep")]
    public class RecipeStep : BaseModel
    {
        [Required]
        [Column("recipe_id")]
        public int recipe_id { get; set; }
        
        [Required]
        [Column("instruction")]
        public string instruction { get; set; } = string.Empty;
        
        [Required]
        [Column("step")]
        public int step { get; set; }
        
        // Không còn media_id - giờ dùng bảng trung gian RecipeStep_Media
    }
}

