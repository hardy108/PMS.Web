using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class Report
    {
        public string ReportID { get; set; }
        public string ReportName { get; set; }
        public string ParentID { get; set; }
        public string ReportPath { get; set; }
        public string ReportType { get; set; }
        public string FilterJson { get; set; }
        public List<string> MandatoryFilterItems { get; set; }
        public bool NeedAUthentication { get { return !string.IsNullOrWhiteSpace(AMPermission); } }
        public string AMPermission { get; set; }
        public string FilterValidation { get; set; }
        public string FilterForm { get; set; }

        public FilterJson FilterRows { get; set; }
        public int DisplayOrder { get; set; }

    }
}
