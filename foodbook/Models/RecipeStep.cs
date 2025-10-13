using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace foodbook.Models
{
    [Table("RecipeStep")]
    public class RecipeStep : BaseModel
    {
        [PrimaryKey("step", shouldInsert: false)]
        public int step { get; set; }
        [PrimaryKey("recipe_id", shouldInsert: false)]
        public int recipe_id { get; set; }
        public string instruction { get; set; } = string.Empty;
        public int? media_id { get; set; }
    }
}


