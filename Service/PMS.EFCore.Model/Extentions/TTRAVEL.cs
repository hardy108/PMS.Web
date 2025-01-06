using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class TTRAVEL
    {
        [NotMapped]
        public string EMPLOYEENAME
        {
            get
            {
                if (this.EMPLOYEE != null)
                    return this.EMPLOYEE.EMPNAME;
                return string.Empty;
            }
        }
        
        [NotMapped]
        public string STARTDATE_IN_TEXT { get { return STARTDATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
    }
}

