using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TATTENDANCE
    {
        

        private string _unitCode = string.Empty;

        [NotMapped]
        public string UNITCODE 
        { 
            get 
            {
                if (DIV != null)
                    return DIV.UNITCODE;
                return _unitCode;
            } 
            set { _unitCode = value; }
        }


        private MEMPLOYEE _employeeforValidation = null;

        [NotMapped]
        public MEMPLOYEE EMPLOYEE_FOR_VALIDATION
        {
            get
            {
                if (EMPLOYEE != null)
                    return EMPLOYEE;
                return _employeeforValidation;
            }
            set { _employeeforValidation = value; }
        }
    }
}
