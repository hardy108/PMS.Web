using PMS.EFCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace WF.EFCore.Models.Filters
{
    public class FilterWorkflow:GeneralPagingFilter
    {

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> DocTypes { get; set; }

        public string DocType { get; set; }

        public string UnitID { get; set; }

        public List<string> UnitIDs { get; set; }
        public string UserName { get; set; }

        public FilterWorkflow()
        {
            DocTypes = new List<string>();
            StartDate = new DateTime(1900, 1, 1);
            EndDate = new DateTime(4999, 12, 31);
            UnitIDs = new List<string>();
        }
    }
}
