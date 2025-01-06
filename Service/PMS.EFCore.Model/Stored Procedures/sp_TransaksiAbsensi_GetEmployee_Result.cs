using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_TransaksiAbsensi_GetEmployee_Result
    {
        public string UNITID { get; set; }
        public int PIN { get; set; }
        public string EMPID { get; set; }
        public string EMPCODE { get; set; }
        public string EMPNAME { get; set; }
        public string EMPTYPE { get; set; }
        public DateTime? DATE { get; set; }
        public DateTime? CHECKIN { get; set; }
        public DateTime? CHECKOUT { get; set; }
        public string INSTATUS { get; set; }
        public string OUTSTATUS { get; set; }
        public string CARDIN { get; set; }
        public string CARDOUT { get; set; }
        public string ATTSTATUS { get; set; }
        public DateTime? INSTART { get; set; }
        public DateTime? INEND { get; set; }
        public DateTime? OUTSTART { get; set; }
        public DateTime? OUTEND { get; set; }
        public DateTime? INTIME { get; set; }
        public DateTime? BREAKSTART { get; set; }
        public DateTime? BREAKEND { get; set; }
        public DateTime? OUTTIME { get; set; }
        public int? WORKLIMIT { get; set; }
        public bool? CARDVALID { get; set; }
        public short? CARDQUOTA { get; set; }
        public DateTime? WORKACTUAL { get; set; }
        public int HKPAID { get; set; }
        public int OVERTIME { get; set; }
        public int OT150 { get; set; }
        public int OT200 { get; set; }
        public int OT300 { get; set; }
        public int OT400 { get; set; }
        public int HOLIDAY { get; set; }
        public decimal SPL { get; set; }
        public int AUTO { get; set; }
        public string STATUS { get; set; }
        public string UPDATEDBY { get; set; }
        public DateTime UPDATED { get; set; }

    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_TransaksiAbsensi_GetEmployee_Result> sp_TransaksiAbsensi_GetEmployee(string UnitId, DateTime Date)
        {
            IEnumerable<sp_TransaksiAbsensi_GetEmployee_Result> rows = null;
            this.LoadStoredProc("sp_TransaksiAbsensi_GetEmployee")
                .AddParam("UnitId", UnitId)
                .AddParam("Date", Date)
                .Exec(r => rows = r.ToList<sp_TransaksiAbsensi_GetEmployee_Result>());
            return rows;
        }
    }
}
