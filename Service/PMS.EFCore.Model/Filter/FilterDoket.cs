using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterDoket:GeneralFilter
    {
        public string Status { get; set; }
        public int RowNoStart { get; set; }
        public int RowNoEnd { get; set; }
        public string DoketId { get; set; }
        public string BlokId { get; set; }
    }
}
