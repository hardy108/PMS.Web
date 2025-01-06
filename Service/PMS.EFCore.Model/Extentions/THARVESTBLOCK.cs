using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class THARVESTBLOCK
    {
        [NotMapped()]
        public decimal QTYFINE { get; set; }

        [NotMapped()]
        public string BLOCKCODE { get; set; }

        [NotMapped()]
        public string EMPNAME { get; set; }

    }
}
