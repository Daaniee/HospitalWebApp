using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hospitalwebapp.DTOs
{
    public class BulkAssignRoleDto
    {
            public List<int> StaffIds { get; set; } = new();
            public int RoleId { get; set; }

    }
}