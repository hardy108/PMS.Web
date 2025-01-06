using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterPosition:GeneralFilter
    {
        public int PositionGroupId { get; set; }
        public string PositionGroupName { get; set; }
        public bool Mandor1 { get; set; }
        
        public bool MandorTanam { get; set; }
        
        public bool MandorNonTanam { get; set; }
        
        public bool KraniTanam { get; set; }
        
        public bool KraniNonTanam { get; set; }
        
        public bool Harvester { get; set; }
        
        public bool AllPostitionFlag
        {
            get
            {
                return !Mandor1 && !MandorTanam && !MandorNonTanam && !KraniTanam && !KraniNonTanam && !Harvester;
            }
        }

        public int PositionFlag { get; set; }

        List<int> _positionFlags = new List<int>();
        public List<int> PositionFlags 
        {
            get 
            {
                List<int> additionalFlag = new List<int>();
                if (Mandor1) additionalFlag.Add(1);
                if (MandorTanam) additionalFlag.Add(2);
                if (MandorNonTanam) additionalFlag.Add(3);
                if (KraniTanam) additionalFlag.Add(4);
                if (KraniNonTanam) additionalFlag.Add(5);
                if (Harvester) additionalFlag.Add(6);
                additionalFlag.AddRange(_positionFlags);
                return additionalFlag;
            }
            set { _positionFlags = value; }
        }

    }
}
