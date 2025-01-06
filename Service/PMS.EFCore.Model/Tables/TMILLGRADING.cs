using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.EFCore.Model
{
    public partial class TMILLGRADING
    {
        public string ID { get; set; }
        public string UNITID { get; set; }
        public string DIVID { get; set; }
        public DateTime GRADINGDATE { get; set; }
        public decimal QTYPCT { get; set; }
        public string STATUS { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime UPDATED { get; set; }

        public virtual MUNIT UNIT { get; set; }
        public virtual MDIVISI DIV { get; set; }
        public virtual MDOCSTATUS STATUSNavigation { get; set; }
    }
}
