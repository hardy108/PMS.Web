using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class MEMPLOYEE
    {
        [NotMapped()]
        public string BANKID { get; set; }
        [NotMapped()]
        public string BANKACCNO { get; set; }
        [NotMapped()]
        public string BANKACCNAME { get; set; }
        [NotMapped()]
        public bool BPJSJKK { get; set; }
        [NotMapped()]
        public bool BPJSJHT { get; set; }
        [NotMapped()]
        public bool BPJSJP { get; set; }
        [NotMapped()]
        public string BPJSKESEHATANNO { get; set; }
        [NotMapped()]
        public string BPJSKESEHATANET { get; set; }
        [NotMapped()]
        public string BPJSKETENAGAKERJAANNO { get; set; }
        [NotMapped()]
        public string BPJSKETENAGAKERJAANNPP { get; set; }
        [NotMapped()]
        public decimal BPJSBASE { get; set; }
        [NotMapped()]
        public decimal BPJSKESEHATANBASE { get; set; }

        [NotMapped()]
        public string STATUSNAME { get; set; }
        [NotMapped()]
        public string TAXSTATUS { get; set; }
        [NotMapped()]
        public string FAMILYSTATUS { get; set; }
        [NotMapped()]
        public string STATUSTEXT
        {
            get
            {
                if (STATUS == "A")
                    return "Active";
                if (STATUS == "C")
                    return "Non-Active";
                if (STATUS == "D")
                    return "Deleted";
                return STATUS;
            }
        }

        [NotMapped()]
        public string UPDATED_IN_TEXT
        {
            get
            {
                return UPDATED.ToString("yyyy-MMM-dd HH:mm:ss");
            }
        }

        [NotMapped()]
        public string CREATED_IN_TEXT
        {
            get
            {
                return CREATED.ToString("yyyy-MMM-dd HH:mm:ss");
            }
        }

        [NotMapped()]
        public EmployeeChangePermission EmployeeChangePermission { get; set; }
        

    }
}
