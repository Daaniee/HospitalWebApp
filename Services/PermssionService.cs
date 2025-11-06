using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hospitalwebapp.Intefaces;
using hospitalwebapp.Models;
using Microsoft.EntityFrameworkCore;

namespace hospitalwebapp.Services
{
    public class PermissionService : PermissionInterface
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int staffId, string permissionName)
    {
        var staff = await _context.Staff
            .Include(s => s.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(s => s.Id == staffId);

        return staff?.Role?.RolePermissions
            .Any(rp => rp.Permission.Action == permissionName) ?? false;
    }
}
}