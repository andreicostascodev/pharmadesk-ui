using Microsoft.EntityFrameworkCore;
using PharmaDesk.Data;
using PharmaDesk.Models;
namespace PharmaDesk.Services;
public class CatalogService(PharmaDeskDbContext db, IAuditService audit, AppSession session) : ICatalogService
{
    public Task<List<Category>> GetCategoriesAsync() => db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    public Task<List<Medicine>> SearchMedicinesAsync(string? query, int? categoryId = null, int skip = 0, int take = 40)
    {
        var q = db.Medicines.Include(x => x.Category).AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(x => x.Name.Contains(term) || (x.GenericName != null && x.GenericName.Contains(term)) || x.Category!.Name.Contains(term));
        }
        if (categoryId.HasValue) q = q.Where(x => x.CategoryId == categoryId.Value);
        return q.OrderByDescending(x => x.IsPromotion).ThenByDescending(x => x.IsNew).Skip(skip).Take(take).ToListAsync();
    }
    public Task<List<Medicine>> GetFeaturedAsync(string section)
    {
        var q = db.Medicines.Include(x => x.Category).AsNoTracking().Where(x => x.IsActive);
        q = section switch
        {
            "Promotii" => q.Where(x => x.IsPromotion),
            "Noutati" => q.Where(x => x.IsNew),
            "Best" => q.OrderByDescending(x => x.Rating),
            _ => q.OrderByDescending(x => x.StockQuantity)
        };
        return q.Take(8).ToListAsync();
    }
    public Task<Medicine?> GetMedicineAsync(int id) => db.Medicines.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
    public async Task SaveMedicineAsync(Medicine medicine)
    {
        if (medicine.Id == 0) db.Medicines.Add(medicine); else db.Medicines.Update(medicine);
        await db.SaveChangesAsync();
        await audit.LogAsync(session.CurrentUser?.Id, medicine.Id == 0 ? "Create medicine" : "Update medicine", "Medicines", medicine.Id);
    }
    public async Task DeleteMedicineAsync(int id)
    {
        var item = await db.Medicines.FirstAsync(x => x.Id == id);
        item.IsActive = false;
        await db.SaveChangesAsync();
        await audit.LogAsync(session.CurrentUser?.Id, "Deactivate medicine", "Medicines", id);
    }
    public async Task SaveCategoryAsync(Category category)
    {
        if (category.Id == 0) db.Categories.Add(category); else db.Categories.Update(category);
        await db.SaveChangesAsync();
        await audit.LogAsync(session.CurrentUser?.Id, "Save category", "Categories", category.Id);
    }
    public Task<List<Medicine>> GetLowStockAsync() => db.Medicines.Include(x => x.Category).Where(x => x.StockQuantity <= x.ReorderLevel).AsNoTracking().ToListAsync();
}
