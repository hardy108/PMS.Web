using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ExceptionNoMenuAccess:Exception
    {
        public ExceptionNoMenuAccess(string menu) : base("Anda tidak memiliki access ke menu " + menu)
        {

        }
    }
}
