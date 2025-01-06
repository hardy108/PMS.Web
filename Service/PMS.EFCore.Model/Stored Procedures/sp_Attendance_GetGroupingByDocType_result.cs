using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_Attendance_GetGroupingByDocType_result
    {
        public string EMPID { get; set; }
        public decimal? HK { get; set; }
        public string DOCTYPE { get; set; }
        public string MO { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_Attendance_GetGroupingByDocType_result> sp_Attendance_GetGroupingByDocType(string unitId, DateTime startDate, DateTime endDate, bool useFinger)
        {
            IEnumerable<sp_Attendance_GetGroupingByDocType_result> rows = null;
            this.LoadStoredProc("sp_Attendance_GetGroupingByDocType")
                .AddParam("UnitId", unitId)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .AddParam("UseFinger", useFinger)
                .Exec(r => rows = r.ToList<sp_Attendance_GetGroupingByDocType_result>());
            return rows;
        }
    }
}
