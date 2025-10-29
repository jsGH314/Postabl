using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        //public DbSet<Follower> Followers { get; set; }

        //public DbSet<Comment> Comments { get; set; }
    }
}
