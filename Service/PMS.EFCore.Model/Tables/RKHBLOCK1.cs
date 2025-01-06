using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class RKHBLOCK1
    {
        public RKHBLOCK1()
        {
        }

        public string RKH_BLOCK_CODE { get; set; }
        public string RKH_MANDOR_CODE { get; set; }
        public string BLOCKID { get; set; }
        public decimal OUTPUT { get; set; }

        public virtual MBLOCK MBLOCK { get; set; }
        public virtual RKHHEADER1 RKHHEADER1 { get; set; }

    }
}
