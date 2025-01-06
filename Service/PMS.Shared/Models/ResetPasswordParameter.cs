using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ResetPasswordParameter
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
