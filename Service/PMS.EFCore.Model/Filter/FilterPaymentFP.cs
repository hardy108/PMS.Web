using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterPaymentFP:GeneralFilter
    {
        public string DocNo { get; set; }
        public string EmpId { get; set; }
        public string TypeManual { get; set; }

    }
}
