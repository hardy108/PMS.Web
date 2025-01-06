using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterStock: GeneralFilter
    {
        public string LOCCODE { get; set; }
        public string MATERIALID { get; set; }
    }
}
