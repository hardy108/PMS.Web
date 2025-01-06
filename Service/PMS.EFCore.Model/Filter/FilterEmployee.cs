using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterEmployee: GeneralFilter
    {
        public bool ShowInactive { get; set; }
        public bool ShowDeleted { get; set; }
        public bool IsUnitMandatory { get; set; }
        public bool IsDivisionMandatory { get; set; }
        public string SupervisorId { get; set; }
        public List<string> SupervisorIds { get; set; }
        public string PositionId { get; set; }
        public int PinId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeType { get; set; }
        public bool WithDetails { get; set; }
        public bool Mandor1 { get; set; }
        public int Mandor1Flag { get { return Mandor1 ? 1 : 0; } }
        public bool MandorTanam { get; set; }
        public int MandorTanamFlag { get { return MandorTanam ? 2 : 0; } }
        public bool MandorNonTanam { get; set; }
        public int MandorNonTanamFlag { get { return MandorNonTanam ? 3 : 0; } }
        public bool KraniTanam { get; set; }
        public int KraniTanamFlag { get { return KraniTanam ? 4 : 0; } }
        public bool KraniNonTanam { get; set; }
        public int KraniNonTanamFlag { get { return KraniNonTanam ? 5 : 0; } }
        public bool Harvester { get; set; }
        public int HarvesterFlag { get { return Harvester ? 6 : 0; } }
        public bool AllPostitionFlag
        {
            get
            {
                return !Mandor1 && !MandorTanam && !MandorNonTanam && !KraniTanam && !KraniNonTanam && !Harvester;
            }
        }

        public bool ByUserName { get; set; }

        public FilterEmployee()
        {
            SupervisorIds = new List<string>();
        }

    }
}
