using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_LoadingResult_GetDaily_Result
    {
        public string LOADINGCODE { get; set; }
        public DateTime? STARTDATE { get; set; }
        public DateTime? ENDDATE { get; set; }
        public DateTime LOADINGDATE { get; set; }
        public short PRODUCTID { get; set; }
        public short SPBDATATYPE { get; set; }
        public string NOSPB { get; set; }
        public string BLOCKID { get; set; }
        public string BASISGROUP { get; set; }
        public string DIVID { get; set; }
        public string KRANIID { get; set; }
        public string EMPLOYEEID { get; set; }
        public short LOADINGTYPE { get; set; }
        public string VEHICLEID { get; set; }
        public string VEHICLETYPEID { get; set; }
        public short LOADINGPAYMENTTYPE { get; set; }
        public string EMPLOYEETYPE { get; set; }
        public decimal ORIBASE1 { get; set; }
        public decimal? BASE1 { get; set; }
        public int BASE2 { get; set; }
        public int BASE3 { get; set; }
        public int BASE4 { get; set; }
        public int BASE5 { get; set; }
        public int BASE6 { get; set; }
        public decimal? FRIDAYPERCENT { get; set; }
        public int FRIDAY1 { get; set; }
        public int FRIDAY2 { get; set; }
        public int FRIDAY3 { get; set; }
        public decimal? PREMI1 { get; set; }
        public int PREMI2 { get; set; }
        public int PREMI3 { get; set; }
        public int PREMI4 { get; set; }
        public int PREMI5 { get; set; }
        public int PREMI6 { get; set; }
        public decimal? EXCEED1 { get; set; }
        public int EXCEED2 { get; set; }
        public int EXCEED3 { get; set; }
        public int EXCEED4 { get; set; }
        public int EXCEED5 { get; set; }
        public int EXCEED6 { get; set; }
        public decimal? QTYJJG { get; set; }
        public decimal? QTYKG { get; set; }
        public decimal? HASILPANEN { get; set; }
        public int TOTALPANEN { get; set; }
        public int PCTORIBASIS1 { get; set; }
        public decimal? PCTBASIS1 { get; set; }
        public int PCTBASIS2 { get; set; }
        public int PCTBASIS3 { get; set; }
        public int PCTBASIS4 { get; set; }
        public int PCTBASIS5 { get; set; }
        public int PCTBASIS6 { get; set; }
        public int TOTALPCTORIBASIS1 { get; set; }
        public int TOTALPCTBASIS1 { get; set; }
        public int TOTALPCTBASIS2 { get; set; }
        public int TOTALPCTBASIS3 { get; set; }
        public int TOTALPCTBASIS4 { get; set; }
        public int TOTALPCTBASIS5 { get; set; }
        public int TOTALPCTBASIS6 { get; set; }
        public int NEWORIBASISPCT1 { get; set; }
        public int NEWORIBASIS1 { get; set; }
        public int NEWBASISPCT1 { get; set; }
        public int NEWBASIS1 { get; set; }
        public int NEWPREMISIAP1 { get; set; }
        public int NEWBASISPCT2 { get; set; }
        public int NEWBASIS2 { get; set; }
        public int NEWPREMISIAP2 { get; set; }
        public int NEWBASISPCT3 { get; set; }
        public int NEWBASIS3 { get; set; }
        public int NEWPREMISIAP3 { get; set; }
        public int NEWBASISPCT4 { get; set; }
        public int NEWBASIS4 { get; set; }
        public int NEWPREMISIAP4 { get; set; }
        public int NEWBASISPCT5 { get; set; }
        public int NEWBASIS5 { get; set; }
        public int NEWPREMISIAP5 { get; set; }
        public int NEWBASISPCT6 { get; set; }
        public int NEWBASIS6 { get; set; }
        public int NEWPREMISIAP6 { get; set; }
        public int NEWHASIL1 { get; set; }
        public int NEWPREMILEBIH1 { get; set; }
        public int NEWINCENTIVE1 { get; set; }
        public int NEWHASIL2 { get; set; }
        public int NEWPREMILEBIH2 { get; set; }
        public int NEWINCENTIVE2 { get; set; }
        public int NEWHASIL3 { get; set; }
        public int NEWPREMILEBIH3 { get; set; }
        public int NEWINCENTIVE3 { get; set; }
        public int NEWHASIL4 { get; set; }
        public int NEWPREMILEBIH4 { get; set; }
        public int NEWINCENTIVE4 { get; set; }
        public int NEWHASIL5 { get; set; }
        public int NEWPREMILEBIH5 { get; set; }
        public int NEWINCENTIVE5 { get; set; }
        public int NEWHASIL6 { get; set; }
        public int NEWPREMILEBIH6 { get; set; }
        public int NEWINCENTIVE6 { get; set; }
        public string ACTIVITYID { get; set; }
        public string UNITCODE { get; set; }
        public string PAYMENTNO { get; set; }
        public string EMPLOYEENAME { get; set; }
        public string POSITIONID { get; set; }
        public string FAMILYSTATUS { get; set; }
        public string TAXSTATUS { get; set; }
        public string STATUSID { get; set; }
        public int REMAININGLEAVE { get; set; }
        public int LEAVE { get; set; }
        public string GOLONGAN { get; set; }
        public string SUPERVISORID { get; set; }
        public string CREATEBY { get; set; }
        public DateTime CREATED { get; set; }
        public int HK { get; set; }
        public int KRANIFLAG { get; set; }
        public int EFLAG { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_LoadingResult_GetDaily_Result> sp_LoadingResult_GetDaily_Result(string divId, DateTime date)
        {
            IEnumerable<sp_LoadingResult_GetDaily_Result> rows = null;
            this.LoadStoredProc("sp_LoadingResult_GetDaily")
                .AddParam("Divid", divId)
                .AddParam("Date", date)
                .Exec(r => rows = r.ToList<sp_LoadingResult_GetDaily_Result>());
            return rows;
        }

        public IEnumerable<TLOADINGRESULT> sp_TLOADINGRESULT_GetDaily_Result(string divId, DateTime date)
        {
            IEnumerable<sp_LoadingResult_GetDaily_Result> rows = null;
            IEnumerable<TLOADINGRESULT> rowsx = null;

            this.LoadStoredProc("sp_LoadingResult_GetDaily")
                .AddParam("Divid", divId)
                .AddParam("Date", date)
                .Exec(r => rows = r.ToList<sp_LoadingResult_GetDaily_Result>());

            rowsx = rows.Select(s => new TLOADINGRESULT
            {
                PAYMENTNO = s.PAYMENTNO,
                UNITCODE = s.UNITCODE,
                DIVID = s.DIVID,
                LOADINGCODE = s.LOADINGCODE,
                LOADINGDATE = s.LOADINGDATE,
                PRODUCTID = s.PRODUCTID,
                LOADINGTYPE = s.LOADINGTYPE,
                SPBDATATYPE = s.SPBDATATYPE,
                NOSPB = s.NOSPB,
                VEHICLEID = s.VEHICLEID,
                VEHICLETYPEID = s.VEHICLETYPEID,
                LOADINGPAYMENTTYPE = s.LOADINGPAYMENTTYPE,
                KRANIID = s.KRANIID,
                EMPLOYEEID = s.EMPLOYEEID,
                ACTIVITYID = s.ACTIVITYID,
                EMPLOYEETYPE = s.EMPLOYEETYPE,
                BLOCKID = s.BLOCKID,
                BASISGROUP = s.BASISGROUP,
                ORIBASE1 = s.ORIBASE1,
                BASE1 = Convert.ToDecimal(s.BASE1),
                BASE2 = s.BASE2,
                BASE3 = s.BASE3,
                BASE4 = s.BASE4,
                BASE5 = s.BASE5,
                BASE6 = s.BASE6,
                FRIDAY1 = s.FRIDAY1,
                FRIDAY2 = s.FRIDAY2,
                FRIDAY3 = s.FRIDAY3,
                PREMI1 = Convert.ToDecimal(s.PREMI1),
                PREMI2 = s.PREMI2,
                PREMI3 = s.PREMI3,
                PREMI4 = s.PREMI4,
                PREMI5 = s.PREMI5,
                PREMI6 = s.PREMI6,
                EXCEED1 = Convert.ToDecimal(s.EXCEED1),
                EXCEED2 = s.EXCEED2,
                EXCEED3 = s.EXCEED3,
                EXCEED4 = s.EXCEED4,
                EXCEED5 = s.EXCEED5,
                EXCEED6 = s.EXCEED6,
                QTYJJG = Convert.ToDecimal(s.QTYJJG),
                QTYKG = Convert.ToDecimal(s.QTYKG),
                HASILPANEN = Convert.ToDecimal(s.HASILPANEN),
                TOTALPANEN = s.TOTALPANEN,
                PCTORIBASIS1 = s.PCTORIBASIS1,
                PCTBASIS1 = Convert.ToDecimal(s.PCTBASIS1),
                PCTBASIS2 = s.PCTBASIS2,
                PCTBASIS3 = s.PCTBASIS3,
                PCTBASIS4 = s.PCTBASIS4,
                PCTBASIS5 = s.PCTBASIS5,
                PCTBASIS6 = s.PCTBASIS6,
                TOTALPCTORIBASIS1 = s.TOTALPCTORIBASIS1,
                TOTALPCTBASIS1 = s.TOTALPCTBASIS1,
                TOTALPCTBASIS2 = s.TOTALPCTBASIS2,
                TOTALPCTBASIS3 = s.TOTALPCTBASIS3,
                TOTALPCTBASIS4 = s.TOTALPCTBASIS4,
                TOTALPCTBASIS5 = s.TOTALPCTBASIS5,
                TOTALPCTBASIS6 = s.TOTALPCTBASIS6,
                NEWORIBASISPCT1 = s.NEWORIBASISPCT1,
                NEWBASISPCT1 = s.NEWBASISPCT1,
                NEWORIBASIS1 = s.NEWORIBASIS1,
                NEWBASIS1 = s.NEWBASIS1,
                NEWPREMISIAP1 = s.NEWPREMISIAP1,
                NEWBASISPCT2 = s.NEWBASISPCT2,
                NEWBASIS2 = s.NEWBASIS2,
                NEWPREMISIAP2 = s.NEWPREMISIAP2,
                NEWBASISPCT3 = s.NEWBASISPCT3,
                NEWBASIS3 = s.NEWBASIS3,
                NEWPREMISIAP3 = s.NEWPREMISIAP3,
                NEWBASISPCT4 = s.NEWBASISPCT4,
                NEWBASIS4 = s.NEWBASIS4,
                NEWPREMISIAP4 = s.NEWPREMISIAP4,
                NEWBASISPCT5 = s.NEWBASISPCT5,
                NEWBASIS5 = s.NEWBASIS5,
                NEWPREMISIAP5 = s.NEWPREMISIAP5,
                NEWBASISPCT6 = s.NEWBASISPCT6,
                NEWBASIS6 = s.NEWBASIS6,
                NEWPREMISIAP6 = s.NEWPREMISIAP6,
                NEWHASIL1 = s.NEWHASIL1,
                NEWPREMILEBIH1 = s.NEWPREMILEBIH1,
                NEWINCENTIVE1 = s.NEWINCENTIVE1,
                NEWHASIL2 = s.NEWHASIL2,
                NEWPREMILEBIH2 = s.NEWPREMILEBIH2,
                NEWINCENTIVE2 = s.NEWINCENTIVE2,
                NEWHASIL3 = s.NEWHASIL3,
                NEWPREMILEBIH3 = s.NEWPREMILEBIH3,
                NEWINCENTIVE3 = s.NEWINCENTIVE3,
                NEWHASIL4 = s.NEWHASIL4,
                NEWPREMILEBIH4 = s.NEWPREMILEBIH4,
                NEWINCENTIVE4 = s.NEWINCENTIVE4,
                NEWHASIL5 = s.NEWHASIL5,
                NEWPREMILEBIH5 = s.NEWPREMILEBIH5,
                NEWINCENTIVE5 = s.NEWINCENTIVE5,
                NEWHASIL6 = s.NEWHASIL6,
                NEWPREMILEBIH6 = s.NEWPREMILEBIH6,
                NEWINCENTIVE6 = s.NEWINCENTIVE6,
                HK = s.HK,
                KRANIFLAG = Convert.ToBoolean(s.KRANIFLAG),
                EFLAG = Convert.ToBoolean(s.EFLAG),
                STATUS = "A"
            }).ToList();
            return rowsx;
        }
    }

}
