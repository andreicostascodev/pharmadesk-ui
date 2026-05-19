using Microsoft.EntityFrameworkCore;
using PharmaDesk.Models;
namespace PharmaDesk.Data;
public class PharmaDeskDbContext(DbContextOptions<PharmaDeskDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Setting> Settings => Set<Setting>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Category>().HasOne(x => x.ParentCategory).WithMany(x => x.Children).HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Medicine>().HasIndex(x => x.Barcode).IsUnique();
        modelBuilder.Entity<Medicine>().Property(x => x.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<Medicine>().Property(x => x.Rating).HasPrecision(3, 2);
        modelBuilder.Entity<CartItem>().HasIndex(x => new { x.UserId, x.MedicineId }).IsUnique();
        modelBuilder.Entity<Order>().Property(x => x.TotalAmount).HasPrecision(12, 2);
        modelBuilder.Entity<Order>().Property(x => x.Discount).HasPrecision(12, 2);
        modelBuilder.Entity<Order>().Property(x => x.Tax).HasPrecision(12, 2);
        modelBuilder.Entity<Order>().Property(x => x.GrandTotal).HasPrecision(12, 2);
        modelBuilder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<OrderItem>().Property(x => x.TotalPrice).HasPrecision(12, 2);
        modelBuilder.Entity<Setting>().HasIndex(x => x.SettingKey).IsUnique();
    }
}
