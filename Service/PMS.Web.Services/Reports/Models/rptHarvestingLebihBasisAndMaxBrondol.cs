using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Web.Services.Reports.Models
{
    public partial class rptHarvestingLebihBasisAndMaxBrondol
    {
        public DateTime STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public string UNITCODE { get; set; }
        public string UNITNAME { get; set; }
        public string HARVESTCODE { get; set; }
        public DateTime HARVESTDATE { get; set; }
        public string HARVESTTYPE { get; set; }
        public string ACTIVITYID { get; set; }
        public string EMPLOYEEID { get; set; }
        public string EMPLOYEENAME { get; set; }
        public Decimal? JJG { get; set; }
        public Decimal? KG { get; set; }
        public Decimal? BASE { get; set; }
        public Decimal? MAXHASIL { get; set; }
        public Decimal? RATIO { get; set; }
    }
}
