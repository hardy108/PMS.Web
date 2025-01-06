using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_Stock_Get_Result
    {     
            public decimal STOCK { get; set; }
            public decimal AMOUNT { get; set; }
            public decimal PRICE { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_Stock_Get_Result> sp_Stock_Get(string locationCode, string materialId, DateTime date)
        {
            IEnumerable<sp_Stock_Get_Result> rows = null;
            this.LoadStoredProc("sp_Stock_Get")
                .AddParam("LocationCode", locationCode)
                .AddParam("MaterialId", materialId)
                .AddParam("Date", date)
                .Exec(r => rows = r.ToList<sp_Stock_Get_Result>());
            return rows;
        }
    }
}
