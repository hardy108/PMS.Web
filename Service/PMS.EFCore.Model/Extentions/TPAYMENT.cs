using PMS.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class TPAYMENT
    {
        [NotMapped]
        public string CODE
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(DOCNO))
                { return DOCNO.Replace(@"/", ""); }
                else
                    return string.Empty;
            }
        }
        [NotMapped]
        public string TOTALFormatted
        {
            get { return string.Format("{0:N}", TOTAL); }
        }

        [NotMapped]
        public string DATE_IN_TEXT { get { return DOCDATE.ToString(StandardFormats.DATE_FORMAT_DISPLAY); } }
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
        public List<VTHARVESTRESULT1> VTHARVESTRESULT1 { get; set; }
        [NotMapped]
        public List<VTHARVESTRESULT1> VTHARVESTRESULT1GERDAN { get; set; }

        [NotMapped]
        public List<VTPAYMENTDETAIL> VTPAYMENTDETAIL { get; set; }
        [NotMapped]
        public List<TPAYMENTTAX> TPAYMENTTAX { get; set; }
        [NotMapped]
        public List<TPAYMENTSCHEME> VTPAYMENTSCHEME { get; set; }

        [NotMapped]
        public List<THARVESTRESULT1> THARVESTRESULT1 { get; set; }
        [NotMapped]
        public List<TGERDANRESULT> TGERDANRESULT { get; set; }
        [NotMapped]
        public List<TLOADINGRESULT> TLOADINGRESULT { get; set; }
        [NotMapped]
        public List<TOPERATINGRESULT> TOPERATINGRESULT { get; set; }


        public void InitEditDaily()
        {
            VTHARVESTRESULT1 = new List<VTHARVESTRESULT1>();
            VTHARVESTRESULT1GERDAN = new List<VTHARVESTRESULT1>();

            THARVESTRESULT1 = new List<THARVESTRESULT1>();
            TGERDANRESULT = new List<TGERDANRESULT>();
            TLOADINGRESULT = new List<TLOADINGRESULT>();
            TOPERATINGRESULT = new List<TOPERATINGRESULT>();
        }

        public void InitEditMonthly()
        {
            VTPAYMENTDETAIL = new List<VTPAYMENTDETAIL>();
            VTPAYMENTSCHEME = new List<TPAYMENTSCHEME>();

            TPAYMENTTAX = new List<TPAYMENTTAX>();
            THARVESTRESULT1 = new List<THARVESTRESULT1>();
            TGERDANRESULT = new List<TGERDANRESULT>();
            TLOADINGRESULT = new List<TLOADINGRESULT>();
            TOPERATINGRESULT = new List<TOPERATINGRESULT>();
        }


    }
}
