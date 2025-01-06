using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class THARVESTEMPLOYEE
    {
        [NotMapped()]
        public string EMPNAME { get; set; }

        [NotMapped()]
        public string GEMPNAME { get; set; }

        [NotMapped()]
        public string EMPTYPE { get; set; }

        [NotMapped()]
        public string UNITNAME { get; set; }


        [NotMapped()]
        public decimal QTY { get; set; }

        [NotMapped()]
        public decimal QTYKG { get; set; }

        [NotMapped()]
        public decimal QTYFINE { get; set; }


    }
}
