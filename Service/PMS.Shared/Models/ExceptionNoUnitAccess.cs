using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ExceptionNoUnitAccess:Exception
    {
        public ExceptionNoUnitAccess(string unit) : base("Anda tidak memiliki access ke unit " + unit)
        {

        }
    }
}
