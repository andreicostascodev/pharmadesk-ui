using System.Linq.Expressions;

namespace PharmaDesk.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
