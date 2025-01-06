using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    
    public partial class PMSContextBase
    {
        public List<JOBSEMPLASTDATE> jbs_Attendance_LastDay()
        {
            List<JOBSEMPLASTDATE> rows = null;

            this.LoadStoredProc("jbs_Attendance_LastDay")
                .Exec(r => rows = r.ToList<JOBSEMPLASTDATE>());
            return rows;
        }


        public void jbs_Attendance_LastDay_V2(string sessionId)
        {
            List<JOBSEMPLASTDATE> rows = null;

            this.LoadStoredProc("jbs_Attendance_LastDay_V2")
                .AddParam("SessionID", sessionId)
                .ExecNonQuery();
        }
    }

}
