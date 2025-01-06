using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterUser:GeneralFilter
    {
        public string AliasOrEmail { get; set; }
        public string Alias { get; set; }
        public List<string> Aliases { get; set; }

        public string Email { get; set; }
        public List<string> Emails { get; set; }
        public FilterUser() : base() 
        {
            Aliases = new List<string>();
            Emails = new List<string>();
        }

    }
}
