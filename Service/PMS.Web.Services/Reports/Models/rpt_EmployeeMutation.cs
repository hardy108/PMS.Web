using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Web.Services.Reports.Models
{
    public partial class rpt_EmployeeMutation
    {
        public string REGIONLAMA { get; set; }
        public string REGIONBARU { get; set; }
        public string COMPANYLAMA { get; set; }
        public string COMPANYBARU { get; set; }
        public string UNITCODELAMA { get; set; }
        public string UNITNAMELAMA { get; set; }
        public string UNITIDLAMA { get; set; }
        public string DIVISILAMA { get; set; }
        public string UNITCODEBARU { get; set; }
        public string UNITNAMEBARU { get; set; }
        public string UNITIDBARU { get; set; }
        public string DIVISIBARU { get; set; }
        public string EMPID { get; set; }
        public string EMPNAME { get; set; }
        public string JABATANLAMA { get; set; }
        public string JABATANBARU { get; set; }
        public string TYPELAMA { get; set; }
        public string TYPEBARU { get; set; }
        public DateTime TMK { get; set; }
        public DateTime TANGGALEFEKTIF { get; set; }
        public string MASAKERJA { get; set; }
        public string SEX { get; set; }
        public string STATUSKELUARGALAMA { get; set; }
        public string STATUSKELUARGABARU { get; set; }
        public string PLACEOFBIRTH { get; set; }
        public DateTime BIRTHDAY { get; set; }
        public int? AGE { get; set; }
        public string SUKU { get; set; }
        public string RELIGION { get; set; }
        public string EDUCATION { get; set; }
        public string NOKTP { get; set; }
    }
}
