using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("User_Report")]
    public class UserReport : BaseModel
    {
        [PrimaryKey("reporter_id", shouldInsert: true)]
        public int reporter_id { get; set; }

        [PrimaryKey("reported_user_id", shouldInsert: true)]
        public int reported_user_id { get; set; }

        [Column("body")]
        public string? body { get; set; }

        [Column("status")]
        public string status { get; set; } = "Đang xử lý";

        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}

