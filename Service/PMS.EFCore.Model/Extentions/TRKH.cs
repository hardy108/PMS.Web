using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class TRKH
    {
        [NotMapped]
        public string DATE_IN_TEXT { get { return DATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
        [NotMapped]
        public string ACTDATE_IN_TEXT { get { return ACTDATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
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
        [NotMapped]
        public string PAYMENTTYPE_IN_TEXT
        {
            get
            {
                switch (PAYMENTTYPE)
                {
                    case 0:
                        return "Harian";
                    case 1:
                        return "Kontanan";
                    case 2:
                        return "Borongan";
                    default:
                        return string.Empty;
                }
                
            }
        }
    }
}
