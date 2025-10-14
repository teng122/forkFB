using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Ingredient")]
    public class Ingredient : BaseModel
    {
        [PrimaryKey("ingredient_id")]
        public int? ingredient_id { get; set; }
        
        [Required]
        public int recipe_id { get; set; }
        
        public string? name { get; set; }
        
        public DateTime created_at { get; set; }
    }
}

