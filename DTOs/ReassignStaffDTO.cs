using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hospitalwebapp.DTOs
{
    public class ReassignStaffDTO
    {
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }

        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}