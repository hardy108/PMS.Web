using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterJournal : GeneralFilter
    {
        public string Modul { get; set; }
        public string JournalType { get; set; }
        public string Ref { get; set; }
    }
}
