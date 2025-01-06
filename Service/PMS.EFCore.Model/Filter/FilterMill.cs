using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterMill : GeneralFilter
    {
        public string MillCode { get; set; }
        public string MillName { get; set; }
    }
}
