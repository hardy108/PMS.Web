using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class MSTATUS
    {
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string ABSENSEX_IN_TEXT
        {
            get
            {
                switch (ABSENSEX)
                {
                    case "P":
                        return "Perempuan";
                    case "L":
                        return "Laki-Laki";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
