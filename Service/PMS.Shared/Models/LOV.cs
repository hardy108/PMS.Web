using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class LOV
    {
        public string LOVID { get; set; }
        public string LOVName { get; set; }
        public string ListID { get; set; }
        public bool FromWebAPI { get; set; }
        public string ApiUrl { get; set; }
        public string ApiUrlCount { get; set; }
        public string ConnectionName { get; set; }
        public string SourceTableName { get; set; }
        public string ViewFilterName { get; set; }
        public string FilterJson { get; set; }
        public List<string> MandatoryFilterItems { get; set; }
        public FilterJson FilterRows { get; set; }
        public List<string> KeyFields { get; set; }
        public string FilterValidation { get; set; }
    }
}
