using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public class EmployeeChangePermission
    {
        [NotMapped()]
        public bool ALLOWEDIT { get; set; }
        [NotMapped()]
        public bool ALLOWEDITBANK { get; set; }
        [NotMapped()]
        public bool ALLOWEDITBPJS { get; set; }
        [NotMapped()]
        public bool ALLOWEDITABSENSI { get; set; }
    }
}
