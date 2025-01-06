using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class THARVESTCOLLECT
    {
        [NotMapped()]
        public string EMPNAME { get; set; }

        [NotMapped()]
        public string BLOCKCODE { get; set; }

        [NotMapped()]
        public string TPHCODE { get; set; }
    }
}
