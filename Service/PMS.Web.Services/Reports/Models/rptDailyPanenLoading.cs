using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Web.Services.Reports.Models
{
    public class rptDailyPanenLoading
    {
        public DateTime STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public int? NO { get; set; }
        public string UNITCODE { get; set; }
        public string UNITNAME { get; set; }
        public string ACTIVITYID { get; set; }
        public string PEKERJAAN { get; set; }
        public Int16 THNTANAM { get; set; }
        public string PAYMENTTYPE { get; set; }
        public Decimal? HAHI { get; set; }
        public Decimal? HASDHI { get; set; }
        public Decimal? JJGHI { get; set; }
        public Decimal? JJGSDHI { get; set; }
        public Decimal? KGHI { get; set; }
        public Decimal? KGSDHI { get; set; }
        public Decimal? HKHI { get; set; }
        public Decimal? HKSDHI { get; set; }
        public Decimal? UPAHHI { get; set; }
        public Decimal? UPAHSDHI { get; set; }
        public Decimal? PREMIBASISHI { get; set; }
        public Decimal? PREMIBASISSDHI { get; set; }
        public Decimal? PREMILEBIHBASISHI { get; set; }
        public Decimal? PREMILEBIHBASISSDHI { get; set; }
        public Decimal? TOTALUPAHHI { get; set; }
        public Decimal? TOTALUPAHSDHI { get; set; }
        public Decimal? COSTKG { get; set; }
    }
}
