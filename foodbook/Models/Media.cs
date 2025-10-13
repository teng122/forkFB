using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace foodbook.Models
{
    [Table("Media")]
    public class Media : BaseModel
    {
        [PrimaryKey("media_id")]
        public int? media_id { get; set; }
        public byte[]? media_img { get; set; }
        public byte[]? media_video { get; set; }
    }
}


