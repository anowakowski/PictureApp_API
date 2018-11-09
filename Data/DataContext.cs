using Microsoft.EntityFrameworkCore;
using PictureApp.API.Models;

namespace PictureApp_API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext>  options) : base (options) {}

        public DbSet<User> Users { get; set; }
    }
}