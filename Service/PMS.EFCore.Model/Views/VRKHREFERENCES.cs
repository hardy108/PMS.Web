using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    //Not Related To DB
    public class VRKHACTIVITY
    {
        
        public string ACTID { get; set; }                
        public string ACTNAME { get; set; }        
        public string UOM { get; set; }
    }

    public class VRKHMATERIAL
    {
        
        public string MATID { get; set; }
        public string MATNAME { get; set; }
        public string UOM { get; set; }
    }

    public class VRKHMANDOR
    {
        
        public string SPVID { get; set; }
        public string SPVNAME { get; set; }
    }

    public class VRKHBLOCK
    {
        
        public string BLOCKID { get; set; }
        public string BLOCKCODE { get; set; }
        public decimal LUASBLOCK { get; set; }
        public short THNTANAM { get; set; }
        public short SPH { get; set; }
        public string TOPOGRAFI { get; set; }
    }


}
