using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace foodbook.Models
{
    [Table("Share")]
    public class Share : BaseModel
    {
        [Column("user_id")]
        public int user_id { get; set; }

        [Column("recipe_id")]
        public int recipe_id { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }
    }
}

