using foodbook.Models;

namespace foodbook.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalRecipes { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public List<User> RecentUsers { get; set; } = new List<User>();
        public List<Recipe> RecentRecipes { get; set; } = new List<Recipe>();
        public List<Report> FlaggedContent { get; set; } = new List<Report>();
        public List<RecipeType> Categories { get; set; } = new List<RecipeType>();
        public List<IngredientMaster> Ingredients { get; set; } = new List<IngredientMaster>();
    }
}
