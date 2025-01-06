using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;


namespace PMS.EFCore.Model
{
    public class sp_MandorFine_GetGroup_Result
    {
        public string ID { get; set; }
        public string UNITID { get; set; }
        public string EMPID { get; set; }
        public DateTime DATE { get; set; }
        public string NOTE { get; set; }
        public string STATUS { get; set; }
        public DateTime UPDATED { get; set; }
        public string UNITNAME { get; set; }
        public string EMPCODE { get; set; }
        public string EMPNAME { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_MandorFine_GetGroup_Result> sp_MandorFine_GetGroup(string unitId, DateTime startDate, DateTime endDate)
        {
            IEnumerable<sp_MandorFine_GetGroup_Result> rows = null;
            this.LoadStoredProc("sp_MandorFine_GetGroup")
                .AddParam("UnitId", unitId)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .Exec(r => rows = r.ToList<sp_MandorFine_GetGroup_Result>());
            return rows;
        }

        public IEnumerable<TMANDORFINE> sp_TMANDORFINE_GetGroup(string unitId, DateTime startDate, DateTime endDate)
        {
            IEnumerable<sp_MandorFine_GetGroup_Result> rows = null;
            IEnumerable<TMANDORFINE> rowsx = null;

            this.LoadStoredProc("sp_MandorFine_GetGroup")
                .AddParam("UnitId", unitId)
                .AddParam("StartDate", startDate)
                .AddParam("EndDate", endDate)
                .Exec(r => rows = r.ToList<sp_MandorFine_GetGroup_Result>());

            rowsx = rows.Select(s => new TMANDORFINE
            {
                ID = s.ID,
                UNITID = s.UNITID,
                EMPID = s.EMPID,
                DATE = s.DATE,
                NOTE = s.NOTE,
                STATUS = s.STATUS,
                UPDATED = s.UPDATED
            }).ToList();
            return rowsx;
        }

    }
}
