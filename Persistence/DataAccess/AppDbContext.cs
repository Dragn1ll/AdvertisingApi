using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public virtual DbSet<AdvertisingPlatformEntity> AdvertisingPlatforms { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdvertisingPlatformEntity>()
            .HasKey(e => e.Id);
        
        modelBuilder.Entity<AdvertisingPlatformEntity>()
            .Property(e => e.Locations)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
    }
}