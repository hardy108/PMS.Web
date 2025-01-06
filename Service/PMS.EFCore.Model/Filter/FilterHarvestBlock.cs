using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    
    public class FilterHarvestBlock:FilterHarvestEmployee
    {
        public string BlockID { get; set; }
        public List<string> BlockIDs { get; set; }

        

        public FilterHarvestBlock() : base()
        {
            BlockIDs = new List<string>();
        }
    }
}
