using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class BlockedToken
    {
        public string TokenString { get; set; }
        public long BlockUntil { get; set; }
    }
}
