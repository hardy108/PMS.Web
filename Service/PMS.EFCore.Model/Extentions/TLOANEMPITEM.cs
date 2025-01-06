using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TLOANEMPITEM
    {
        [NotMapped]
        public string MATNAME { get; set; }
        [NotMapped]
        public string ACCNAME { get; set; }

    }
}
