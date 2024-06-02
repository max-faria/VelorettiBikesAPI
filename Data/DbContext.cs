using Microsoft.EntityFrameworkCore;
using VelorettiAPI.Models;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }
        public DbSet<User> Users {get; set; }
}