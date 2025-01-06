using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_PaymentDetail_GetTaxYTD_Result
    {
        public string DOCNO { get; set; }
        public string EMPID { get; set; }
        public string EMPCODE { get; set; }
        public string COSTCENTER { get; set; }
        public decimal? BASICWAGES { get; set; }
        public decimal? BASICWAGESBRUTO { get; set; }
        public decimal? PREMIPANEN { get; set; }
        public decimal? PREMIPANENKONTAN { get; set; }
        public decimal? PREMINONPANEN { get; set; }
        public decimal? PREMIHADIR { get; set; }
        public decimal? PENALTY { get; set; }
        public int DAYS { get; set; }
        public int HOLIDAY { get; set; }
        public int SUNDAY { get; set; }
        public int PRESENT { get; set; }
        public int HK { get; set; }
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
        public int OVERTIMEHOUR { get; set; }
        public decimal? OVERTIME { get; set; }
        public string RICEPAIDASMONEY { get; set; }
        public int NATURA { get; set; }
        public decimal? NATURAINCOME { get; set; }
        public int NATURAINCOMEEMPLOYEE { get; set; }
        public int NATURADEDUCTION { get; set; }
        public int TAXINCENTIVE { get; set; }
        public decimal? INCENTIVE { get; set; }
        public int PERIOD1 { get; set; }
        public int JAMSOSTEKINCENTIVE { get; set; }
        public decimal? JAMSOSTEKDEDUCTION { get; set; }
        public int KOPERASI { get; set; }
        public decimal? TAX { get; set; }
        public int DEBIT { get; set; }
        public int SPSI { get; set; }
        public int TOTALSALARY { get; set; }
        public string EMPNAME { get; set; }
        public string EMPTYPE { get; set; }
        public string TYPECODE { get; set; }
        public string POSITIONID { get; set; }
        public int? JOINTDATE { get; set; }
        public string FAMILYSTATUS { get; set; }
        public string TAXSTATUS { get; set; }
        public string STATUSID { get; set; }
        public int REMAININGLEAVE { get; set; }
        public int LEAVE { get; set; }
        public string GOLONGAN { get; set; }
        public string SUPERVISORID { get; set; }
        public int NONPWP { get; set; }
        public int BPJSKES { get; set; }
        public int BPJSJKK { get; set; }
        public int BPJSJHT { get; set; }
        public int BPJSJP { get; set; }
        public int NATURACALC { get; set; }
        public int RESIGN { get; set; }
        public int BPJSBASE { get; set; }
        public int BPJSKESBASE { get; set; }

    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_PaymentDetail_GetTaxYTD_Result> sp_PaymentDetail_GetTaxYTD_Result(string UnitId, DateTime Date)
        {
            IEnumerable<sp_PaymentDetail_GetTaxYTD_Result> rows = null;
            this.LoadStoredProc("sp_PaymentDetail_GetTaxYTD")
                .AddParam("UnitId", UnitId)
                .AddParam("Date", Date)
                .Exec(r => rows = r.ToList<sp_PaymentDetail_GetTaxYTD_Result>());
            return rows;
        }

        public IEnumerable<TPAYMENTDETAIL> sp_PaymentDetail_GetTaxYTD_TPAYMENTDETAILResult(string UnitId, DateTime Date)
        {
            IEnumerable<sp_PaymentDetail_GetTaxYTD_Result> rows = null;
            IEnumerable<TPAYMENTDETAIL> rowsx = null;

            this.LoadStoredProc("sp_PaymentDetail_GetTaxYTD")
                .AddParam("UnitId", UnitId)
                .AddParam("Date", Date)
                .Exec(r => rows = r.ToList<sp_PaymentDetail_GetTaxYTD_Result>());
            rowsx = rows.Select(s => new TPAYMENTDETAIL
            {
                DOCNO = s.DOCNO,
                EMPID = s.EMPID,
                EMPCODE = s.EMPCODE,
                COSTCENTER = s.COSTCENTER,
                BASICWAGES = Convert.ToDecimal(s.BASICWAGES),
                BASICWAGESBRUTO = Convert.ToDecimal(s.BASICWAGESBRUTO),
                PREMIPANEN = Convert.ToDecimal(s.PREMIPANEN),
                PREMIPANENKONTAN = Convert.ToDecimal(s.PREMIPANENKONTAN),
                PREMINONPANEN = Convert.ToDecimal(s.PREMINONPANEN),
                PREMIHADIR = Convert.ToDecimal(s.PREMIHADIR),
                PENALTY = Convert.ToDecimal(s.PENALTY),
                DAYS = Convert.ToInt16(s.DAYS),
                HOLIDAY = Convert.ToInt16(s.HOLIDAY),
                SUNDAY = Convert.ToInt16(s.SUNDAY),
                PRESENT = Convert.ToInt16(s.PRESENT),
                HK = s.HK,
                HKC = s.HKC,
                HKH1 = s.HKH1,
                HKH2 = s.HKH2,
                HKP1 = s.HKP1,
                HKP2 = s.HKP2,
                HKP3 = s.HKP3,
                HKP4 = s.HKP4,
                HKS1 = s.HKS1,
                HKS2 = s.HKS2,
                MANGKIR = s.MANGKIR,
                OVERTIMEHOUR = s.OVERTIMEHOUR,
                OVERTIME = Convert.ToDecimal(s.OVERTIME),
                RICEPAIDASMONEY = s.RICEPAIDASMONEY,
                NATURA = s.NATURA,
                NATURAINCOME = Convert.ToDecimal(s.NATURAINCOME),
                NATURAINCOMEEMPLOYEE = s.NATURAINCOMEEMPLOYEE,
                NATURADEDUCTION = s.NATURADEDUCTION,
                TAXINCENTIVE = s.TAXINCENTIVE,
                INCENTIVE = Convert.ToDecimal(s.INCENTIVE),
                PERIOD1 = s.PERIOD1,
                JAMSOSTEKINCENTIVE = s.JAMSOSTEKINCENTIVE,
                JAMSOSTEKDEDUCTION = Convert.ToDecimal(s.JAMSOSTEKDEDUCTION),
                KOPERASI = s.KOPERASI,
                TAX = Convert.ToDecimal(s.TAX),
                DEBIT = s.DEBIT,
                SPSI = s.SPSI,
                TOTALSALARY = s.TOTALSALARY,
                EMPNAME = s.EMPNAME,
                EMPTYPE = s.EMPTYPE,
                POSITIONID = s.POSITIONID,
                JOINTDATE = null,
                FAMILYSTATUS = s.FAMILYSTATUS,
                TAXSTATUS = s.TAXSTATUS,
                STATUSID = s.STATUSID,
                REMAININGLEAVE = Convert.ToInt16(s.REMAININGLEAVE),
                LEAVE = Convert.ToInt16(s.LEAVE),
                GOLONGAN = s.GOLONGAN,
                SUPERVISORID = s.SUPERVISORID
            }).ToList();
            return rowsx;

        }


    }
}
