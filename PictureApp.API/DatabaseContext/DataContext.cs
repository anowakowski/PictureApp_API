using Microsoft.EntityFrameworkCore;
using PictureApp.API.Models;

namespace PictureApp.API.DatabaseContext
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext>  options) : base (options) {}

        public DbSet<User> Users { get; set; }

        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    }
}