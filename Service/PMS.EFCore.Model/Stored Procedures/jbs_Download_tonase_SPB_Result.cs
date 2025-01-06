using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;


namespace PMS.EFCore.Model
{
    public class jbs_Download_tonase_SPB_Result
    {
        public string NO { get; set; }
        public DateTime DATE { get; set; }
        public decimal WEIGHT1 { get; set; }
        public decimal WEIGHT2 { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<jbs_Download_tonase_SPB_Result> GetSPBResult(string unitId, DateTime date)
        {
            IEnumerable<jbs_Download_tonase_SPB_Result> rows = null;
            this.LoadStoredProc("jbs_Download_tonase_SPB")
                .AddParam("UnitId", unitId)
                .AddParam("Date", date)
                .Exec(r => rows = r.ToList<jbs_Download_tonase_SPB_Result>());
            return rows;
        }

        public IEnumerable<jbs_Download_tonase_SPB_Result> GetMillResult(string unitId, DateTime date)
        {
            IEnumerable<jbs_Download_tonase_SPB_Result> rows = null;
            this.LoadStoredProc("jbs_Download_tonase_MILL")
                .AddParam("UnitCode", unitId)
                .AddParam("Date", date)
                .Exec(r => rows = r.ToList<jbs_Download_tonase_SPB_Result>());
            return rows;
        }

    }
}
