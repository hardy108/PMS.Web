using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class VEMPLOYEE
    {
        public string EMPID { get; set; }
        public string UNITCODE { get; set; }
        public string EMPCODE { get; set; }
        public string EMPNAME { get; set; }
        public string DIVID { get; set; }
        public string POSITIONID { get; set; }
        public string SUPERVISORID { get; set; }
        public string EMPTYPE { get; set; }
        public string STATUSID { get; set; }
        public decimal BASICWAGES { get; set; }
        public decimal KOPERASIDEDUCTION { get; set; }
        public string EMPSEX { get; set; }
        public decimal SALARYPRD1 { get; set; }
        public decimal SPSIDEDUCTION { get; set; }
        public DateTime BIRTHDAY { get; set; }
        public DateTime JOINTDATE { get; set; }
        public DateTime? RESIGNEDDATE { get; set; }
        public short REMAININGLEAVE { get; set; }
        public short LEAVE { get; set; }
        public string GOLONGAN { get; set; }
        public string NOSPK { get; set; }
        public string PLACEOFBIRTH { get; set; }
        public string RACE { get; set; }
        public string RELIGION { get; set; }
        public string EDUCATION { get; set; }
        public string KTPID { get; set; }
        public string KTPADDRESS { get; set; }
        public string NPWP { get; set; }
        public string KOTAASAL { get; set; }
        public string PROVINSI { get; set; }
        public string SPOUSEID { get; set; }
        public bool BPJSKES { get; set; }
        public bool NATURA { get; set; }
        public int PINID { get; set; }
        public string ATTENDGROUPID { get; set; }
        public string STATUS { get; set; }
        public bool TRFHO { get; set; }
        public string CREATEDBY { get; set; }
        public DateTime CREATED { get; set; }
        public string UPDATEDBY { get; set; }
        public DateTime UPDATED { get; set; }
        public string POSITIONNAME { get; set; }
        public string TYPEID { get; set; }
        public string TYPECODE { get; set; }
        public string TYPENAME { get; set; }
        public string DIVNAME { get; set; }
        public string UNITNAME { get; set; }
        public string BANKID { get; set; }
        public string BANKNO { get; set; }
        public string BANKFULLNAME { get; set; }
        public string STATUSNAME { get; set; }

        public string TAXSTATUS { get; set; }

        public string FAMILYSTATUS { get; set; }
    }

}
