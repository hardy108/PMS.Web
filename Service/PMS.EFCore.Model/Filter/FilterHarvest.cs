using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    
    public class FilterHarvest:GeneralFilter
    {
        public short HarvestType { get; set; }
        public List<short> HarvestTypes { get; set; }
        public short PaymentType { get; set; }
        public List<short> PaymentTypes { get; set; }

        public FilterHarvest() : base()
        {
            HarvestTypes = new List<short>();
            PaymentTypes = new List<short>();
            HarvestType = -1;
            PaymentType = -1;
        }
    }
}
