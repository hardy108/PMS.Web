using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TITINVOICE
    {
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string CREATED_IN_TEXT { get { return CREATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string REGISTERDATE_IN_TEXT { get { return REGISTERDATE.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string INVOICEDATE_IN_TEXT { get { return INVOICEDATE.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string DUEDATE_IN_TEXT { get { return DUEDATE.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
    }
}
