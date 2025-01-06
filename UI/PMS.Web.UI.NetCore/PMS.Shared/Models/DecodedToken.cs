using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace PMS.Shared.Models
{
    public class DecodedToken
    {
        public string TokenString { get; set; }
        public string UserName { get; set; }

        public string FullName { get; set; }
        public string Session { get; set; }

        public bool ForResetPassword { get; set; }

        public bool ChangePassword { get; set; }

        public long Expiration { get; set; }

        public string Key { get; set; }
        public List<Claim> Claims { get; set; }

        public string Purpose { get; set; }

        public DecodedToken()
        {
            Claims = new List<Claim>();
        }

    }
}
