using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Exceptions
{
    public class ExceptionConfiguration:BaseException
    {
        public ExceptionConfiguration(string configkey) : base($"Invalid configuration of {configkey}, please contact the administrator")
        {
            _code = 8001;
        }
    }
}
