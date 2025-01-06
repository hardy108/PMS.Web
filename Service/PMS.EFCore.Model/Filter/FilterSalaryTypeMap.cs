using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterSalaryTypeMap : GeneralFilter
    {
        public string unitId { get; set; }
        public string employeeCode { get; set; }
        public string premiId { get; set; }
    }
}
