using System;
using System.Collections.Generic;

namespace foodbook.Models
{
    public class ProfileViewModel
    {
        public User? User { get; set; }

        public List<RecipeViewModel> Recipes { get; set; } = new List<RecipeViewModel>();

        public int RecipeCount { get; set; } = 0;
        public int FollowersCount { get; set; } = 0;
        public int FollowingCount { get; set; } = 0;
    }
}