using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class RKHHEADER1
    {
        [NotMapped]
        public string DATE_IN_TEXT { get { return RKH_DATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
        [NotMapped]
        public string ACTDATE_IN_TEXT { get { return RKH_ACTDATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string STATUS_IN_TEXT
        {
            get
            {
                switch (STATUS)
                {
                    case "A":
                        return "Approved";
                    case "C":
                        return "Canceled";
                    case "P":
                        return "Proces";
                    case "D":
                        return "Deleted";
                    default:
                        return string.Empty;
                }
            }
        }

    }
}
