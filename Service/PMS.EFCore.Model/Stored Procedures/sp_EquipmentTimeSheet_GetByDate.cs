using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_EquipmentTimeSheet_GetByDate
    {
        public string ID { get; set; }
        public string UNITID { get; set; }
        public string DIVID { get; set; }
        public DateTime DATE { get; set; }
        public string EQUIPID { get; set; }
        public short ROT { get; set; }
        public string NOTE { get; set; }
        public string STATUS { get; set; }
        public DateTime UPDATED { get; set; }
        public string EQUIPCODE { get; set; }
        public string EQUIPNO { get; set; }
        public string EQUIPTYPE { get; set; }
        public string EQUIPNAME { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_EquipmentTimeSheet_GetByDate> GetByDate(string equipid, DateTime date)
        {
            IEnumerable<sp_EquipmentTimeSheet_GetByDate> rows = null;
            this.LoadStoredProc("GetByDate")
                .AddParam("EQUIPID", equipid)
                .AddParam("DATE", date)
                .Exec(r => rows = r.ToList<sp_EquipmentTimeSheet_GetByDate>());
            return rows;
        }
    }
}
