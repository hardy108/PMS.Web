using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class RKHEMPLOYEE1
    {
        public RKHEMPLOYEE1()
        {
        }

        public string RKH_EMPLOYEE_CODE { get; set; }
        public string RKH_MANDOR_CODE { get; set; }
        public string EMPID { get; set; }

        public virtual MEMPLOYEE MEMPLOYEE { get; set; }
        public virtual RKHHEADER1 RKHHEADER1 { get; set; }

    }
}
