using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;


namespace PMS.EFCore.Model
{
    public class sp_PaymentJournal
    {
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string NOTE { get; set; }
        public decimal? AMOUNT { get; set; }
        public string BLOCKID { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_PaymentJournal> sp_PaymentJournal(string DocNo)
        {
            IEnumerable<sp_PaymentJournal> rows = null;
            this.LoadStoredProc("sp_PaymentJournal")
                .AddParam("DocNo", DocNo)
                .Exec(r => rows = r.ToList<sp_PaymentJournal>());
            return rows;
        }
    }

}
