using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hospitalwebapp.Attributes
{
    public interface IAuditLogger
    {
        void Log(
            string action,
            int? roleId = null,
            int? staffId = null,
            int? targetStaffId = null,
            int? targetPatientId = null,
            string details = null
        );
    }



}