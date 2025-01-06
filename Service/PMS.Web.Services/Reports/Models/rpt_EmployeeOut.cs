using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Web.Services.Reports.Models
{
    public partial class rpt_EmployeeOut
    {
        public string REGION { get; set; }
        public string COMPANY { get; set; }
        public string UNITCODE { get; set; }
        public string UNITNAME { get; set; }
        public string UNITID { get; set; }
        public string DIVISI { get; set; }
        public string EMPID { get; set; }
        public string EMPNAME { get; set; }
        public string JABATAN { get; set; }
        public string TYPE { get; set; }
        public DateTime TMK { get; set; }
        public DateTime TKK { get; set; }
        public string MASAKERJA { get; set; }
        public string SEX { get; set; }
        public string STATUSKELUARGA { get; set; }
        public string PLACEOFBIRTH { get; set; }
        public DateTime BIRTHDAY { get; set; }
        public int? AGE { get; set; }
        public string SUKU { get; set; }
        public string RELIGION { get; set; }
        public string EDUCATION { get; set; }
        public string NOKTP { get; set; }
        public string KETERANGAN { get; set; }
    }
}
