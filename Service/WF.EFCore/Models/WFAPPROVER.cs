using AM.EFCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace WF.EFCore.Models
{
    public class WFAPPROVER
    {
        public VAMUSER VAMUSER { get; set; }
        public bool IsApprover { get; set; }
    }
}
