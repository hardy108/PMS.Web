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
    public class sp_HarvestingResult_GetDaily_Result
        {
        public string HARVESTCODE { get; set; }
        public DateTime? STARTDATE { get; set; }
        public DateTime? ENDDATE { get; set; }
        public DateTime HARVESTDATE { get; set; }
        public string BLOCKID { get; set; }
        public string BASISGROUP { get; set; }
        public string DIVID { get; set; }
        public string MANDORID { get; set; }
        public string MANDOR1ID { get; set; }
        public string KRANIID { get; set; }
        public string CHECKERID { get; set; }
        public string EMPLOYEEID { get; set; }
        public string GEMPID { get; set; }
        public short HARVESTTYPE { get; set; }
        public short? HARVESTPAYMENTTYPE { get; set; }
        public string EMPLOYEETYPE { get; set; }
        public decimal? ORIBASE1 { get; set; }
        public decimal? BASE1 { get; set; }
        public decimal? BASE2 { get; set; }
        public decimal? BASE3 { get; set; }
        public decimal? FRIDAYPCT { get; set; }
        public int FRIDAY1 { get; set; }
        public decimal? FRIDAY2 { get; set; }
        public decimal? FRIDAY3 { get; set; }
        public decimal? PREMI1 { get; set; }
        public decimal? PREMI2 { get; set; }
        public decimal? PREMI3 { get; set; }
        public decimal? EXCEED1 { get; set; }
        public decimal? EXCEED2 { get; set; }
        public decimal? EXCEED3 { get; set; }
        public decimal? FINEQTY { get; set; }
        public decimal? FINEAMOUNT { get; set; }
        public decimal? PREMIPKKTGI { get; set; }
        public int INCENTIVEPKKTGI { get; set; }
        public decimal? QTYJJG { get; set; }
        public decimal? QTYKG { get; set; }
        public decimal? HASILPANEN { get; set; }
        public int TOTALPANEN { get; set; }
        public int PCTORIBASIS1 { get; set; }
        public decimal? PCTBASIS1 { get; set; }
        public decimal? PCTBASIS2 { get; set; }
        public decimal? PCTBASIS3 { get; set; }
        public int TOTALPCTORIBASIS1 { get; set; }
        public int TOTALPCTBASIS1 { get; set; }
        public int TOTALPCTBASIS2 { get; set; }
        public int TOTALPCTBASIS3 { get; set; }
        public int NEWORIBASISPCT1 { get; set; }
        public int NEWBASISPCT1 { get; set; }
        public int NEWORIBASIS1 { get; set; }
        public int NEWBASIS1 { get; set; }
        public int NEWPREMISIAP1 { get; set; }
        public int NEWBASISPCT2 { get; set; }
        public int NEWBASIS2 { get; set; }
        public int NEWPREMISIAP2 { get; set; }
        public int NEWBASISPCT3 { get; set; }
        public int NEWBASIS3 { get; set; }
        public int NEWPREMISIAP3 { get; set; }
        public int NEWHASIL1 { get; set; }
        public int NEWPREMILEBIH1 { get; set; }
        public int NEWINCENTIVE1 { get; set; }
        public int NEWHASIL2 { get; set; }
        public int NEWPREMILEBIH2 { get; set; }
        public int NEWINCENTIVE2 { get; set; }
        public int NEWHASIL3 { get; set; }
        public int NEWPREMILEBIH3 { get; set; }
        public int NEWINCENTIVE3 { get; set; }
        public decimal? HAHASIL { get; set; }
        public decimal? HABASE { get; set; }
        public decimal? HAPREMI { get; set; }
        public int HAPCT { get; set; }
        public int NEWHAPCT { get; set; }
        public int NEWHABASE { get; set; }
        public int NEWHAPREMI { get; set; }
        public int HAINCENTIVE { get; set; }
        public decimal? ATTPREMI { get; set; }
        public int NEWATTPREMI { get; set; }
        public int ATTINCENTIVE { get; set; }
        public decimal? GERDANBASE { get; set; }
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
        public int MANDORFLAG { get; set; }
        public int MANDOR1FLAG { get; set; }
        public int KRANIFLAG { get; set; }
        public int CHECKERFLAG { get; set; }
        public int EFLAG { get; set; }
        public int GFLAG { get; set; }
        public string TOPOGRAFI { get; set; }
    }
    public partial class PMSContextBase
        {
            public IEnumerable<sp_HarvestingResult_GetDaily_Result> sp_HarvestingResult_GetDaily_Result(string divId, DateTime date)
            {
                IEnumerable<sp_HarvestingResult_GetDaily_Result> rows = null;
                this.LoadStoredProc("sp_HarvestingResult_GetDaily")
                    .AddParam("Divid", divId)
                    .AddParam("Date", date)
                    .Exec(r => rows = r.ToList<sp_HarvestingResult_GetDaily_Result>());
            return rows;
            }

            public IEnumerable<VTHARVESTRESULT1> sp_VTHARVESTRESULT1_GetDaily(string divId, DateTime date)
            {           
                IEnumerable<sp_HarvestingResult_GetDaily_Result> rows = null;
                IEnumerable<VTHARVESTRESULT1> rowsx = null;

                this.LoadStoredProc("sp_HarvestingResult_GetDaily")
                    .AddParam("Divid", divId)
                    .AddParam("Date", date)
                    .Exec(r => rows = r.ToList<sp_HarvestingResult_GetDaily_Result>());

                rowsx = rows.Select(s => new VTHARVESTRESULT1
                {
                    PAYMENTNO = s.PAYMENTNO,
                    UNITCODE = s.UNITCODE,
                    DIVID = s.DIVID,
                    HARVESTCODE = s.HARVESTCODE,
                    HARVESTDATE = s.HARVESTDATE,
                    ACTIVITYID = s.ACTIVITYID,
                    HARVESTTYPE = s.HARVESTTYPE,
                    HARVESTPAYMENTTYPE = Convert.ToSByte(s.HARVESTPAYMENTTYPE),
                    BLOCKID = s.BLOCKID,
                    MANDOR1ID = s.MANDOR1ID,
                    MANDORID = s.MANDORID,
                    KRANIID = s.KRANIID,
                    CHECKERID = s.CHECKERID,
                    EMPLOYEEID = s.EMPLOYEEID,
                    GEMPID = s.GEMPID,
                    EMPLOYEETYPE = s.EMPLOYEETYPE,
                    BASISGROUP = s.BASISGROUP,
                    ORIBASE1 = Convert.ToDecimal(s.ORIBASE1),
                    BASE1 = Convert.ToDecimal(s.BASE1),
                    BASE2 = Convert.ToDecimal(s.BASE2),
                    BASE3 = Convert.ToDecimal(s.BASE3),
                    FRIDAY1 = Convert.ToDecimal(s.FRIDAY1),
                    FRIDAY2 = Convert.ToDecimal(s.FRIDAY2),
                    FRIDAY3 = Convert.ToDecimal(s.FRIDAY3),
                    PREMI1 = Convert.ToDecimal(s.PREMI1),
                    PREMI2 = Convert.ToDecimal(s.PREMI2),
                    PREMI3 = Convert.ToDecimal(s.PREMI3),
                    EXCEED1 = Convert.ToDecimal(s.EXCEED1),
                    EXCEED2 = Convert.ToDecimal(s.EXCEED2),
                    EXCEED3 = Convert.ToDecimal(s.EXCEED3),
                    FINEQTY = Convert.ToDecimal(s.FINEQTY),
                    FINEAMOUNT = Convert.ToDecimal(s.FINEAMOUNT),
                    QTYJJG = Convert.ToDecimal(s.QTYJJG),
                    QTYKG = Convert.ToDecimal(s.QTYKG),
                    HASILPANEN = Convert.ToDecimal(s.HASILPANEN),
                    TOTALPANEN = Convert.ToDecimal(s.TOTALPANEN),
                    PCTORIBASIS1 = Convert.ToDecimal(s.PCTORIBASIS1),
                    PCTBASIS1 = Convert.ToDecimal(s.PCTBASIS1),
                    PCTBASIS2 = Convert.ToDecimal(s.PCTBASIS2),
                    PCTBASIS3 = Convert.ToDecimal(s.PCTBASIS3),
                    TOTALPCTORIBASIS1 = Convert.ToDecimal(s.TOTALPCTORIBASIS1),
                    TOTALPCTBASIS1 = Convert.ToDecimal(s.TOTALPCTBASIS1),
                    TOTALPCTBASIS2 = Convert.ToDecimal(s.TOTALPCTBASIS2),
                    TOTALPCTBASIS3 = Convert.ToDecimal(s.TOTALPCTBASIS3),
                    NEWORIBASISPCT1 = Convert.ToDecimal(s.NEWORIBASISPCT1),
                    NEWBASISPCT1 = Convert.ToDecimal(s.NEWBASISPCT1),
                    NEWORIBASIS1 = Convert.ToDecimal(s.NEWORIBASIS1),
                    NEWBASIS1 = Convert.ToDecimal(s.NEWBASIS1),
                    NEWPREMISIAP1 = Convert.ToDecimal(s.NEWPREMISIAP1),
                    NEWBASISPCT2 = Convert.ToDecimal(s.NEWBASISPCT2),
                    NEWBASIS2 = Convert.ToDecimal(s.NEWBASIS2),
                    NEWPREMISIAP2 = Convert.ToDecimal(s.NEWPREMISIAP2),
                    NEWBASISPCT3 = Convert.ToDecimal(s.NEWBASISPCT3),
                    NEWBASIS3 = Convert.ToDecimal(s.NEWBASIS3),
                    NEWPREMISIAP3 = Convert.ToDecimal(s.NEWPREMISIAP3),
                    NEWHASIL1 = Convert.ToDecimal(s.NEWHASIL1),
                    NEWPREMILEBIH1 = Convert.ToDecimal(s.NEWPREMILEBIH1),
                    NEWINCENTIVE1 = Convert.ToDecimal(s.NEWINCENTIVE1),
                    NEWHASIL2 = Convert.ToDecimal(s.NEWHASIL2),
                    NEWPREMILEBIH2 = Convert.ToDecimal(s.NEWPREMILEBIH2),
                    NEWINCENTIVE2 = Convert.ToDecimal(s.NEWINCENTIVE2),
                    NEWHASIL3 = Convert.ToDecimal(s.NEWHASIL3),
                    NEWPREMILEBIH3 = Convert.ToDecimal(s.NEWPREMILEBIH3),
                    NEWINCENTIVE3 = Convert.ToDecimal(s.NEWINCENTIVE3),
                    PREMIPKKTGI = Convert.ToDecimal(s.PREMIPKKTGI),
                    INCENTIVEPKKTGI = Convert.ToDecimal(s.INCENTIVEPKKTGI),
                    HAHASIL = Convert.ToDecimal(s.HAHASIL),
                    HABASE = Convert.ToDecimal(s.HABASE),
                    HAPREMI = Convert.ToDecimal(s.HAPREMI),
                    HAPCT = Convert.ToDecimal(s.HAPCT),
                    NEWHAPCT = Convert.ToDecimal(s.NEWHAPCT),
                    NEWHABASE = Convert.ToDecimal(s.NEWHABASE),
                    NEWHAPREMI = Convert.ToDecimal(s.NEWHAPREMI),
                    HAINCENTIVE = Convert.ToDecimal(s.HAINCENTIVE),
                    ATTPREMI = Convert.ToDecimal(s.ATTPREMI),
                    NEWATTPREMI = Convert.ToDecimal(s.NEWATTPREMI),
                    ATTINCENTIVE = Convert.ToDecimal(s.ATTINCENTIVE),
                    GERDANBASE = Convert.ToDecimal(s.GERDANBASE),
                    HK = s.HK,
                    MANDOR1FLAG = Convert.ToBoolean(s.MANDOR1FLAG),
                    MANDORFLAG = Convert.ToBoolean(s.MANDORFLAG),
                    KRANIFLAG = Convert.ToBoolean(s.KRANIFLAG),
                    CHECKERFLAG = Convert.ToBoolean(s.CHECKERFLAG),
                    EFLAG = Convert.ToBoolean(s.EFLAG),
                    GFLAG = Convert.ToBoolean(s.GFLAG),
                    STATUS = "A"
                }).ToList();
                return rowsx;

            }


    }

}
