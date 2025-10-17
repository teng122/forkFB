using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Comment")]
    public class Comment : BaseModel
    {
        [PrimaryKey("comment_id", shouldInsert: false)]
        public int comment_id { get; set; }

        [Required]
        [Column("user_id")]
        public int user_id { get; set; }

        [Required]
        [Column("recipe_id")]
        public int recipe_id { get; set; }

        [Column("body")]
        public string? body { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
