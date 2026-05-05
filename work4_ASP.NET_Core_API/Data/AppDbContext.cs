using Microsoft.EntityFrameworkCore;
using work4_ASP.NET_Core_API.Models;

namespace work4_ASP.NET_Core_API.Data;

public class AppDbContext : DbContext
{
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { 
}
public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Начальная таблица создаётся миграцией
    }
}