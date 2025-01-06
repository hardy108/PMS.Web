using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    
    public class FilterHarvestEmployee: FilterHarvest
    {
        public string EmployeeID { get; set; }
        public List<string> EmployeeIDs { get; set; }


        public FilterHarvestEmployee() : base()
        {
            EmployeeIDs = new List<string>();
            
        }
    }
}
