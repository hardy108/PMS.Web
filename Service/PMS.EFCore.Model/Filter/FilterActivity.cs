using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterActivity : GeneralFilter
    {
        
        public bool RA { get; set; }
        public bool CE { get; set; }
        public bool GA { get; set; }
        public bool Nursery { get; set; }
        public bool LC { get; set; }
        public bool TBM { get; set; }
        public bool TM { get; set; }
        public bool HV { get; set; }
        public int? HVYTPE { get; set; }
        public string RFID { get; set; }
        public int? AUTO { get; set; }

    }
}
