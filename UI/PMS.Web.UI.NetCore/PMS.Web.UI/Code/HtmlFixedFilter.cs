using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Web.UI.Code
{
    public class HtmlFixedFilter
    {
        public string ToBindingField { get; set; }
        public string FromFormId { get; set; }

        public override string ToString()
        {
            return string.Format("\"{0}\": {1}", ToBindingField, string.Format("$(\"#{0}\").val()", FromFormId));
        }

        public static string ListToString(List<HtmlFixedFilter> fixedFilters)
        {
            if (fixedFilters.Count == 0)
                throw new Exception("FixedFilters must not be empty");

            string result = "{";
            foreach(HtmlFixedFilter fixedFilter in fixedFilters)
            {
                result += fixedFilter.ToString() + ",";
            }
            result = result.Substring(0, result.Length - 1);
            result += "}";
            return result;
        }
    }   
}