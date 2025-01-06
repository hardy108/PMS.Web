using PMS.Shared;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TSALARYITEM
    {
        [NotMapped()]
        public string DATE_IN_TEXT { get { return DATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }

        [NotMapped()]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }

        [NotMapped]
        public string STATUS_IN_TEXT
        {
            get
            {
                return StandardUtility.GetRecordStatusDescription(STATUS);
            }
        }

        [NotMapped()]
        public string EMPNAME { get; set; }

        public TSALARYITEM()
        {
        }

        
        public TSALARYITEM(TSALARYITEM mandorFine)
        {
            this.CopyFrom(mandorFine);        
        }
    }
}
