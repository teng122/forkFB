namespace foodbook.Models
{
    public class NewfeedViewModel
    {
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string Description { get; set; }
        public string ThumbnailImg { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Level { get; set; }

        public string UserName { get; set; } = "Ẩn danh";
        public string UserAvatarUrl { get; set; }

        public int LikesCount { get; set; } = 0;
        public int CommentsCount { get; set; } = 0;
        public int SharesCount { get; set; } = 0;
        
        public List<string> Tags { get; set; } = new List<string>();
    }
}
