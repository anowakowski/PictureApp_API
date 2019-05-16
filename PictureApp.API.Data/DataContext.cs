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

        public DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }

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

            modelBuilder.Entity<User>()
                .HasOne(x => x.ResetPasswordToken)
                .WithOne(x => x.User)
                .HasForeignKey<ResetPasswordToken>(x => x.UserId);

            modelBuilder.Entity<User>()
                .HasOne(x => x.ActivationToken)
                .WithOne(x => x.User)
                .HasForeignKey<AccountActivationToken>(x => x.UserId);
        }
    }
}