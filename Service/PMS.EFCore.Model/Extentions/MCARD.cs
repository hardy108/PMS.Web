using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class MCARD
    {
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string TYPE_IN_TEXT
        {
            get
            {
                switch (TYPE)
                {
                    case "E":
                        return "Ekspedisi";
                    case "V":
                        return "Vendor";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
