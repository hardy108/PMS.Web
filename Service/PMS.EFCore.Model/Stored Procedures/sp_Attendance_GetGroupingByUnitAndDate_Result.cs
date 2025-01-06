using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_Attendance_GetGroupingByUnitAndDate_Result
    {
        public string ID { get; set; }
        public string DIVID { get; set; }
        public string EMPLOYEEID { get; set; }
        public DateTime DATE { get; set; }
        public int PRESENT { get; set; }
        public string REMARK { get; set; }
        public decimal? HK { get; set; }
        public string ABSENTCODE { get; set; }
        public string STATUS { get; set; }
        public string REF { get; set; }
        public int AUTO { get; set; }
        public string CREATEBY { get; set; }
        public DateTime CREATEDDATE { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime UPDATEDDATE { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_Attendance_GetGroupingByUnitAndDate_Result> sp_Attendance_GetGroupingByUnitAndDate(string unitCode, DateTime startDate, DateTime endDate, bool useFingerprint)
        {
            IEnumerable<sp_Attendance_GetGroupingByUnitAndDate_Result> rows = null;
            this.LoadStoredProc("sp_Attendance_GetGroupingByUnitAndDate")
                .AddParam("UnitCode", unitCode)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .AddParam("UseFinger", useFingerprint)
                .Exec(r => rows = r.ToList<sp_Attendance_GetGroupingByUnitAndDate_Result>());
            return rows;
        }
    }
}
