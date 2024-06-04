using Microsoft.EntityFrameworkCore;
using VelorettiAPI.Models;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }
        public DbSet<User> Users {get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().Property(u => u.IsAdmin).HasDefaultValue(false);
    }
}