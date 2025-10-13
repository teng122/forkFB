using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace foodbook.Models
{
    [Table("Recipe")]
    public class Recipe : BaseModel
    {
        [PrimaryKey("recipe_id")]
        public int? recipe_id { get; set; }
        public int user_id { get; set; }
        public int? recipe_type_id { get; set; }
        public string? name { get; set; }
        public byte[]? thumbnail_img { get; set; }
        public int? step_number { get; set; }
        public string? description { get; set; }
        public int? cook_time { get; set; }
        public DateTime? created_at { get; set; }
        public string? level { get; set; }
    }
}


