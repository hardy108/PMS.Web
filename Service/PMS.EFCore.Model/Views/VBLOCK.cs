using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class VBLOCK
    {
        public string BLOCKID { get; set; }
        public string DIVID { get; set; }
        public short BLNTANAM { get; set; }
        public short THNTANAM { get; set; }
        public string JENISBIBIT { get; set; }
        public decimal LUASBLOCK { get; set; }
        public decimal CURRENTPLANTED { get; set; }
        public string TOPOGRAPI { get; set; }
        public string KELASTANAH { get; set; }
        public decimal BJR { get; set; }
        public short SPH { get; set; }
        public short PHASE { get; set; }
        public DateTime EFFDATE { get; set; }
        public string ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public bool? ACTIVE { get; set; }
        public string CREATEBY { get; set; }
        public DateTime? CREATED { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime? UPDATED { get; set; }
        public string COSTCTR { get; set; }
        public string WBS { get; set; }
        public string DIVNAME { get; set; }
        public string UNITCODE { get; set; }
        public string UNITNAME { get; set; }
    }

}
