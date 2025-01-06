using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterRKH: GeneralFilter
    {
        public string UpkeepID { get; set; }
        public string ActivityID { get; set; }
        public DateTime ActualDate { get; set; }
        public short PaymentType { get; set; }
    }
}
