
using System;
using System.Collections.Generic;

namespace PMS.EFCore.Model
{
    public partial class TLEAVE
    {
        public string ID { get; set; }
        public string UNITID { get; set; }
        public DateTime DATE { get; set; }
        public string EMPID { get; set; }
        public string TYPEID { get; set; }
        public DateTime DATEFROM { get; set; }
        public DateTime DATETO { get; set; }
        public decimal QTY { get; set; }
        public string NOTE { get; set; }
        public string STATUS { get; set; }
        public long? WFDOCTRANSNO { get; set; }
        public string WFDOCSTATUS { get; set; }
        public string WFDOCSTATUSTEXT { get; set; }
        public string WFERRORTEXT { get; set; }
        public string ATTDOCID { get; set; }
        public int PROCESS { get; set; }
        public string PROCESSSTATUS { get; set; }

        public string CREATEBY { get; set; }
        public DateTime? CREATED { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime UPDATED { get; set; }

        
    }
}