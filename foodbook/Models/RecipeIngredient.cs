using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Recipe_Ingredient")]
    public class RecipeIngredient : BaseModel
    {
        [Required]
        [Column("recipe_id")]
        public int recipe_id { get; set; }
        
        [Required]
        [Column("ingredient_id")]
        public int ingredient_id { get; set; }
        
        [Column("quantity")]
        public string? quantity { get; set; }
        
        [Column("unit")]
        public string? unit { get; set; }
        
        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}


