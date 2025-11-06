using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hospitalwebapp.Models;
using hospitalwebapp.DTOs;
using hospitalwebapp.Attributes;

namespace hospitalwebapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogger _audit;

        public AdminController(AppDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        
        [RequirePermission("ManageRoles")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRolesIncludingDeleted()
        {
            var roles = await _context.Roles
                .IgnoreQueryFilters()
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();

            var response = roles.Select(role => new
            {
                role.Id,
                role.Name,
                role.IsDeleted,
                Permissions = role.RolePermissions.Select(rp => rp.Permission.Action).ToList()
            });

            return Ok(new ApiResponse<object>(true, 200, "All roles retrieved", response));
        }

        [RequirePermission("ManageRoles")]
        [HttpGet("search")]
        public async Task<IActionResult> GetRole([FromQuery] int? id, [FromQuery] string? name)
        {
            Role? role = null;

            if (id.HasValue)
            {
                role = await _context.Roles
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == id.Value);
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                var searchName = name!.ToLower();
                role = await _context.Roles
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Name.ToLower() == searchName);
            }
            else
            {
                return BadRequest(new ApiResponseNoData(false, 400, "Either 'id' or 'name' must be provided"));
            }

            if (role == null)
                return NotFound(new ApiResponseNoData(false, 404, "Role not found"));

            var result = new
            {
                role.Id,
                role.Name,
                Permissions = role.RolePermissions.Select(rp => rp.Permission.Action).ToList()
            };

            return Ok(new ApiResponse<object>(true, 200, "Role retrieved successfully", result));
        }

        [RequirePermission("ManageRoles")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto dto)
        {
            if (dto.Id == null && string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new ApiResponseNoData(false, 400, "Provide either role ID or name"));

            Role? role = null;

            if (dto.Id != null)
                role = await _context.Roles.FindAsync(dto.Id);
            else if (!string.IsNullOrWhiteSpace(dto.Name))
                role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Name);

            if (role == null)
                return NotFound(new ApiResponseNoData(false, 404, "Role not found"));

            if (string.IsNullOrWhiteSpace(dto.NewName))
                return BadRequest(new ApiResponseNoData(false, 400, "New name is required"));

            role.Name = dto.NewName;
            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("StaffId")?? 999;
            _audit.Log("UpdateRoleName", role.Id, adminId, null);



            return Ok(new ApiResponse<object>(true, 200, "Role updated successfully", new { role.Id, role.Name }));
        }

        [RequirePermission("ManagePermissions")]
        [HttpGet("permissions", Name = "Search By Permission")]
        public async Task<IActionResult> GetRolePermissions([FromQuery] int? id, [FromQuery] string? name)
        {
            if (id == null && string.IsNullOrWhiteSpace(name))
                return BadRequest(new ApiResponseNoData(false, 400, "Provide either role ID or name"));

            Role? role = null;

            if (id != null)
                role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == id);
            else
                role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());

            if (role == null)
                return NotFound(new ApiResponseNoData(false, 404, "Role not found"));

            var permissions = role.RolePermissions
                .Select(rp => new { rp.Permission.Id, rp.Permission.Action })
                .ToList();

            return Ok(new ApiResponse<object>(true, 200, "Permissions retrieved successfully", permissions));
        }

        [RequirePermission("ManagePermissions")]
        [HttpPost("permissions/assign")]
        public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsDto dto)
        {
            if (dto.RoleId == null && string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new ApiResponseNoData(false, 400, "Provide either role ID or name"));

            Role? role = null;

            if (dto.RoleId != null)
                role = await _context.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == dto.RoleId);
            else
                role = await _context.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Name == dto.RoleName);

            if (role == null)
                return NotFound(new ApiResponseNoData(false, 404, "Role not found"));

            var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            var newPermissions = dto.PermissionIds
                .Where(pid => !existingPermissionIds.Contains(pid))
                .Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid })
                .ToList();

            if (!newPermissions.Any())
                return BadRequest(new ApiResponseNoData(false, 400, "No new permissions to assign"));

            await _context.RolePermissions.AddRangeAsync(newPermissions);
            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("StaffId");
            _audit.Log("AssignPermissions", role.Id, adminId, null);

            return Ok(new ApiResponseNoData(true, 200, "Permissions assigned successfully"));
        }

        [RequirePermission("ManagePermissions")]
        [HttpDelete("permissions/remove")]
        public async Task<IActionResult> RemovePermissions([FromBody] RemovePermissionsDto dto)
        {
            if (dto.RoleId == null && string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new ApiResponseNoData(false, 400, "Provide either role ID or name"));

            Role? role = null;

            if (dto.RoleId != null)
                role = await _context.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == dto.RoleId);
            else if (!string.IsNullOrWhiteSpace(dto.RoleName))
                role = await _context.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Name == dto.RoleName);

            if (role == null)
                return NotFound(new ApiResponseNoData(false, 404, "Role not found"));

            var toRemove = role.RolePermissions
                .Where(rp => dto.PermissionIds.Contains(rp.PermissionId))
                .ToList();

            if (!toRemove.Any())
                return BadRequest(new ApiResponseNoData(false, 400, "No matching permissions found to remove"));

            _context.RolePermissions.RemoveRange(toRemove);
            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("StaffId");
            _audit.Log("RemovePermissions", role.Id, adminId, null);



            return Ok(new ApiResponseNoData(true, 200, "Permissions removed successfully"));
        }

        [RequirePermission("ManageRoles")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new ApiResponseNoData(false, 400, "Role name is required"));

            bool exists = await _context.Roles.AnyAsync(r => r.Name.ToLower() == name.ToLower());
            if (exists)
                return Conflict(new ApiResponseNoData(false, 409, "Role already exists"));

            var role = new Role { Name = name };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var adminId = HttpContext.Session.GetInt32("StaffId");
            _audit.Log("CreateRole", role.Id, adminId, null);

            return Ok(new ApiResponse<object>(true, 201, "Role created successfully", new { role.Id, role.Name }));
        }

        [RequirePermission("ManageRoles")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteRole(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return NotFound(new ApiResponseNoData(false, 404, "Role not found"));

            if (role.IsDeleted)
                return BadRequest(new ApiResponseNoData(false, 400, "Role is already deleted"));

            role.IsDeleted = true;
            await _context.SaveChangesAsync();

            var adminId = HttpContext.Session.GetInt32("StaffId");
            _audit.Log("SoftDeleteRole", role.Id, adminId, null);



            return Ok(new ApiResponseNoData(true, 200, "Role soft-deleted successfully"));
        }

        [RequirePermission("ManageRoles")]
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreRole(int id)
        {
            var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);
            if (role == null || !role.IsDeleted)
                return NotFound(new ApiResponseNoData(false, 404, "Role not found or not deleted"));

            role.IsDeleted = false;
            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("StaffId");
            _audit.Log("RestoreRole", role.Id, adminId, null);

            return Ok(new ApiResponseNoData(true, 200, "Role restored successfully"));
        }

        [RequirePermission("ManageRoles")]
        [HttpPut("staff/reassign")]
        public async Task<IActionResult> ReassignStaffRole([FromBody] ReassignStaffDTO dto)
        {
            if (dto.StaffId == null && string.IsNullOrWhiteSpace(dto.StaffName))
                return BadRequest(new ApiResponseNoData(false, 400, "Provide either staff ID or name"));

            if (dto.RoleId == null && string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new ApiResponseNoData(false, 400, "Provide either role ID or name"));

            Staff? staff = null;
            Role? role = null;

            if (dto.StaffId != null)
                staff = await _context.Staff.FindAsync(dto.StaffId);
            else
                staff = await _context.Staff.FirstOrDefaultAsync(s => s.FullName.ToLower() == dto.StaffName!.ToLower());

            if (dto.RoleId != null)
                role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId && !r.IsDeleted);
            else
                role = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == dto.RoleName!.ToLower() && !r.IsDeleted);

            if (staff == null || role == null)
                return NotFound(new ApiResponseNoData(false, 404, "Staff or role not found"));

            staff.RoleId = role.Id;
            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("StaffId");

            _audit.Log("ReassignStaffRole", role.Id, adminId, staff.Id);
            return Ok(new ApiResponseNoData(true, 200, "Staff reassigned to new role"));
        }
        
        [RequirePermission("ManageRoles")]
        [HttpPut("bulk-assign")]
        public async Task<IActionResult> BulkAssignRole([FromBody] BulkAssignRoleDto dto)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId && !r.IsDeleted);
            if (role == null)
                return NotFound(new ApiResponseNoData(false, 404, "Role not found"));

            var staffList = await _context.Staff.Where(s => dto.StaffIds.Contains(s.Id)).ToListAsync();
            if (!staffList.Any())
                return BadRequest(new ApiResponseNoData(false, 400, "No matching staff found"));

            foreach (var staff in staffList)
                staff.RoleId = role.Id;

            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("StaffId");
            foreach (var staff in staffList)
            {
                _audit.Log("BulkAssignRole", role.Id, adminId, staff.Id);
            }

            return Ok(new ApiResponseNoData(true, 200, "Role assigned to staff"));
        }

        [RequirePermission("ManageRoles")]
        [HttpPost("create-with-permissions")]
        public async Task<IActionResult> CreateRoleWithPermissions([FromBody] CreateRoleWithPermissionsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new ApiResponseNoData(false, 400, "Role name is required"));

            bool exists = await _context.Roles.AnyAsync(r => r.Name.ToLower() == dto.Name.ToLower());
            if (exists)
                return Conflict(new ApiResponseNoData(false, 409, "Role already exists"));

            var role = new Role { Name = dto.Name };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var rolePermissions = dto.PermissionIds
                .Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid })
                .ToList();

            await _context.RolePermissions.AddRangeAsync(rolePermissions);
            await _context.SaveChangesAsync();
            var adminId = HttpContext.Session.GetInt32("StaffId");
            _audit.Log("CreateRoleWithPermissions", role.Id, adminId, null);

            return Ok(new ApiResponse<object>(true, 201, "Role created with permissions", new { role.Id, role.Name }));
        }

        [RequirePermission("ViewAuditLogs")]
        [HttpGet("{id}/audit")]
        public async Task<IActionResult> GetRoleAudit(int id)
        {
            var logs = await _context.AuditLogs
                .Where(log => log.RoleId == id)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, 200, "Audit logs retrieved", logs));
        }



    }
}