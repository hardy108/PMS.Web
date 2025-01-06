using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterSPB : GeneralFilter
    {
        public string SPBNo { get; set; }
        public string SPBNoDHS { get; set; }
        public string MillCode { get; set; }
        public string VehicleNo { get; set; }
        public string DriverName { get; set; }
        public string OperatorName { get; set; }
        public string BlockId { get; set; }

    }
}
