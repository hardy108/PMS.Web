using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    //Not Related To DB
    public class VBKUACTIVITY
    {
        
        public string ACTID { get; set; }                
        public string ACTNAME { get; set; }        
        public string UOM { get; set; }
    }

    public class VBKUMATERIAL
    {
        
        public string MATID { get; set; }
        public string MATNAME { get; set; }
        public string UOM { get; set; }
    }

    public class VBKUEMPLOYEE
    {
        
        public string EMPID { get; set; }
        public string EMPNAME { get; set; }

    }

 


}
