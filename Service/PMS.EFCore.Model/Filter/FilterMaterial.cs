using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterMaterial : GeneralFilter
    {
        public string UOM { get; set; }
        public string AccountCode { get; set; }
    }
}


