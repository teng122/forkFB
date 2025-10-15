using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Recipe_type")]
    public class RecipeType : BaseModel
    {
        [PrimaryKey("recipe_type_id", shouldInsert: false)]
        public int? recipe_type_id { get; set; }
        
        [Column("content")]
        public string? content { get; set; }
        
        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}

