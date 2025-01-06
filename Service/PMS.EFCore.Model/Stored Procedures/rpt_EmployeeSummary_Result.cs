using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class rpt_EmployeeSummary_Result
    {
        public string RegionId { get; set; }
        public string RegionName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string UnitId { get; set; }
        public string UnitInitial { get; set; }
        public string UnitName { get; set; }
        public string PositionId { get; set; }
        public string PositionName { get; set; }
        public short Standard { get; set; }
        public int? BHL_SEX_L { get; set; }
        public int? BHL_SEX_P { get; set; }
        public int? SKUH_SEX_L { get; set; }
        public int? SKUH_SEX_P { get; set; }
        public int? SKUB_SEX_L { get; set; }
        public int? SKUB_SEX_P { get; set; }
        public int BHL { get { return BHL_SEX_L.Value + BHL_SEX_P.Value; } }
        public int SKUH { get { return SKUH_SEX_L.Value + SKUH_SEX_P.Value; } }
        public int SKUB { get { return SKUB_SEX_L.Value + SKUB_SEX_P.Value; } }
        public int TOTAL { get { return BHL + SKUH + SKUB; } }

    }

    public partial class PMSContextBase
    {

        public IEnumerable<rpt_EmployeeSummary_Result> rpt_EmployeeSummary(string Id)
        {
            IEnumerable<rpt_EmployeeSummary_Result> rows = null;
            this.LoadStoredProc("rpt_EmployeeSummary")
                    .AddParam("Id", Id)
                    .Exec(r => rows = r.ToList<rpt_EmployeeSummary_Result>());
            return rows;
        }
    }

}
