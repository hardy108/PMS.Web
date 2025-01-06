using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class MPERIOD
    {
        public string PERIODNAME { get { return TO2.ToString("MMMM yyyy"); } }
    }
}
