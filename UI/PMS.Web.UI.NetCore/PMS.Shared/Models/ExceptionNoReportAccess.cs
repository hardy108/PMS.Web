using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ExceptionNoReportAccess:Exception
    {
        public ExceptionNoReportAccess(string report) : base("Anda tidak memiliki access ke report " + report)
        {

        }
    }
}
