using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class TUPKEEPMATERIALSUM
    {
        public string UPKEEPCODE { get; set; }
        public string MATERIALID { get; set; }
        public string MATERIALNAME { get; set; }
        public decimal QUANTITY { get; set; }
        public string UOM { get; set; }
        
        public bool REQBATCH { get; set; }
        public string BATCHID { get; set; }
        public decimal STOCK { get; set; }
    }
}
