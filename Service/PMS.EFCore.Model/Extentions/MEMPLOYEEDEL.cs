using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    public partial class MEMPLOYEEDEL
    {
        [NotMapped()]
        public string EMPNAME { get; set; }
        [NotMapped()]
        public string EMPPOSITION { get; set; }

        [NotMapped()]
        public string FLAGTEXT
        {
            get
            {
                return FLAG ? "Sudah Diproses" : "Belum Diproses";

            }
        }

        [NotMapped()]
        public DateTime RESIGNEDDATE { get; set; }
    }
}
