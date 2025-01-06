using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ExceptionNoAuthorization:Exception
    {
        public ExceptionNoAuthorization() : base("Belum ada otorisasi")
        { 
        }
    }
}
