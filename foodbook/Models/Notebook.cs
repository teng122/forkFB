using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Notebook")]
    public class Notebook : BaseModel
    {
        [PrimaryKey("user_id", shouldInsert: false)]
        public int user_id { get; set; }

        [PrimaryKey("recipe_id", shouldInsert: false)]
        public int recipe_id { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
