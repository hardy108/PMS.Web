using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterPenaltyType: GeneralFilter
    {
        public short TYPE { get; set; }
        public string NAME { get; set; }
        public bool? ACTIVE
        {
            get
            {

                try
                {
                    if (ACTIVE.Equals("true"))
                        return true;
                    if (ACTIVE.Equals("false"))
                        return false;
                    return null;
                }
                catch { return null; }
            }
        }
       
    }
}
