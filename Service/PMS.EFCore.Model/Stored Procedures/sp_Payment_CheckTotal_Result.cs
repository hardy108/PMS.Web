using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_Payment_CheckTotal
    {
        public decimal TOTAL { get; set; }
        public decimal DTOTAL { get; set; }
        public decimal ITOTAL { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_Payment_CheckTotal> sp_Payment_CheckTotal(string DocNo)
        {
            IEnumerable<sp_Payment_CheckTotal> rows = null;
            this.LoadStoredProc("sp_Payment_CheckTotal")
                .AddParam("DocNo", DocNo)
                .Exec(r => rows = r.ToList<sp_Payment_CheckTotal>());
            return rows;
        }
    }

}
