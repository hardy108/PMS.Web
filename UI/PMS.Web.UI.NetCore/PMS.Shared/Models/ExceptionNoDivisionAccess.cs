using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ExceptionNoDivisionAccess:Exception
    {
        public ExceptionNoDivisionAccess(string divisi) : base("Anda tidak memiliki access ke divisi " + divisi)
        {

        }
    }
}
