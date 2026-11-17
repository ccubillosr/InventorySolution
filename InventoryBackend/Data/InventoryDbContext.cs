using InventoryBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryBackend.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Stock> Stocks => Set<Stock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
        });

        // Product configuration with Category relationship
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Sku).IsRequired();
            entity.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Stock configuration with Product relationship
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.WarehouseLocation).IsRequired().HasMaxLength(50);
            entity.Property(s => s.Notes).HasMaxLength(500);
            entity.HasIndex(s => new { s.ProductId, s.WarehouseLocation }).IsUnique();
            entity.HasOne(s => s.Product)
                  .WithMany(p => p.Stocks)
                  .HasForeignKey(s => s.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
