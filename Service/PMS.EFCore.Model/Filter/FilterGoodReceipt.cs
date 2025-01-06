using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterGoodReceipt : GeneralFilter
    {
        public string No { get; set; }
        public string VoucherNo { get; set; }
        public string VendorCode { get; set; }
        public string ExpCode { get; set; }
    }
}
