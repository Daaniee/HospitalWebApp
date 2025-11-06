// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
// using Microsoft.EntityFrameworkCore;
// using hospitalwebapp.Models;

// public class HasPermissionAttribute : Attribute, IAuthorizationFilter
// {
//     private readonly string _requiredPermission;

//     public HasPermissionAttribute(string requiredPermission)
//     {
//         _requiredPermission = requiredPermission;
//     }

//     public void OnAuthorization(AuthorizationFilterContext context)
//     {
//         var staffId = context.HttpContext.Session.GetInt32("StaffId");

//         if (staffId == null)
//         {
//             context.Result = new ForbidResult();
//             return;
//         }

//         var db = context.HttpContext.RequestServices.GetService<AppDbContext>();
//         var staff = db.Staff
//             .Include(s => s.Role)
//                 .ThenInclude(r => r.RolePermissions)
//                     .ThenInclude(rp => rp.Permission)
//             .FirstOrDefault(s => s.Id == staffId.Value);

//         if (staff == null || staff.Role == null || staff.Role.IsDeleted)
//         {
//             context.Result = new ForbidResult();
//             return;
//         }

//         var hasPermission = staff.Role.RolePermissions
//             .Any(rp => rp.Permission.Action == _requiredPermission);

//         if (!hasPermission)
//         {
//             context.Result = new ForbidResult();
//         }
//     }
// }


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using hospitalwebapp.Services;
using hospitalwebapp.Intefaces;
using hospitalwebapp.Models;

public class RequirePermissionAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var permissionService = context.HttpContext.RequestServices.GetService<PermissionInterface>();
        var staffId = context.HttpContext.Session.GetInt32("StaffId") ?? 0;

        if (permissionService == null || !await permissionService.HasPermissionAsync(staffId, _permission))
        {
            context.Result = new UnauthorizedObjectResult(new ApiResponseNoData(false, 403, "Access denied"));
            return;
        }

        await next();
    }
}
