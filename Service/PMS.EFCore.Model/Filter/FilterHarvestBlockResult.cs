using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterHarvestBlockResult:GeneralFilter
    {
        public string Source { get; set; }
        public string NoSPB { get; set; }
        public string VehId { get; set; }
        public string Driver { get; set; }
        public string BlockId { get; set; }
    }
}
