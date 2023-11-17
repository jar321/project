using System.Collections.Generic;
using Travelogram.Models; // Adjust this if your Post model is in a different namespace
using Microsoft.AspNetCore.Identity;  // Required for IdentityUser


namespace Travelogram.ViewModels
{
    public class UserProfileViewModel
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public required IList<Post> UserPosts { get; set; }
        // Add other fields as necessary (e.g., profile picture, bio, etc.)
    }
}
