using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Exceptions
{
    public class ExceptionNoAuthorization : BaseException
    {
        public ExceptionNoAuthorization() : base("You are not authorized")
        {
            _code = 3001;
        }
    }
    public class ExceptionNoActionAccess:BaseException
    {
        public ExceptionNoActionAccess(string action) : base("You are not authorized to " + action)
        {
            _code = 3002;
        }
    }

    public class ExceptionNoMenuAccess : BaseException
    {
        public ExceptionNoMenuAccess(string menu) : base("You are not authorized to acces " + menu + " menu")
        {
            _code = 3003;
        }
    }

    public class ExceptionNoReportAccess : BaseException
    {
        public ExceptionNoReportAccess(string report) : base("You are not authorized to acces " + report + " report")
        {
            _code = 3004;
        }
    }
}
