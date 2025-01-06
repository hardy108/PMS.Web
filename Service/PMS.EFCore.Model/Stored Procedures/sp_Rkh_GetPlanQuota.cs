using PMS.EFCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public static class sp_Rkh_GetPlanQuota
    {
        public static int GetRKHQuota(this PMSContextBase context, string unitId, DateTime actualDate, int paymentType,string rkhId)
        {
            //sp_Rkh_GetPlanQuota
            int result = 0;
            context.LoadStoredProc("sp_Rkh_GetPlanQuota")
                    .AddParam("UnitId", unitId)
                    .AddParam("ActualDate", actualDate)
                    .AddParam("PaymentType", paymentType)
                    .AddParam("RkhId", rkhId)
                    .ExecScalar(out result);
            return result;
        }
    }
}
