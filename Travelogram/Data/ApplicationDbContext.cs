using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Travelogram.Models;

namespace Travelogram.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> // Notice the change here
    {
        // Parameterless constructor for Moq
        protected ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
