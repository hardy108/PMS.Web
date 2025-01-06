using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TATTENDANCEPROBLEM
    {
        [NotMapped()]
        public string UPDATED_IN_TEXT
        {
            get
            {
                return UPDATED.ToString("dd-MMM-yyyy HH:mm:ss");
            }
        }
    }
}
