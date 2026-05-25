using Inpad.Api.Data;
using Inpad.Api.Models;

namespace Inpad.Api.Services;

public class AuditService(AppDbContext db)
{
    public async Task LogAsync(string action, string entityType, int? entityId = null, string? details = null, int? userId = null, string? userEmail = null)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            UserId = userId,
            UserEmail = userEmail
        });
        await db.SaveChangesAsync();
    }
}
