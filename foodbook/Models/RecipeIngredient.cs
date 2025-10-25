using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Recipe_Ingredient")]
    public class RecipeIngredient : BaseModel
    {
        [PrimaryKey("recipe_id", shouldInsert: true)]
        [Column("recipe_id")]
        public int recipe_id { get; set; }
        
        [PrimaryKey("ingredient_id", shouldInsert: true)]
        [Column("ingredient_id")]
        public int ingredient_id { get; set; }
        
        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}


