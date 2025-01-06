using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class VDIVISI
    {
        [NotMapped()]
        public bool Seeding => CODE.ToUpper() == "Z";

    }
}
