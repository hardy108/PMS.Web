using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_HarvestingResult_GetFromHarvestingCollection_Result
    {
        public string HARVESTCODE { get; set; }
        public DateTime? STARTDATE { get; set; }
        public DateTime? ENDDATE { get; set; }
        public string EMPLOYEEID { get; set; }
        public DateTime HARVESTDATE { get; set; }
        public string BLOCKID { get; set; }
        public string BASISGROUP { get; set; }
        public string DIVID { get; set; }
        public string MANDORID { get; set; }
        public string MANDOR1ID { get; set; }
        public string KRANIID { get; set; }
        public string CHECKERID { get; set; }
        public short HARVESTTYPE { get; set; }
        public short? HARVESTPAYMENTTYPE { get; set; }
        public string EMPLOYEETYPE { get; set; }
        public string GEMPID { get; set; }
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
        public short? REMAININGLEAVE { get; set; }
        public short? LEAVE { get; set; }
        public string GOLONGAN { get; set; }
        public string SUPERVISORID { get; set; }
        public string CREATEBY { get; set; }
        public DateTime CREATED { get; set; }
        public int MANDOR1FLAG { get; set; }
        public int MANDORFLAG { get; set; }
        public int KRANIFLAG { get; set; }
        public int CHECKERFLAG { get; set; }
        public int EFLAG { get; set; }
        public int GFLAG { get; set; }
        public int HK { get; set; }
        public string TOPOGRAFI { get; set; }
    }
    public partial class PMSContextBase
    {
        public IEnumerable<sp_HarvestingResult_GetFromHarvestingCollection_Result> sp_HarvestingResult_GetFromHarvestingCollection_Result
            (string unitCode, string basedCalculation, string premiSystem, DateTime startperiod, DateTime startdate, DateTime endDate, int period, bool basisByKg)
        {
            IEnumerable<sp_HarvestingResult_GetFromHarvestingCollection_Result> rows = null;
            this.LoadStoredProc("sp_HarvestingResult_GetFromHarvestingCollection")
                .AddParam("UnitCode", unitCode)
                .AddParam("BasedCalculation", basedCalculation)
                .AddParam("PremiSystem", premiSystem)
                .AddParam("StartPeriod", startperiod)
                .AddParam("StartDate", startdate)
                .AddParam("EndDate", endDate)
                .AddParam("Period", period)
                .AddParam("BasisByKg", basisByKg)               
                .Exec(r => rows = r.ToList<sp_HarvestingResult_GetFromHarvestingCollection_Result>());
            return rows;
        }
    }

}
