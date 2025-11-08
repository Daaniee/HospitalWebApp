using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hospitalwebapp.Attributes;
using hospitalwebapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hospitalwebapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IAuditLogger _audit;

        public StaffController(AppDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }


        // [HttpPost("GetAll")]
        // public async Task<IActionResult> GetAll(StaffSearchDto dto)
        // {
        //     var query = _context.Staff
        //         .Include(s => s.Role)
        //         .Where(s => !s.IsDeleted);

        //     if (dto.Id != null)
        //         query = query.Where(s => s.Id == dto.Id);

        //     if (!string.IsNullOrWhiteSpace(dto.FullName))
        //         query = query.Where(s => s.FullName.ToLower().Contains(dto.FullName.ToLower()));

        //     if (!string.IsNullOrWhiteSpace(dto.Email))
        //         query = query.Where(s => s.Email.ToLower().Contains(dto.Email.ToLower()));

        //     if (dto.RoleId != null)
        //         query = query.Where(s => s.RoleId == dto.RoleId);

        //     if (!string.IsNullOrWhiteSpace(dto.SortBy))
        //     {
        //         query = dto.SortBy.ToLower() switch
        //         {
        //             "name" => dto.Descending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
        //             "email" => dto.Descending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
        //             "id" => dto.Descending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
        //             _ => query
        //         };
        //     }

        //     var totalCount = await query.CountAsync();
        //     var staffList = await query
        //         .Skip((dto.Page - 1) * dto.PageSize)
        //         .Take(dto.PageSize)
        //         .ToListAsync();

        //     var result = staffList.Select(s => new
        //     {
        //         s.Id,
        //         s.CustomId,
        //         s.FullName,
        //         s.Email,
        //         s.PhoneNumber,
        //         s.Gender,
        //         s.ProfileImageUrl,
        //         s.IsActive,
        //         Role = s.Role != null ? s.Role.Name : null,
        //         s.CreatedAt,
        //         s.UpdatedAt
        //     });

        //     return Ok(new ApiResponse<object>(true, 200, "Staff retrieved", new
        //     {
        //         Total = totalCount,
        //         Page = dto.Page,
        //         PageSize = dto.PageSize,
        //         Data = result
        //     }));
        // }

        // [RequirePermission("ViewStaff")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null, [FromQuery] bool descending = false)
        {
            var query = _context.Staff
                .Include(s => s.Role)
                .Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "name" => descending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
                    "email" => descending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
                    "id" => descending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
                    _ => query
                };
            }

            var totalCount = await query.CountAsync();
            var staffList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = staffList.Select(s => new
            {
                s.Id,
                s.CustomId,
                s.FullName,
                s.Email,
                s.PhoneNumber,
                s.Gender,
                s.ProfileImageUrl,
                s.IsActive,
                s.Specialization,
                Role = s.Role?.Name,
                s.CreatedAt,
                s.UpdatedAt
            });

            return Ok(new ApiResponse<object>(true, 200, "All staff retrieved", new
            {
                Total = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = result
            }));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> FilterStaff([FromBody] StaffSearchDto dto)
        {
            var query = _context.Staff
                .Include(s => s.Role)
                .Where(s => !s.IsDeleted);

            if (dto.Id != null)
                query = query.Where(s => s.Id == dto.Id);

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                query = query.Where(s => s.FullName.ToLower().Contains(dto.FullName.ToLower()));

            if (!string.IsNullOrWhiteSpace(dto.Email))
                query = query.Where(s => s.Email.ToLower().Contains(dto.Email.ToLower()));

            if (dto.RoleId != null)
                query = query.Where(s => s.RoleId == dto.RoleId);

            if (!string.IsNullOrWhiteSpace(dto.SortBy))
            {
                query = dto.SortBy.ToLower() switch
                {
                    "name" => dto.Descending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
                    "email" => dto.Descending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
                    "id" => dto.Descending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
                    _ => query
                };
            }

            var totalCount = await query.CountAsync();
            var staffList = await query
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            var result = staffList.Select(s => new
            {
                s.Id,
                s.CustomId,
                s.FullName,
                s.Email,
                s.PhoneNumber,
                s.Gender,
                s.ProfileImageUrl,
                s.IsActive,
                s.Specialization,
                Role = s.Role?.Name,
                s.CreatedAt,
                s.UpdatedAt
            });

            return Ok(new ApiResponse<object>(true, 200, "Filtered staff retrieved", new
            {
                Total = totalCount,
                Page = dto.Page,
                PageSize = dto.PageSize,
                Data = result
            }));
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateStaffDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseNoData(false, 400, "Invalid input"));

            var exists = await _context.Staff.AnyAsync(s => s.Email.ToLower() == dto.Email.ToLower());
            if (exists)
                return Conflict(new ApiResponseNoData(false, 409, "Staff with this email already exists"));

            // Get role name
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
                return BadRequest(new ApiResponseNoData(false, 400, "Invalid role"));

            // Generate prefix from role name
            string prefix = role.Name.Substring(0, Math.Min(3, role.Name.Length)).ToUpper();

            // Count existing staff with same role
            var count = await _context.Staff
                .IgnoreQueryFilters()
                .Where(s => s.RoleId == dto.RoleId)
                .CountAsync();

            // Generate CustomId
            string customId = $"{prefix}{(count + 1).ToString().PadLeft(4, '0')}";


            var staff = new Staff
            {
                CustomId = customId,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Gender = dto.Gender,
                Specialization = dto.Specialization,
                ProfileImageUrl = dto.ProfileImageUrl,
                PasswordHash = dto.PasswordHash,
                RoleId = dto.RoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();

            var adminId = HttpContext.Session?.GetInt32("StaffId") ?? 0;
            _audit.Log("CreateStaff", null, adminId, staff.Id);

            return Ok(new ApiResponse<object>(true, 201, "Staff created", new
            {
                staff.Id,
                staff.CustomId,
                staff.FullName,
                staff.Email
            }));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDto dto)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null || staff.IsDeleted)
                return NotFound(new ApiResponseNoData(false, 404, "Staff not found"));

            // Apply updates
            staff.FullName = dto.FullName ?? staff.FullName;
            staff.Email = dto.Email ?? staff.Email;
            staff.PhoneNumber = dto.PhoneNumber ?? staff.PhoneNumber;
            staff.Gender = dto.Gender ?? staff.Gender;
            staff.Specialization = dto.Specialization ?? staff.Specialization;
            staff.ProfileImageUrl = dto.ProfileImageUrl ?? staff.ProfileImageUrl;
            staff.IsActive = dto.IsActive ?? staff.IsActive;
            staff.RoleId = dto.RoleId ?? staff.RoleId;
            staff.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var adminId = HttpContext.Session?.GetInt32("StaffId") ?? 0;
            _audit.Log("UpdateStaff", null, adminId, staff.Id);

            return Ok(new ApiResponse<object>(true, 200, "Staff updated", new
            {
                staff.Id,
                staff.CustomId,
                staff.FullName,
                staff.Email
            }));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteStaff(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null || staff.IsDeleted)
                return NotFound(new ApiResponseNoData(false, 404, "Staff not found"));

            staff.IsDeleted = true;
            staff.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var adminId = HttpContext.Session?.GetInt32("StaffId") ?? 0;
            _audit.Log("SoftDeleteStaff", null, adminId, staff.Id);

            return Ok(new ApiResponseNoData(true, 200, "Staff soft-deleted"));
        }

        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreStaff(int id)
        {
            var staff = await _context.Staff
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null || !staff.IsDeleted)
                return NotFound(new ApiResponseNoData(false, 404, "Staff not found or not deleted"));

            staff.IsDeleted = false;
            staff.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var adminId = HttpContext.Session?.GetInt32("StaffId") ?? 0;
            _audit.Log("RestoreStaff", null, adminId, staff.Id);

            return Ok(new ApiResponseNoData(true, 200, "Staff restored"));
        }

        // [RequirePermission("ManageRoles")]
        [HttpGet("role-audit/{id}")]
        public async Task<IActionResult> GetRoleAuditLogs(int id)
        {
            var staff = await _context.Staff
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
                return NotFound(new ApiResponseNoData(false, 404, "Staff not found"));

            var logs = await _context.AuditLogs
                .Where(r => r.TargetStaffId == id || r.StaffId == id)
                .OrderByDescending(r => r.Timestamp)
                .Select(r => new
                {
                    r.Id,
                    r.Action,
                    r.Timestamp,
                    PerformedBy = _context.Staff
                        .Where(s => s.Id == r.StaffId)
                        .Select(s => s.FullName)
                        .FirstOrDefault(),
                    Target = _context.Staff
                        .Where(s => s.Id == r.TargetStaffId)
                        .Select(s => s.FullName)
                        .FirstOrDefault(),
                    RoleName = _context.Roles
                        .Where(role => role.Id == r.RoleId)
                        .Select(role => role.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, 200, "Role audit logs retrieved", logs));
        }   

    }
}
