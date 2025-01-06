using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Exceptions
{
    public class ExceptionMessage
    {
        public int Code { get; set; }
        public string Message { get; set; }
        

        public ExceptionMessage() { }

        public ExceptionMessage(string message)
        {
            Message = $"[{DateTime.Now:dd-MMM-yyyy HH:mm:ss}]{message}";
            Code = -1;
        }

        

        public ExceptionMessage(BaseException exception)
        {
            Message = $"[{DateTime.Now:dd-MMM-yyyy HH:mm:ss}]{GetAllExceptionMessage(exception)}";            
            Code = exception.Code;
        }

        public ExceptionMessage(Exception exception)
        {
            Message = $"[{DateTime.Now:dd-MMM-yyyy HH:mm:ss}]{GetAllExceptionMessage(exception)}";
            Code = -1;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string GetAllExceptionMessage(BaseException ex)
        {
            if (ex.InnerException == null)
                return ex.Message;
            else
                return ex.Message + "\r\n" + GetAllExceptionMessage(ex.InnerException);
        }

        public static string GetAllExceptionMessage(Exception ex)
        {
            if (ex.InnerException == null)
                return ex.Message;
            else
                return ex.Message + "\r\n" + GetAllExceptionMessage(ex.InnerException);
        }


    }
}
