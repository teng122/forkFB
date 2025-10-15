using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Ingredient")]
    public class Ingredient : BaseModel
    {
        [PrimaryKey("ingredient_id", shouldInsert: false)]
        public int? ingredient_id { get; set; }
        
        [Required]
        [Column("recipe_id")]
        public int recipe_id { get; set; }
        
        [Column("name")]
        public string? name { get; set; }
        
        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}

