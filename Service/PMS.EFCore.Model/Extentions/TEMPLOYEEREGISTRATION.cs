using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.EFCore.Model
{
    public partial class TEMPLOYEEREGISTRATION
    {

        [NotMapped()]
        public string UPDATED_IN_TEXT
        {
            get
            {
                
                return UPDATED.ToString("yyyy-MMM-dd HH:mm:ss");
            }
        }

        [NotMapped()]
        public string CREATED_IN_TEXT
        {
            get
            {
                return CREATED.ToString("yyyy-MMM-dd HH:mm:ss");
            }
        }
    }
}
