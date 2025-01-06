using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Web.Services.Reports.Models
{
    public class rptUpkeepDaily
    {
        public DateTime STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public string UNITCODE { get; set; }
        public string UNITNAME { get; set; }
        public string ACTID { get; set; }
        public string ACTNAME { get; set; }
        public Int16? THNTANAM { get; set; }
        public string UOM { get; set; }
        public Decimal? DAILYHAHI { get; set; }
        public Decimal? DAILYHASDHI { get; set; }
        public Decimal? DAILYHKHI { get; set; }
        public Decimal? DAILYHKSDHI { get; set; }
        public Decimal? DAILYUPAHHI { get; set; }
        public Decimal? DAILYUPAHSDHI { get; set; }
        public Decimal? BORHAHI { get; set; }
        public Decimal? BORHASDHI { get; set; }
        public Decimal? BORPREMIHI { get; set; }
        public Decimal? BORPREMISDHI { get; set; }
        public Decimal? UPAHHI { get; set; }
        public Decimal? UPAHSDHI { get; set; }
        public string MATERIALNAME { get; set; }
        public string UOMMAT { get; set; }
        public Decimal? QTYMATHI { get; set; }
        public Decimal? QTYMATSDHI { get; set; }
        public Decimal? COSTMATHI { get; set; }
        public Decimal? COSTMATSDHI { get; set; }
        public Decimal? COST { get; set; }
        public Decimal? HKHA { get; set; }
        public Decimal? MATHA { get; set; }
        public Decimal? COSTHA { get; set; }
    }
}
