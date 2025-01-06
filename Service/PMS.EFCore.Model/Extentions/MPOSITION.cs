using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PMS.EFCore.Model
{
    partial class MPOSITION
    {
        [NotMapped()]
        public string GENDERFLAGTEXT
        {
            get
            {
                if (string.IsNullOrWhiteSpace(GENDERFLAG))
                    return "Umum";
                if (GENDERFLAG == "L")
                    return "Laki-laki";
                if (GENDERFLAG == "P")
                    return "Perempuan";
                return "Tidak Valid";
            }
        }
    }
}
