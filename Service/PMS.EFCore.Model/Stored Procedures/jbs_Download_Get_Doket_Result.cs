using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class jbs_Download_Get_Doket_Result
    {
        public string SPBNO { get; set; }
        public string DOKID { get; set; }
        public decimal? JJG { get; set; }
        public decimal? BRD { get; set; }
        public decimal? BJR { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<jbs_Download_Get_Doket_Result> jbs_Download_Get_Doket_Result(string unitId, DateTime date)
        {
            IEnumerable<jbs_Download_Get_Doket_Result> rows = null;
            this.LoadStoredProc("jbs_Download_Get_Doket")
                .AddParam("UnitCode", unitId)
                .AddParam("Date", date)
                .Exec(r => rows = r.ToList<jbs_Download_Get_Doket_Result>());
            return rows;
        }
    }

}
