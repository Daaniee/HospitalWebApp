using hospitalwebapp.Attributes;
using hospitalwebapp.Models;

namespace hospitalwebapp.Services
{
    public class AuditLogger : IAuditLogger
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public AuditLogger(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        public void Log(string action, int? roleId = null, int? staffId = null, int? targetStaffId = null, int? targetPatientId = null, string details = null)
        {
            var currentStaffId = staffId ?? _httpContext.HttpContext?.Session.GetInt32("StaffId");

            var log = new AuditLog
            {
                StaffId = currentStaffId,
                Action = action,
                RoleId = roleId,
                TargetStaffId = targetStaffId,
                TargetPatientId = targetPatientId,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
