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
    public class sp_PaymentDetail_GetPremiNonPanen_Result
    {
        public string TYPE { get; set; }
        public string EMPLOYEEID { get; set; }
        public decimal? PREMIHK { get; set; }
        public decimal? PREMIAMOUNT { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_PaymentDetail_GetPremiNonPanen_Result> sp_PaymentDetail_GetPremiNonPanen(string unitCode, DateTime startPeriod, DateTime startDate, DateTime endDate)
        {
            IEnumerable<sp_PaymentDetail_GetPremiNonPanen_Result> rows = null;
            this.LoadStoredProc("sp_PaymentDetail_GetPremiNonPanen")
                .AddParam("UnitCode", unitCode)
                .AddParam("StartPeriod", startPeriod)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .Exec(r => rows = r.ToList<sp_PaymentDetail_GetPremiNonPanen_Result>());
            return rows;
        }
    }
}
