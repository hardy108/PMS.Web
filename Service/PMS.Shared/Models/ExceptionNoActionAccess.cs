using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ExceptionNoActionAccess:Exception
    {
        public ExceptionNoActionAccess(string action) : base("Anda tidak memiliki akses untuk melakukan " + action)
        { 
        }
    }
}
