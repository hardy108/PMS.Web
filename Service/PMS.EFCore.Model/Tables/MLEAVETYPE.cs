using System;
using System.Collections.Generic;

namespace PMS.EFCore.Model
{
    public partial class MLEAVETYPE
    {
        public MLEAVETYPE()
        {
            
        }

        public string ID { get; set; }
        public string NAME { get; set; }
        public string NOTE { get; set; }
        public string ABSENTCODE { get; set; }
        public string SEX { get; set; }
        public bool LIMITED { get; set; }
        public short LIMIT { get; set; }

        
    }
}