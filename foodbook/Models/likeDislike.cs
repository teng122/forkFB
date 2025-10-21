using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace foodbook.Models
{
    [Table("like_dislike")]
    public class likeDislike : BaseModel
    {
        [PrimaryKey("ld_id", false)]
        public int ld_id { get; set; }

        [Column("recipe_id")]
        public int recipe_id { get; set; }

        [Column("user_id")]
        public int user_id { get; set; }

        [Column("body")]
        public string body { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }
    }
}

