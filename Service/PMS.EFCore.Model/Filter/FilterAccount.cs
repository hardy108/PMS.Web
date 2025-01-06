using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterAccount:GeneralFilter
    {
        public int TypeId { get; set; }
        public bool? Parent { get; set; }
        public string ParentCode { get; set; }
        public string AccountCode { get; set; }
    }
}
