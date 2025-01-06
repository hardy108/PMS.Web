using System;
using System.Collections.Generic;

namespace PMS.EFCore.Model
{
    public partial class RKHHEADER1
    { 
        public RKHHEADER1()
        {
            RKHMANDOR1 = new HashSet<RKHMANDOR1>();
            RKHBLOCK1 = new HashSet<RKHBLOCK1>();
            RKHEMPLOYEE1 = new HashSet<RKHEMPLOYEE1>();
            RKHMATERIAL1 = new HashSet<RKHMATERIAL1>();

            VRKH1MANDORREF = new HashSet<VRKH1MANDOR>();
            VRKH1BLOCKREF = new HashSet<VRKH1BLOCK>();
            VRKH1EMPLOYEEREF = new HashSet<VRKH1EMPLOYEE>();
            VRKH1MATERIALREF = new HashSet<VRKH1MATERIAL>();
        }

        public string RKH_CODE { get; set; }
        public string RKH_DIVID { get; set; }
        public DateTime RKH_DATE { get; set; }
        public DateTime RKH_ACTDATE { get; set; }
        public string NOTE{ get; set; }
        public string STATUS { get; set; }
        public string CREATEBY { get; set; }
        public DateTime CREATED { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime UPDATED { get; set; }
        public string CANCELEDCOMMENT { get; set; }

        public virtual MDIVISI DIV { get; set; }
        public virtual MDOCSTATUS STATUSNavigation { get; set; }

        public virtual ICollection<RKHMANDOR1> RKHMANDOR1 { get; set; }
        public virtual ICollection<RKHBLOCK1> RKHBLOCK1 { get; set; }
        public virtual ICollection<RKHEMPLOYEE1> RKHEMPLOYEE1 { get; set; }
        public virtual ICollection<RKHMATERIAL1> RKHMATERIAL1 { get; set; }

        //custom
        public virtual ICollection<VRKH1MANDOR> VRKH1MANDORREF { get; set; }
        public virtual ICollection<VRKH1BLOCK> VRKH1BLOCKREF { get; set; }
        public virtual ICollection<VRKH1EMPLOYEE> VRKH1EMPLOYEEREF { get; set; }
        public virtual ICollection<VRKH1MATERIAL> VRKH1MATERIALREF { get; set; }


    }
}
