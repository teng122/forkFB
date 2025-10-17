using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Report")]
    public class Report : BaseModel
    {
        [PrimaryKey("user_id", shouldInsert: false)]
        public int user_id { get; set; }

        [PrimaryKey("recipe_id", shouldInsert: false)]
        public int recipe_id { get; set; }

        [Column("body")]
        public string? body { get; set; }

        [Column("status")]
        public string status { get; set; } = "Đang xử lý";

        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
