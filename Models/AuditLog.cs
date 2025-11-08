using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hospitalwebapp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? StaffId { get; set; }         // Who performed the action
        public int? RoleId { get; set; }          // Which role was affected
        public int? TargetStaffId { get; set; }   // Who was affected (e.g. reassigned)
        public int? TargetPatientId { get; set; } // For patient-related actions
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;      // Optional: summary or JSON
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }


}