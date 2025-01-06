using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    
    public class FilterReport:GeneralFilter
    {
        
        public string Format { get; set; }//PDF, HTML
        public bool Inline { get; set; }
        public string Type { get; set; }
        public bool PaymentType { get; set; }
    }
}
