using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.EFCore.Model
{
    public partial class TTRAVEL
    {
        public string IDTRAVEL { get; set; }
        public string UNITID { get; set; }
        public string DIVID { get; set; }
        public string EMPLOYEEID { get; set; }
        public DateTime STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public string STATUS { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime UPDATED { get; set; }
        public string NOTE { get; set; }

        public virtual MUNIT UNIT { get; set; }
        public virtual MDIVISI DIV { get; set; }
        public virtual MEMPLOYEE EMPLOYEE { get; set; }
        public virtual MDOCSTATUS STATUSNavigation { get; set; }
    }
}
