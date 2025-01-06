using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class TITEMAILREQUEST
    {
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string CREATED_IN_TEXT { get { return CREATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string REGISTERDATE_IN_TEXT { get { return REGISTERDATE.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string REQUESTDATE_IN_TEXT { get { return REQUESTDATE.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }        
    }
}
