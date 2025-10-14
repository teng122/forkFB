using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

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

