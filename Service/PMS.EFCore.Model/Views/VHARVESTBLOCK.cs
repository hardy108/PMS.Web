using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    //Not Related To DB
    
    

    public class VHARVESTBLOCK
    {
        
        public string HARVESTCODE { get; set; }
        public string BLOCKID { get; set; }

        public short THNTANAM { get; set; }

        public string BLOCKCODE { get; set; }
        public decimal LUASBLOCK { get; set; }        
        public decimal HARVESTAREA { get; set; }

        public decimal KG { get; set; }

        public decimal QTY { get; set; }
        
        public decimal QTYFINE { get; set; }

    } 


}
