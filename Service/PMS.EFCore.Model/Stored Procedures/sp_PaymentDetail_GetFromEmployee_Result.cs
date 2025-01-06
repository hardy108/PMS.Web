using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;

namespace PMS.EFCore.Model
{
    public partial class PMSContextBase
    {
        public IEnumerable<VTPAYMENTDETAIL> sp_PaymentDetail_GetFromEmployee(string unitCode, DateTime startDate, DateTime endDate)
        {
            IEnumerable<VTPAYMENTDETAIL> rows = null;
            this.LoadStoredProc("sp_PaymentDetail_GetFromEmployee")
                .AddParam("UnitCode", unitCode)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .Exec(r => rows = r.ToList<VTPAYMENTDETAIL>());
            return rows;
        }

        public IEnumerable<TPAYMENTDETAIL> sp_TPAYMENTDETAIL_GetFromEmployee_Result(string unitCode, DateTime startDate, DateTime endDate)
        {
            IEnumerable<VTPAYMENTDETAIL> rows = null;
            IEnumerable<TPAYMENTDETAIL> rowsx = null;

            this.LoadStoredProc("sp_PaymentDetail_GetFromEmployee")
                .AddParam("UnitCode", unitCode)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .Exec(r => rows = r.ToList<VTPAYMENTDETAIL>());

            rowsx = rows.Select(s => new TPAYMENTDETAIL
            {
                DOCNO = s.DOCNO,
                EMPID = s.EMPID,
                EMPCODE = s.EMPCODE,
                COSTCENTER = s.COSTCENTER,
                BASICWAGES = s.BASICWAGES,
                BASICWAGESBRUTO = s.BASICWAGESBRUTO,
                PREMIPANEN = s.PREMIPANEN,
                PREMIPANENKONTAN = s.PREMIPANENKONTAN,
                PREMINONPANEN = s.PREMINONPANEN,
                PREMIHADIR = s.PREMIHADIR,
                PENALTY = s.PENALTY,
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
                OVERTIME = s.OVERTIME,
                RICEPAIDASMONEY = s.RICEPAIDASMONEY,
                NATURA = s.NATURA,
                NATURAINCOME = s.NATURAINCOME,
                NATURAINCOMEEMPLOYEE = s.NATURAINCOMEEMPLOYEE,
                NATURADEDUCTION = s.NATURADEDUCTION,
                TAXINCENTIVE = s.TAXINCENTIVE,
                INCENTIVE = s.INCENTIVE,
                PERIOD1 = s.PERIOD1,
                JAMSOSTEKINCENTIVE = s.JAMSOSTEKINCENTIVE,
                JAMSOSTEKDEDUCTION = s.JAMSOSTEKDEDUCTION,
                KOPERASI = s.KOPERASI,
                TAX = s.TAX,
                DEBIT = s.DEBIT,
                SPSI = s.SPSI,
                TOTALSALARY = s.TOTALSALARY,
                EMPNAME = s.EMPNAME,
                EMPTYPE = s.EMPTYPE,
                POSITIONID = s.POSITIONID,
                JOINTDATE = s.JOINTDATE,
                FAMILYSTATUS = s.FAMILYSTATUS,
                TAXSTATUS = s.TAXSTATUS,
                STATUSID = s.STATUSID,
                REMAININGLEAVE = s.REMAININGLEAVE,
                LEAVE = s.LEAVE,
                GOLONGAN = s.GOLONGAN,
                SUPERVISORID = s.SUPERVISORID
            }).ToList();
            return rowsx;
        }
    }
}
