using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ChangePasswordParameter
    {

        List<int> UserIds { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

        public ChangePasswordParameter()
        {
            UserIds = new List<int>();
        }
    }
}
