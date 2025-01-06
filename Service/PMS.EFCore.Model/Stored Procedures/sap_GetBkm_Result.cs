using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sap_GetBkm_Result
    {
        public string DATE { get; set; }
        public string CODE { get; set; }
        public string BLOCK { get; set; }
        public string ACTID { get; set; }
        public string UNIT { get; set; }
        public string DIV { get; set; }
        public string MATID { get; set; }
        public string QTY { get; set; }
        public string UOM { get; set; }
        public string BATCH { get; set; }
        public string CE { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sap_GetBkm_Result> sap_GetBkm(string Code)
        {
            IEnumerable<sap_GetBkm_Result> rows = null;
            this.LoadStoredProc("sap_GetBkm")
                .AddParam("No", Code)
                .Exec(r => rows = r.ToList<sap_GetBkm_Result>());
            return rows;
        }
    }
}
