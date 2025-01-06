using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PMS.Web.UI.TagHelpers
{
    public class XMainList:TagHelper
    {
        public string ColumnsApiUrl { get; set; }
        public string RowsApiUrl { get; set; }

    }
}
