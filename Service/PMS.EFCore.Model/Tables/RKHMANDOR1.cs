using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class RKHMANDOR1
    {
        public RKHMANDOR1()
        {
            RKHBLOCK1 = new HashSet<RKHBLOCK1>();
            RKHEMPLOYEE1 = new HashSet<RKHEMPLOYEE1>();
            RKHMATERIAL1 = new HashSet<RKHMATERIAL1>();
        }

        public string RKH_MANDOR_CODE { get; set; }
        public string RKH_CODE { get; set; }
        public string ACTID { get; set; }
        public string MANDORID { get; set; }

        public virtual MACTIVITY ACTIVITY { get; set; }
        public virtual MEMPLOYEE MANDOR { get; set; }

        public virtual ICollection<RKHBLOCK1> RKHBLOCK1 { get; set; }
        public virtual ICollection<RKHEMPLOYEE1> RKHEMPLOYEE1 { get; set; }
        public virtual ICollection<RKHMATERIAL1> RKHMATERIAL1 { get; set; }


    }
}
