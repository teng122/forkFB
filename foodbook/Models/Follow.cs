using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace foodbook.Models
{
    [Table("Follow")]
    public class Follow : BaseModel
    {
        [Column("follower_id")]
        public int follower_id { get; set; }

        [Column("following_id")]
        public int following_id { get; set; }
    }
}
