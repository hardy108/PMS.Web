using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class TCALENDAR
    {
        [NotMapped]
        public string DTDATE_IN_TEXT { get { return DTDATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
        [NotMapped]
        public string DAYS_IN_TEXT
        {
            get
            {
                switch (HOLIDAY)
                {
                    case true:
                        return "Hari Besar";
                    case false:
                        return "Hari Minggu";
                    default:
                        return string.Empty;
                }
            }
        }
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
    }
}
