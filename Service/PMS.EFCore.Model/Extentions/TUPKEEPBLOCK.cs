using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TUPKEEPBLOCK
    {
        [NotMapped()]
        public string BLOCKCODE { get; set; }
        [NotMapped()]
        public short THNTANAM { get; set; }
        [NotMapped()]
        public decimal CURRENTPLANTED { get; set; }
        [NotMapped()]
        public string UOM1 { get; set; }
        [NotMapped()]
        public string UOM2 { get; set; }
    }
}
