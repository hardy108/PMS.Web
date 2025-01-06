using PMS.EFCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class sp_Rkh_PlanActual_Result
    {
        public decimal ACTUAL { get; set; }
        public decimal PLAN { get; set; }
    }


    public partial class PMSContextBase
    {
        public IEnumerable<sp_Rkh_PlanActual_Result> sp_Rkh_PlanActual(string DividionId, DateTime? ActualDate, short? PaymentType, string ActivityId, string UpkeepId)
        {

            IEnumerable<sp_Rkh_PlanActual_Result> rows = null;
            this.LoadStoredProc("sp_Rkh_PlanActual")
                 .AddParam("DividionId", DividionId)
                 .AddParam("ActualDate", ActualDate)
                 .AddParam("PaymentType", PaymentType)
                 .AddParam("ActivityId", ActivityId)
                 .AddParam("UpkeepId", UpkeepId).Exec(r => rows = r.ToList<sp_Rkh_PlanActual_Result>());
            return rows;
        }

    }
}
