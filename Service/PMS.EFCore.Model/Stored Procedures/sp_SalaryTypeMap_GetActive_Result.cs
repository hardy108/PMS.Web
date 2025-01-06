using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_SalaryTypeMap_GetActive_Result
    {
        public string ID { get; set; }
        public string UNITID { get; set; }
        public string TYPEID { get; set; }
        public string FREQ { get; set; }
        public string POSID { get; set; }
        public string GOL { get; set; }
        public string EMPID { get; set; }
        public decimal HKMIN { get; set; }
        public decimal AMOUNT { get; set; }
        public string STATUS { get; set; }
        public DateTime UPDATED { get; set; }
        public string UNITNAME { get; set; }
        public string TYPENAME { get; set; }
        public string EMPCODE { get; set; }
        public string EMPNAME { get; set; }
        public string POSNAME { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_SalaryTypeMap_GetActive_Result> sp_SalaryTypeMap_GetActive(string unitId)
        {
            IEnumerable<sp_SalaryTypeMap_GetActive_Result> rows = null;
            this.LoadStoredProc("sp_SalaryTypeMap_GetActive")
                .AddParam("UnitId", unitId)
                .Exec(r => rows = r.ToList<sp_SalaryTypeMap_GetActive_Result>());
            return rows;
        }

        public IEnumerable<TSALARYTYPEMAP> sp_TSALARYTYPEMAP_GetActive(string unitId)
        {
            IEnumerable<sp_SalaryTypeMap_GetActive_Result> rows = null;
            IEnumerable<TSALARYTYPEMAP> rowsx = null;

            this.LoadStoredProc("sp_SalaryTypeMap_GetActive")
                .AddParam("UnitId", unitId)
                .Exec(r => rows = r.ToList<sp_SalaryTypeMap_GetActive_Result>());

            rowsx = rows.Select(s => new TSALARYTYPEMAP
            {
                ID = s.ID,
                UNITID = s.UNITID,
                TYPEID = s.TYPEID,
                FREQ = s.FREQ,
                POSID = s.POSID,
                GOL = s.GOL,
                EMPID = s.EMPID ,
                HKMIN = s.HKMIN,
                AMOUNT = s.AMOUNT,
                STATUS = s.STATUS,
                UPDATED = s.UPDATED
            }).ToList();
            return rowsx;
        }

    }
}