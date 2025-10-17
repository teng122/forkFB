using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("RecipeStep_Media")]
    public class RecipeStepMedia : BaseModel
    {
        [Required]
        [Column("recipe_id")]
        public int recipe_id { get; set; }

        [Required]
        [Column("step")]
        public int step { get; set; }

        [Required]
        [Column("media_id")]
        public int media_id { get; set; }

        [Column("display_order")]
        public int display_order { get; set; } = 1;
    }
}

