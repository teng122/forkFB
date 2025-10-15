using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace foodbook.Models
{
    [Table("Media")]
    public class Media : BaseModel
    {
        [PrimaryKey("media_id", shouldInsert: false)]
        public int? media_id { get; set; }
        
        [Column("media_img")]
        public string? media_img { get; set; }  // URL từ Supabase Storage
        
        [Column("media_video")]
        public string? media_video { get; set; }  // URL từ Supabase Storage
    }
}

