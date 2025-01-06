using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public class USERACCESS
    {
        public string USERNAME { get; set; }
        public string MENUCODE { get; set; }
        public bool FADD { get; set; }
        public bool FDEL { get; set; }
        public bool FEDIT { get; set; }
        public bool FAPPR { get; set; }
        public bool FCANCEL { get; set; }
        public bool ACTIVE { get; set; }

        [NotMapped()]
        public List<string> AllPermissions { get; set; }


        [NotMapped()]
        public List<string> CustomPermissions { get; set; }

        public USERACCESS()
        {
            AllPermissions = new List<string>();
            CustomPermissions = new List<string>();
        }

    }
}
