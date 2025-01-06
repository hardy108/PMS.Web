using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterGoodIssue : GeneralFilter
    {
        public string No { get; set; }
        public string VoucherNo { get; set; }
    }
}
