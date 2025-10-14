using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Recipe_type")]
    public class RecipeType : BaseModel
    {
        [PrimaryKey("recipe_type_id")]
        public int? recipe_type_id { get; set; }
        
        public string? content { get; set; }
        
        public DateTime created_at { get; set; }
    }
}

