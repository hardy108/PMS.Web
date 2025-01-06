using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_DHS_DashBoard_GetSummaryData_Result
    {
        public string UNITCODE { get; set; }
        public int? BlockCount { get; set; }
        public decimal? YtdJanjangPanen { get; set; }
        public decimal? MtdJanjangPanen { get; set; }
        public decimal? YtdJanjangKirim { get; set; }
        public decimal? MtdJanjangKirim { get; set; }
        public decimal? YtdJanjangRestan { get; set; }
        public decimal? MtdJanjangRestan { get; set; }
        public decimal? YtdBrondolPanen { get; set; }
        public decimal? MtdBrondolKirim { get; set; }
        public decimal? YtdBrondolKirim { get; set; }
        public decimal? MtdBrondolPanen { get; set; }
        public decimal? YtdJjgTerima { get; set; }
        public decimal? YtdTonTerima { get; set; }
        public decimal? MtdJjgTerima { get; set; }
        public decimal? MtdTonTerima { get; set; }
    }

    public partial class PMSContextBase
    {

        public IEnumerable<sp_DHS_DashBoard_GetSummaryData_Result> sp_DHS_DashBoard_GetSummaryData(string estateCode, DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<sp_DHS_DashBoard_GetSummaryData_Result> rows = null;
            this.LoadStoredProc("sp_DHS_DashBoard_GetSummaryData")
                    .AddParam("estateCode",estateCode)
                    .AddParam("startDate",startDate)
                    .AddParam("endDate",endDate)
                    .Exec(r => rows = r.ToList<sp_DHS_DashBoard_GetSummaryData_Result>());
            return rows;
            //return sp_DHS_DashBoard_GetSummaryData_Result.FromSql($"execute sp_DHS_DashBoard_GetSummaryData {estateCode},{startDate},{endDate}");
        }
    }

}
