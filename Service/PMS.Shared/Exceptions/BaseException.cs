using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Exceptions
{
    public class BaseException : Exception
    {
        protected int _code { get; set; }

        public int Code { get { return _code; } }
        public BaseException() : base()
        {
            _code = -1;
        }

        public BaseException(string message) : base(message)
        {
            _code = -1;
        }

        public BaseException(string message, int code) : base(message)
        {
            _code = code;
        }
    }
}
