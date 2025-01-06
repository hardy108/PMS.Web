using PMS.EFCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    

    public partial class PMSContextBase
    {
        class sp_Employee_GenerateNewPin_Result
        {
            public int PIN { get; set; }
        }
        public int sp_Employee_GenerateNewPin()
        {
            
            sp_Employee_GenerateNewPin_Result row = null;
            this.LoadStoredProc("sp_Employee_GenerateNewPin")
                .Exec(r => row = r.FirstOrDefault<sp_Employee_GenerateNewPin_Result>());                
            if (row == null)
                throw new Exception("No PIN available");
            return row.PIN;
        }
    }
}
