using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class VSCHEDULEEMPLOYEE
    {
        public string ID { get; set; }
        public string UNITCODE { get; set; }
        public string EMPLOYEEID { get; set; }
        public string EMPLOYEECODE { get; set; }
        public string EMPLOYEENAME { get; set; }
        public DateTime DATE { get; set; }
        public DateTime INSTART { get; set; }
        public DateTime INEND { get; set; }
        public DateTime OUTSTART { get; set; }
        public DateTime OUTEND { get; set; }
        public DateTime BREAKSTART { get; set; }
        public DateTime BREAKEND { get; set; }
        public DateTime INTIME { get; set; }
        public DateTime OUTTIME { get; set; }
        public string STATUS { get; set; }
        public DateTime UPDATED { get; set; }
    }

}
