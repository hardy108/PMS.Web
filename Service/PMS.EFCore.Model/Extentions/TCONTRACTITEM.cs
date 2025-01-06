using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TCONTRACTITEM
    {
        [NotMapped]
        public string ACTNAME { get; set; }
    }
}
