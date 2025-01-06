using PMS.EFCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class sp_HarvestingBlockResult
    {
        public DateTime DATE { get; set; }
        public string BLOCK { get; set; }
        public decimal QTY { get; set; }
    }

    public class sp_HarvestingBlockResult2
    {
        public string CODE { get; set; }
        public string BLOCK { get; set; }
        public DateTime DATE { get; set; }
        public short TYPE { get; set; }
        public decimal QTY { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_HarvestingBlockResult> sp_HarvestingBlock_Result(DateTime startDate, DateTime endDate, string unitId, string divisionCode)
        {
            IEnumerable<sp_HarvestingBlockResult> rows = null;
            this.LoadStoredProc("sp_WbTransaction_GetBlockQty")
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .AddParam("UnitId", unitId)
                .AddParam("DivisionCode", divisionCode)
                .ExecMultiResults(r => rows = r.ReadToList<sp_HarvestingBlockResult>());

            return rows;
        }

        public IEnumerable<sp_HarvestingBlockResult2> sp_HarvestingBlock_Result2(DateTime startDate, DateTime endDate, string unitId, string divisionCode)
        {
            IEnumerable<sp_HarvestingBlockResult2> rows = null;
            this.LoadStoredProc("sp_WbTransaction_GetBlockQty")
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .AddParam("UnitId", unitId)
                .AddParam("DivisionCode", divisionCode)
                .ExecMultiResults(r => 
                {
                    if (r.NextResult())
                        rows = r.ReadToList<sp_HarvestingBlockResult2>();
                });

            return rows;
        }

        public void sp_HarvestingBlock_ResultMulti(DateTime startDate, DateTime endDate, string unitId, string divisionCode,IEnumerable<sp_HarvestingBlockResult> result1, IEnumerable<sp_HarvestingBlockResult2> result2)
        {
            result1 = null;
            result2 = null;
            this.LoadStoredProc("sp_WbTransaction_GetBlockQty")
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .AddParam("UnitId", unitId)
                .AddParam("DivisionCode", divisionCode)
                .ExecMultiResults(r =>
                {
                    result1 = r.ReadToList<sp_HarvestingBlockResult>();
                    if (r.NextResult())
                        result2 = r.ReadToList<sp_HarvestingBlockResult2>();
                });

            
        }
    }
}
