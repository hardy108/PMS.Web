using PMS.Shared;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class THARVEST
    {
        [NotMapped]
        public string HARVESTDATE_IN_TEXT { get { return HARVESTDATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
        [NotMapped]
        public string UPDATED_IN_TEXT { get { return UPDATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string CREATES_IN_TEXT { get { return CREATED.ToString(StandardFormats.DATETIME_FORMAT_DISPLAY); } }
        [NotMapped]
        public string STATUS_IN_TEXT
        {
            get
            {
                return StandardUtility.GetRecordStatusDescription(STATUS);
            }
        }
        [NotMapped]
        public string HARVESTPAYMENTTYPE_IN_TEXT
        {
            get
            {
                switch (HARVESTPAYMENTTYPE)
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
        [NotMapped]
        public string HARVESTTYPE_IN_TEXT
        {
            get
            {
                switch (HARVESTTYPE)
                {
                    case 0:
                        return "Potong Buah";
                    case 1:
                        return "Kutip Brondol";                    
                    default:
                        return string.Empty;
                }

            }
        }

        [NotMapped]
        public List<THARVESTASIS> THARVESTASISEDIT { get; set; }

        [NotMapped]
        public List<THARVESTBASE> THARVESTBASEEDIT { get; set; }

        [NotMapped]
        public List<VHARVESTBLOCK> VHARVESTBLOCKEDIT { get; set; }

        public List<VHARVESTBLOCK> VHARVESTBLOCK { get; set; }

        [NotMapped]
        public List<THARVESTBLOCK> THARVESTBLOCKEDIT { get; set; }

        [NotMapped]
        public List<THARVESTCOLLECT> THARVESTCOLLECTEDIT { get; set; }

        [NotMapped]
        public List<THARVESTEMPLOYEE> THARVESTEMPLOYEEEDIT { get; set; }

        [NotMapped]
        public List<THARVESTFINE> THARVESTFINEEDIT { get; set; }

        public void InitEdit()
        {
            THARVESTASISEDIT = new List<THARVESTASIS>();
            THARVESTBASEEDIT = new List<THARVESTBASE>();
            VHARVESTBLOCKEDIT = new List<VHARVESTBLOCK>();            
            THARVESTBLOCKEDIT = new List<THARVESTBLOCK>();
            THARVESTCOLLECTEDIT = new List<THARVESTCOLLECT>();
            THARVESTEMPLOYEEEDIT = new List<THARVESTEMPLOYEE>();
            THARVESTFINEEDIT = new List<THARVESTFINE>();
        }
    }
}
