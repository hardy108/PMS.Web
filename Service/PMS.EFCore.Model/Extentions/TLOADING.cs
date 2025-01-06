using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class TLOADING
    {
        [NotMapped]
        public string DATE_IN_TEXT { get { return LOADINGDATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
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
        public string STATUSUPLOAD_IN_TEXT
        {
            get
            {
                switch (UPLOAD)
                {
                    case 0:
                        return "Not Posted";
                    case 3:
                        return "Posted";
                    default:
                        return string.Empty;
                }
            }
        }
        [NotMapped]
        public string LOADINGPAYMENTTYPE_IN_TEXT
        {
            get
            {
                switch (LOADINGPAYMENTTYPE)
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
