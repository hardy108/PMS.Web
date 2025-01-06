using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_PaymentDetail_GetOvertime_Result
    {
        public string EMPLOYEEID { get; set; }
        public decimal? OT150 { get; set; }
        public decimal? OT200 { get; set; }
        public decimal? OT300 { get; set; }
        public decimal? OT400 { get; set; }
        public decimal? DEBIT { get; set; }
    }


    public partial class PMSContextBase
    {
        public IEnumerable<sp_PaymentDetail_GetOvertime_Result> sp_PaymentDetail_GetOvertime(string unitCode, DateTime startDate, DateTime endDate, bool hkCalcByFingerPrint)
        {
            IEnumerable<sp_PaymentDetail_GetOvertime_Result> rows = null;
            this.LoadStoredProc("sp_PaymentDetail_GetOvertime")
                .AddParam("UnitCode", unitCode)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .AddParam("UseFinger", hkCalcByFingerPrint)
                .Exec(r => rows = r.ToList<sp_PaymentDetail_GetOvertime_Result>());
            return rows;
        }
    }

}
