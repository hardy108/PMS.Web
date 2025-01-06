using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TPAYMENTWFADJUSTHK
    {
        [NotMapped()]
        public string UPDATED_IN_TEXT
        {
            get
            {
                if (UPDATED.HasValue)
                    return UPDATED.Value.ToString("dd-MMM-yyyy HH:mm:ss");
                return string.Empty;
            }
        }

        [NotMapped()]
        public string DATE_IN_TEXT
        {
            get
            {
                return DATE.ToString("dd-MMM-yyyy");
            }
        }
    }
}
