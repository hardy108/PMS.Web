using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    //Not Related To DB

    public class VLOADINGBLOCK
    {
        public short THNTANAM { get; set; }
        public string BLOCKID { get; set; }
        public decimal LUASBLOCK { get; set; }
        public decimal KG { get; set; }
   }

    public class VLOADINGEMPLOYEE
    {
        public string EMPID { get; set; }
        public string EMPCODE { get; set; }
        public string EMPNAME { get; set; }
        public string EMPTYPE { get; set; }      
    }

}
