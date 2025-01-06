using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class VCOMPANY
    {
        public string LEGALID { get; set; }
        public string NPWP { get; set; }
        public string ADDR1 { get; set; }
        public string ADDR2 { get; set; }
        public string ADDR3 { get; set; }
        public string POSTALCODE { get; set; }
        public byte[] LOGO { get; set; }
        public string ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public bool? ACTIVE { get; set; }
        public string CREATEBY { get; set; }
        public DateTime? CREATED { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime? UPDATED { get; set; }
    }

}
