using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PharmaDesk.Data;

namespace PharmaDesk.Repositories;

public class GenericRepository<T>(PharmaDeskDbContext db) : IGenericRepository<T> where T : class
{
    public Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = db.Set<T>().AsQueryable();
        if (predicate is not null) query = query.Where(predicate);
        return query.ToListAsync();
    }

    public Task<T?> GetByIdAsync(int id) => db.Set<T>().FindAsync(id).AsTask();

    public async Task AddAsync(T entity)
    {
        db.Set<T>().Add(entity);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        db.Set<T>().Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        db.Set<T>().Remove(entity);
        await db.SaveChangesAsync();
    }
}
