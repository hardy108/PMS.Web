using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_PaymentDetail_GetManualPremi_Result
    {
        public string EMPID { get; set; }
        public string TYPEID { get; set; }
        public decimal PRMPNN { get; set; }
        public decimal PRMNPN { get; set; }
        public decimal INC { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_PaymentDetail_GetManualPremi_Result> sp_PaymentDetail_GetManualPremi_Result(string unitCode, DateTime startDate, DateTime endDate)
        {
            IEnumerable<sp_PaymentDetail_GetManualPremi_Result> rows = null;
            this.LoadStoredProc("sp_PaymentDetail_GetManualPremi")
                .AddParam("UnitCode", unitCode)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .Exec(r => rows = r.ToList<sp_PaymentDetail_GetManualPremi_Result>());
            return rows;
        }
    }

}
