using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_EquipmentTimeSheet_GetLastKm
    {
        public decimal KM { get; set; }
    }

    public partial class PMSContextBase
    {
        public IEnumerable<sp_EquipmentTimeSheet_GetLastKm> GetLastKm(string equipid, short rot)
        {
            IEnumerable<sp_EquipmentTimeSheet_GetLastKm> rows = null;
            this.LoadStoredProc("GetLastKm")
                .AddParam("EQUIPID", equipid)
                .AddParam("ROT", rot)
                .Exec(r => rows = r.ToList<sp_EquipmentTimeSheet_GetLastKm>());
            return rows;
        }
    }
}
