using Microsoft.EntityFrameworkCore;
using PharmaDesk.Data;
using PharmaDesk.Models;
namespace PharmaDesk.Services;
public class CartService(PharmaDeskDbContext db, IAuditService audit) : ICartService
{
    public Task<List<CartItem>> GetCartAsync(int userId) => db.CartItems.Include(x => x.Medicine).ThenInclude(x => x!.Category).Where(x => x.UserId == userId).OrderByDescending(x => x.AddedAt).ToListAsync();
    public async Task AddToCartAsync(int userId, int medicineId, int quantity = 1)
    {
        var existing = await db.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.MedicineId == medicineId);
        if (existing is null)
        {
            db.CartItems.Add(new CartItem { UserId = userId, MedicineId = medicineId, Quantity = quantity });
        }
        else
        {
            existing.Quantity += quantity;
        }
        await db.SaveChangesAsync();
        await audit.LogAsync(userId, "Add to cart", "CartItems", medicineId);
    }
    public async Task UpdateQuantityAsync(int cartItemId, int quantity)
    {
        var item = await db.CartItems.FirstAsync(x => x.Id == cartItemId);
        item.Quantity = Math.Max(1, quantity);
        await db.SaveChangesAsync();
    }
    public async Task RemoveAsync(int cartItemId)
    {
        var item = await db.CartItems.FirstAsync(x => x.Id == cartItemId);
        db.CartItems.Remove(item);
        await db.SaveChangesAsync();
    }
    public async Task ClearAsync(int userId)
    {
        var items = await db.CartItems.Where(x => x.UserId == userId).ToListAsync();
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync();
    }
}
