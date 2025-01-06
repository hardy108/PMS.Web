using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class VDIVISI
    {
        public string DIVID { get; set; }
        public string UNITCODE { get; set; }
        public string DIVASISTEN { get; set; }
        public string DIVASKEP { get; set; }
        public string ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public bool? ACTIVE { get; set; }
        public string CREATEBY { get; set; }
        public DateTime? CREATED { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime? UPDATED { get; set; }
        public string FULLNAME { get; set; }
    }

}
