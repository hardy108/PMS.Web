using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterPremiPanen: FilterBlock
    {
        public FilterPremiPanen() : base()
        {
            EmployeeTypes = new List<string>();
        }
        public string EmpoyeeType { get; set; }
        public List<string> EmployeeTypes { get; set; }
    }
}
