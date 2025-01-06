using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterPeriod : GeneralFilter
    {
        public string Active2 { get; set; }
        public string UnitCode { get; set; }
        public short? Month { get; set; }
        public short? Year { get; set; }
        public DateTime dateTime { get; set; }
        public bool? IsActive2
        {
            get
            {
                try
                {
                    if (Active2.Equals("true"))
                        return true;
                    if (Active2.Equals("false"))
                        return false;
                    return null;
                }
                catch { return true; }
            }
        }
    }
}
