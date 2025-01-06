using System;
using System.Collections.Generic;
using System.Text;
using PMS.EFCore.Helper;

namespace AM.EFCore.Models.Filters
{
    public class FilterUser : GeneralPagingFilter
    {
        public int Id { get; set; }
        public List<int> Ids { get; set; }
        public string UsernameOrEmail { get; set; }
        public string Username { get; set; }        
        public List<string> Aliases { get; set; }

        public string Email { get; set; }
        public List<string> Emails { get; set; }

        public string Active { get; set; }

        public bool? IsActive
        {
            get
            {

                try
                {
                    if (!string.IsNullOrWhiteSpace(Active))
                    {
                        if (Active.Equals("true"))
                            return true;
                        if (Active.Equals("false"))
                            return false;
                    }
                    return null;
                }
                catch { return null; }
            }
        }
        public FilterUser() : base()
        {
            Aliases = new List<string>();
            Emails = new List<string>();
            Ids = new List<int>();
        }

    }
}
