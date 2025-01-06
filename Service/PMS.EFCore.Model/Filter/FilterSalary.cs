using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterSalary : GeneralFilter
    {
        public string DOCNO { get; set; }
        public string TYPE { get; set; }
        public string EMPID { get; set; }
        public bool? AUTO { get; set; }
        public string STATUS { get; set; }
    }
}
