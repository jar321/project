using System;
using System.Collections.Generic;  // Required for List<Comment>
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;  // Required for IdentityUser

namespace Travelogram.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string? Heading { get; set; }
        public string? Content { get; set; }
        [DisplayName("Author Name")]
        public string? Fname { get; set; }
        public string? FileName { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int? Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        // New properties to associate the post with a user
        public string? UserId { get; set; }  // Foreign key property
        public IdentityUser? User { get; set; }  // Navigation property
    }
}
