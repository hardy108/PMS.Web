using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ExceptionMessage
    {
        public string Message { get; set; }

        public ExceptionMessage() { }

        public ExceptionMessage(string message)
        {
            Message = message;
        }

        public ExceptionMessage(Exception exception)
        {
            Message = GetAllExceptionMessage(exception);
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
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
