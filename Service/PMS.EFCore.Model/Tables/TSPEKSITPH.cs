using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.EFCore.Model
{
    public partial class TSPEKSITPH
    {
        public string ID { get; set; }
        public string UNITID { get; set; }
        public string DIVID { get; set; }
        public DateTime TGLSPEKSI { get; set; }
        public decimal PRODMILLIN { get; set; }
        public decimal PRODMILLEX { get; set; }
        public decimal RESTAN { get; set; }
        public bool BRDTERTINGGAL { get; set; }
        public bool BRDBERSERAKAN { get; set; }
        public string STATUS { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime UPDATED { get; set; }

        public virtual MUNIT UNIT { get; set; }
        public virtual MDIVISI DIV { get; set; }

    }
}
