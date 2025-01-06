using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterRKH1:GeneralFilter
    {
        public string RKHCode { get; set; }
        public string ActivityID { get; set; }
        public DateTime RKHDate { get; set; }
        public DateTime RKHActDate { get; set; }
    }
}
