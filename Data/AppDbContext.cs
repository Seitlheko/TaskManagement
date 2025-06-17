using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }

        public DbSet<User> Users { get; set; }


    }
}
