using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterHarvestResult:GeneralFilter
    {
        public string EmpType { get; set; }
        public string HarvestCode { get; set; }
    }
}
