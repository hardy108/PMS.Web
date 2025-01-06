using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_HarvestingBlock_GetOtherQty_Result
    {
        public string BLOCKID { get; set; }
        public decimal JJG0 { get; set; }
        public decimal BRD0 { get; set; }
        public decimal JJG1 { get; set; }
        public decimal BRD1 { get; set; }
        public decimal JJG2 { get; set; }
        public decimal BRD2 { get; set; }

    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_HarvestingBlock_GetOtherQty_Result> sp_HarvestingBlock_GetOtherQty(string harvestCode, DateTime date)
        {
            IEnumerable<sp_HarvestingBlock_GetOtherQty_Result> rows = null;
            this.LoadStoredProc("sp_HarvestingBlock_GetOtherQty")
                .AddParam("Code", harvestCode)
                .AddParam("Date", date)
                .Exec(r => {
                    rows = r.ToList<sp_HarvestingBlock_GetOtherQty_Result>();
                });
            return rows;
        }
    }
}
