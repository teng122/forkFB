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
        public bool IsFollowing { get; set; } = false;

        // For Followers and Following pages
        public List<FollowerViewModel> Followers { get; set; } = new List<FollowerViewModel>();
        public List<FollowerViewModel> Following { get; set; } = new List<FollowerViewModel>();
    }

    public class FollowerViewModel
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public int? RecipeCount { get; set; }
        public int? FollowersCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}