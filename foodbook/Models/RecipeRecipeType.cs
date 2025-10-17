using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Recipe_RecipeType")]
    public class RecipeRecipeType : BaseModel
    {
        [Required]
        [Column("recipe_id")]
        public int recipe_id { get; set; }

        [Required]
        [Column("recipe_type_id")]
        public int recipe_type_id { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
