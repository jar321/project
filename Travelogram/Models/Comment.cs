using System;
using Microsoft.AspNetCore.Identity;
using Travelogram.Models;

public class Comment
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public DateTime DatePosted { get; set; } = DateTime.Now;

    // Foreign key for the post
    public int PostId { get; set; }
    public Post? Post { get; set; }

    // Foreign key for the user
    public string? UserId { get; set; }
    public virtual IdentityUser? User { get; set; }
}
