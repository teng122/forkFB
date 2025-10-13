using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace foodbook.Models
{
    [Table("Ingredient")]
    public class Ingredient : BaseModel
    {
        [PrimaryKey("ingredient_id")]
        public int? ingredient_id { get; set; }
        public int recipe_id { get; set; }
        public string? name { get; set; }
        public DateTime? created_at { get; set; }
    }
}


