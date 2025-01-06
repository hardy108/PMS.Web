using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class VTPAYMENTDETAIL
    {
        public string DOCNO { get; set; }
        public string EMPID { get; set; }
        public string EMPCODE { get; set; }
        public string COSTCENTER { get; set; }
        public decimal BASICWAGES { get; set; }
        public decimal BASICWAGESBRUTO { get; set; }
        public decimal PREMIPANEN { get; set; }
        public decimal PREMIPANENKONTAN { get; set; }
        public decimal PREMINONPANEN { get; set; }
        public decimal PREMIHADIR { get; set; }
        public decimal PENALTY { get; set; }
        public int DAYS { get; set; }
        public int HOLIDAY { get; set; }
        public int SUNDAY { get; set; }
        public int PRESENT { get; set; }
        public decimal HK { get; set; }
        public int HKC { get; set; }
        public int HKH1 { get; set; }
        public int HKH2 { get; set; }
        public int HKP1 { get; set; }
        public int HKP2 { get; set; }
        public int HKP3 { get; set; }
        public int HKP4 { get; set; }
        public int HKS1 { get; set; }
        public int HKS2 { get; set; }
        public int MANGKIR { get; set; }
        public decimal OVERTIMEHOUR { get; set; }
        public decimal OVERTIME { get; set; }
        public string RICEPAIDASMONEY { get; set; }
        public decimal NATURA { get; set; }
        public decimal NATURAINCOME { get; set; }
        public decimal NATURAINCOMEEMPLOYEE { get; set; }
        public decimal NATURADEDUCTION { get; set; }
        public decimal TAXINCENTIVE { get; set; }
        public decimal INCENTIVE { get; set; }
        public int PERIOD1 { get; set; }
        public decimal JAMSOSTEKINCENTIVE { get; set; }
        public decimal JAMSOSTEKDEDUCTION { get; set; }
        public decimal KOPERASI { get; set; }
        public decimal TAX { get; set; }
        public decimal DEBIT { get; set; }
        public decimal SPSI { get; set; }
        public decimal TOTALSALARY { get; set; }
        public string EMPNAME { get; set; }
        public string EMPTYPE { get; set; }
        public string TYPECODE { get; set; }
        public string POSITIONID { get; set; }
        public DateTime JOINTDATE { get; set; }
        public string FAMILYSTATUS { get; set; }
        public string TAXSTATUS { get; set; }
        public string STATUSID { get; set; }
        public short REMAININGLEAVE { get; set; }
        public short LEAVE { get; set; }
        public string GOLONGAN { get; set; }
        public string SUPERVISORID { get; set; }
        public int NONPWP { get; set; }
        public bool BPJSKES { get; set; }
        public bool? BPJSJKK { get; set; }
        public bool? BPJSJHT { get; set; }
        public bool? BPJSJP { get; set; }
        public bool NATURACALC { get; set; }
        public int RESIGN { get; set; }
        public decimal BPJSBASE { get; set; }
        public decimal BPJSKESBASE { get; set; }
    }
}
