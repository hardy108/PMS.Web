using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class UserAccess
    {
        public string MENUCODE { get; set; }
        public bool FADD { get; set; }
        public bool FDEL { get; set; }
        public bool FEDIT { get; set; }
        public bool FAPPR { get; set; }
        public bool FCANCEL { get; set; }
        public bool ACTIVE { get; set; }
    }
}
