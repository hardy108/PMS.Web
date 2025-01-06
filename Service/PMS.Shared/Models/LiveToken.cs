using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class SessionToken
    {
        public string TokenString { get; set; }        

        public DateTime UtcLastAccess { get; set; }

        public bool ForResetPassword { get; set; }
    }
}
