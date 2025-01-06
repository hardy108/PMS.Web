using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_WbTransaction_GetBlockQty_Result
    {
        public string ID { get; set; }
        public string NOSPB { get; set; }
        public DateTime? DATE { get; set; }
        public string VEHID { get; set; }
        public string DRIVER { get; set; }
        public string BLOCK { get; set; }
        public decimal? NETTO { get; set; }
        public decimal? BJR { get; set; }
        public decimal? JJG { get; set; }
        public decimal? BRD { get; set; }
        public decimal? QTY { get; set; }
        public string STATUS { get; set; }
    }

    public class sp_WbTransaction_GetBlockQty_Result2
    {
        public string NOSPB { get; set; }
        public DateTime DATE { get; set; }
        public string DIVID { get; set; }
        public short QLYCATID { get; set; }
        public decimal VALUE { get; set; }
    }
    public partial class PMSContextBase
    {
        public IEnumerable<sp_WbTransaction_GetBlockQty_Result> sp_WbTransaction_GetBlockQty(string estateCode, DateTime startDate, DateTime endDate)
        {
            IEnumerable<sp_WbTransaction_GetBlockQty_Result> rows = null;
            this.LoadStoredProc("sp_WbTransaction_GetBlockQty")
                .AddParam("UnitId", estateCode)
                .AddParam("From", startDate.ToString("yyyyMMdd"))
                .AddParam("To", endDate.ToString("yyyyMMdd"))
                .ExecMultiResults(r => rows = r.ReadToList<sp_WbTransaction_GetBlockQty_Result>());
            return rows;
        }

        public IEnumerable<sp_WbTransaction_GetBlockQty_Result2> sp_WbTransaction_GetBlockQty2(string estateCode, DateTime startDate, DateTime endDate)
        {
            IEnumerable<sp_WbTransaction_GetBlockQty_Result2> rows = null;
            this.LoadStoredProc("sp_WbTransaction_GetBlockQty")
                .AddParam("UnitId", estateCode)
                .AddParam("From", startDate.ToString("yyyyMMdd"))
                .AddParam("To", endDate.ToString("yyyyMMdd"))
                .ExecMultiResults(r =>                 
                {
                    if (r.NextResult())
                        rows = r.ReadToList<sp_WbTransaction_GetBlockQty_Result2>();

                });
            return rows;
        }

        public void sp_WbTransaction_GetBlockQty_MultiResult(string estateCode, DateTime startDate, DateTime endDate, IEnumerable<sp_WbTransaction_GetBlockQty_Result> result1, IEnumerable<sp_WbTransaction_GetBlockQty_Result2> result2)
        {
            result1 = new List<sp_WbTransaction_GetBlockQty_Result>();
            result2 = new List<sp_WbTransaction_GetBlockQty_Result2>();
            this.LoadStoredProc("sp_WbTransaction_GetBlockQty")
                .AddParam("UnitId", estateCode)
                .AddParam("From", startDate.ToString("yyyyMMdd"))
                .AddParam("To", endDate.ToString("yyyyMMdd"))
                .ExecMultiResults(r =>
                {
                    result1= r.ReadToList<sp_WbTransaction_GetBlockQty_Result>();
                    if (r.NextResult())
                        result2 = r.ReadToList<sp_WbTransaction_GetBlockQty_Result2>();

                });
            
        }

    }

}
