using WebApplication3.Model;

namespace WebApplication3
{
    public interface IAuditLogService
    {
        Task LogAsync(string userId, string action);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly AuthDbContext _context;

        public AuditLogService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string action)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow,
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }


}