using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterBlock:GeneralFilter
    {
        public FilterBlock() : base()
        {
            BlockIDs = new List<string>();
        }
        public string BlockID { get; set; }
        public List<string> BlockIDs { get; set; }
    }
}
