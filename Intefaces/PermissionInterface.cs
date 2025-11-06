using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hospitalwebapp.Intefaces
{
    public interface PermissionInterface
    {
         Task<bool> HasPermissionAsync(int staffId, string permissionName);
    }
}