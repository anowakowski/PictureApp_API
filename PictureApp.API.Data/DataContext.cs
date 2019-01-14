using Microsoft.EntityFrameworkCore;
using PictureApp.API.Models;

namespace PictureApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }

        public DbSet<AccountActivationToken> AccountActivationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Followers)
                .WithOne(f => f.Follower)
                .HasForeignKey(f => f.FollowerId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Following)
                .WithOne(f => f.Followee)
                .HasForeignKey(f => f.FolloweeId);
        }
    }
}