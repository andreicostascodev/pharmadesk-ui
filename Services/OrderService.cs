using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PharmaDesk.Data;
using PharmaDesk.Models;
using System.IO;

namespace PharmaDesk.Services;

public class OrderService(PharmaDeskDbContext db, IConfiguration config, IReportService reports, IAuditService audit) : IOrderService
{
    public async Task<Order> CheckoutAsync(int userId, string shippingAddress, string paymentMethod, string? prescriptionSourcePath)
    {
        var cart = await db.CartItems.Include(x => x.Medicine).Where(x => x.UserId == userId).ToListAsync();
        if (cart.Count == 0) throw new InvalidOperationException("Cosul este gol.");
        if (cart.Any(x => x.Medicine!.IsPrescriptionRequired) && string.IsNullOrWhiteSpace(prescriptionSourcePath))
        {
            throw new InvalidOperationException("Comanda contine produse cu prescriptie. Incarca un PDF.");
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            OrderNumber = await CreateOrderNumberAsync(),
            PaymentMethod = paymentMethod,
            ShippingAddress = shippingAddress,
            Status = paymentMethod == "Card" ? "Platita" : "Noua"
        };

        if (!string.IsNullOrWhiteSpace(prescriptionSourcePath))
        {
            var folder = Path.GetFullPath(config["App:PrescriptionFolder"] ?? "Prescriptions");
            Directory.CreateDirectory(folder);
            var target = Path.Combine(folder, $"{order.OrderNumber}-{Path.GetFileName(prescriptionSourcePath)}");
            File.Copy(prescriptionSourcePath, target, true);
            order.PrescriptionUploadUrl = target;
        }

        foreach (var item in cart)
        {
            if (item.Medicine!.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Stoc insuficient pentru {item.Medicine.Name}.");
            }

            item.Medicine.StockQuantity -= item.Quantity;
            order.Items.Add(new OrderItem
            {
                MedicineId = item.MedicineId,
                Quantity = item.Quantity,
                UnitPrice = item.Medicine.UnitPrice,
                TotalPrice = item.Medicine.UnitPrice * item.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(x => x.TotalPrice);
        order.Discount = Math.Round(order.Items.Sum(x => x.TotalPrice * (cart.First(c => c.MedicineId == x.MedicineId).Medicine!.DiscountPercent / 100m)), 2);
        order.Tax = Math.Round((order.TotalAmount - order.Discount) * 0.09m, 2);
        order.GrandTotal = order.TotalAmount - order.Discount + order.Tax;

        db.Orders.Add(order);
        db.CartItems.RemoveRange(cart);
        await db.SaveChangesAsync();
        order.InvoicePath = await reports.GenerateInvoiceAsync(order);
        await WriteDummyEmailAsync(order);
        await audit.LogAsync(userId, "Checkout", "Orders", order.Id);
        return order;
    }

    public Task<List<Order>> GetOrdersForUserAsync(int userId) => db.Orders.Include(x => x.Items).ThenInclude(x => x.Medicine).Where(x => x.UserId == userId).OrderByDescending(x => x.OrderDate).ToListAsync();

    public Task<List<Order>> GetAllOrdersAsync() => db.Orders.Include(x => x.User).Include(x => x.Items).ThenInclude(x => x.Medicine).OrderByDescending(x => x.OrderDate).ToListAsync();

    public async Task MarkShippedAsync(int orderId)
    {
        var order = await db.Orders.FirstAsync(x => x.Id == orderId);
        order.Status = "Expediata";
        await db.SaveChangesAsync();
        await audit.LogAsync(order.UserId, "Mark shipped", "Orders", orderId);
    }

    private async Task<string> CreateOrderNumberAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await db.Orders.CountAsync(x => x.OrderNumber.StartsWith($"PH-{today}")) + 1;
        return $"PH-{today}-{count:0000}";
    }

    private async Task WriteDummyEmailAsync(Order order)
    {
        var path = Path.GetFullPath(config["App:EmailLogFile"] ?? "Logs/dummy-email.log");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.AppendAllTextAsync(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm}] Email dummy: comanda {order.OrderNumber}, total {order.GrandTotal:0.00} RON, status {order.Status}{Environment.NewLine}");
    }
}
