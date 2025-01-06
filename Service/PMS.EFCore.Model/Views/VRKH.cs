using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class VRKH
    {
        public string JenisPekerjaan { get; set; }
        public string Blok { get; set; }
        public string MandorKontraktor { get; set; }
        public decimal? JumlahVolume { get; set; }
        public string SatuanVolume { get; set; }
        public int? TKOrang { get; set; }
        public string NamaMaterial { get; set; }
        public decimal? Jumlah { get; set; }
        public string Satuan { get; set; }
    }

}
