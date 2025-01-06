using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Web.Services.Reports.Models
{
    public class rptHarvestingUpkeepDailyDivision
    {
        public DateTime STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public string UNITCODE { get; set; }
        public string UNITNAME { get; set; }
        public string DIVID { get; set; }
        public string ACTIVITYID { get; set; }
        public string ACTIVITYNAME { get; set; }
        public string BLOCKID { get; set; }
        public int? LUASBLOCK { get; set; }
        public string UOMACTIVITY { get; set; }
        public Decimal? HIHASILKERJA { get; set; }
        public Decimal? SDHIHASILKERJA { get; set; }
        public Decimal? HIHK { get; set; }
        public Decimal? SDHIHK { get; set; }
        public Decimal? HIUPAH { get; set; }
        public Decimal? SDHIUPAH { get; set; }
        public Decimal? HIHASILKERJABORONGAN { get; set; }
        public Decimal? SDHIHASILKERJABORONGAN { get; set; }
        public Decimal? HIPREMIBORONGAN { get; set; }
        public Decimal? SDHIPREMIBORONGAN { get; set; }
        public Decimal? TOTALUPAHHI { get; set; }
        public Decimal? TOTALUPAHSDHI { get; set; }
        public string MATERIALNAME { get; set; }
        public string UOMMATERIAL { get; set; }
        public Decimal? HIQTY { get; set; }
        public Decimal? SDHIQTY { get; set; }
        public Decimal? HIQTYCOST { get; set; }
        public Decimal? SDHIQTYCOST { get; set; }
        public Decimal? TOTALCOSTSDHI { get; set; }
        public Decimal? HAHK { get; set; }
        public Decimal? BAHANHA { get; set; }
        public Decimal? COSTHA { get; set; }
    }
}
