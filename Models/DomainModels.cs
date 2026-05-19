using System.ComponentModel.DataAnnotations;
namespace PharmaDesk.Models;
public class Role
{
    public int Id { get; set; }
    [MaxLength(40)] public string Name { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}
public class User
{
    public int Id { get; set; }
    [MaxLength(80)] public string Username { get; set; } = string.Empty;
    [MaxLength(255)] public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(160)] public string Email { get; set; } = string.Empty;
    [MaxLength(180)] public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
public class Category
{
    public int Id { get; set; }
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}
public class Medicine
{
    public int Id { get; set; }
    [MaxLength(180)] public string Name { get; set; } = string.Empty;
    [MaxLength(180)] public string? GenericName { get; set; }
    [MaxLength(80)] public string Barcode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    [MaxLength(80)] public string DosageForm { get; set; } = string.Empty;
    [MaxLength(80)] public string Strength { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public bool IsPrescriptionRequired { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(500)] public string? ImageUrl { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public decimal Rating { get; set; } = 4.6m;
    public bool IsNew { get; set; }
    public bool IsPromotion { get; set; }
    public int DiscountPercent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int MedicineId { get; set; }
    public Medicine? Medicine { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public decimal LineTotal => (Medicine?.UnitPrice ?? 0) * Quantity;
}
public class Order
{
    public int Id { get; set; }
    [MaxLength(40)] public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    [MaxLength(40)] public string PaymentMethod { get; set; } = "Card";
    [MaxLength(500)] public string ShippingAddress { get; set; } = string.Empty;
    [MaxLength(40)] public string Status { get; set; } = "Noua";
    [MaxLength(500)] public string? PrescriptionUploadUrl { get; set; }
    [MaxLength(500)] public string? InvoicePath { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int MedicineId { get; set; }
    public Medicine? Medicine { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    [MaxLength(180)] public string Action { get; set; } = string.Empty;
    [MaxLength(120)] public string TableName { get; set; } = string.Empty;
    public int? RecordId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
public class Setting
{
    public int Id { get; set; }
    [MaxLength(120)] public string SettingKey { get; set; } = string.Empty;
    [MaxLength(1000)] public string SettingValue { get; set; } = string.Empty;
}
