using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class MSCHEDULEEMPLOYEE
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
        public string HOLIDAYS_IN_TEXT
        {
            get
            {
                switch (HOLIDAY)
                {
                    case true:
                        return "Ya";
                    case false:
                        return "Tidak";
                    default:
                        return string.Empty;
                }
            }
        }
        [NotMapped]
        public string DATE_IN_TEXT { get { return DATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
    }
}

