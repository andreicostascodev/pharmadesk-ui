using PharmaDesk.Models;
namespace PharmaDesk.Services;
public interface IAuthService
{
    Task<User?> LoginAsync(string username, string password);
    Task<(bool Success, string Message)> RegisterAsync(string username, string password, string email, string fullName);
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}
public interface ICatalogService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Medicine>> SearchMedicinesAsync(string? query, int? categoryId = null, int skip = 0, int take = 40);
    Task<List<Medicine>> GetFeaturedAsync(string section);
    Task<Medicine?> GetMedicineAsync(int id);
    Task SaveMedicineAsync(Medicine medicine);
    Task DeleteMedicineAsync(int id);
    Task SaveCategoryAsync(Category category);
    Task<List<Medicine>> GetLowStockAsync();
}
public interface ICartService
{
    Task<List<CartItem>> GetCartAsync(int userId);
    Task AddToCartAsync(int userId, int medicineId, int quantity = 1);
    Task UpdateQuantityAsync(int cartItemId, int quantity);
    Task RemoveAsync(int cartItemId);
    Task ClearAsync(int userId);
}
public interface IOrderService
{
    Task<Order> CheckoutAsync(int userId, string shippingAddress, string paymentMethod, string? prescriptionSourcePath);
    Task<List<Order>> GetOrdersForUserAsync(int userId);
    Task<List<Order>> GetAllOrdersAsync();
    Task MarkShippedAsync(int orderId);
}
public interface IReportService
{
    Task<string> GenerateInvoiceAsync(Order order);
    Task<string> ExportSalesExcelAsync(DateTime from, DateTime to);
    Task<string> ExportSalesPdfAsync(DateTime from, DateTime to);
}
public interface IAuditService
{
    Task LogAsync(int? userId, string action, string tableName, int? recordId = null);
    Task<List<AuditLog>> GetLogsAsync();
}
public interface IToastService
{
    event Action<string>? ToastRaised;
    void Show(string message);
}
