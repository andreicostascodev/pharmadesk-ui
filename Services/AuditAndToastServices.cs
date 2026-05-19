using Microsoft.EntityFrameworkCore;
using PharmaDesk.Data;
using PharmaDesk.Models;
namespace PharmaDesk.Services;
public class AuditService(PharmaDeskDbContext db) : IAuditService
{
    public async Task LogAsync(int? userId, string action, string tableName, int? recordId = null)
    {
        db.AuditLogs.Add(new AuditLog { UserId = userId, Action = action, TableName = tableName, RecordId = recordId, Timestamp = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
    public Task<List<AuditLog>> GetLogsAsync() => db.AuditLogs.Include(x => x.User).AsNoTracking().OrderByDescending(x => x.Timestamp).Take(300).ToListAsync();
}
public class ToastService : IToastService
{
    public event Action<string>? ToastRaised;
    public void Show(string message) => ToastRaised?.Invoke(message);
}
