using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class RKHMATERIAL1
    {
        public RKHMATERIAL1()
        {
        }

        public string RKH_MATERIAL_CODE { get; set; }
        public string RKH_MANDOR_CODE { get; set; }
        public string MATERIALID { get; set; }
        public decimal QTY { get; set; }

        public virtual MMATERIAL MATERIAL { get; set; }
        public virtual RKHHEADER1 RKHHEADER1 { get; set; }

    }
}
