using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Ingredient_Master")]
    public class IngredientMaster : BaseModel
    {
        [PrimaryKey("ingredient_id", shouldInsert: false)]
        public int? ingredient_id { get; set; }
        
        [Required]
        [Column("name")]
        public string name { get; set; } = string.Empty;
        
        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}


