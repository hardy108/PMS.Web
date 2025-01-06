using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_SPB_GetBlockQty_Result
    {
        public string NOSPB { get; set; }
        public DateTime? DATE { get; set; }
        public string VEHID { get; set; }
        public string DRIVER { get; set; }
        public string BLOCK { get; set; }
        public decimal? NETTO { get; set; }
        public int BJR { get; set; }
        public decimal? JJG { get; set; }
        public decimal? BRD { get; set; }
        public decimal? QTY { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_SPB_GetBlockQty_Result> sp_SPB_GetBlockQty_Result(string unitId, DateTime from, DateTime to)
        {
            IEnumerable<sp_SPB_GetBlockQty_Result> rows = null;
            this.LoadStoredProc("sp_SPB_GetBlockQty")
                .AddParam("UnitId", unitId)
                .AddParam("From", from)
                .AddParam("To", to)
                .Exec(r => rows = r.ToList<sp_SPB_GetBlockQty_Result>());
            return rows;
        }
    }

}
