using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMS.Web.UI.TagHelpers;
using PMS.Shared.Models;

namespace PMS.Web.UI.TagHelpers
{
    public class XFieldDataListPeriod : XFieldDataList
    {
        
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/period/list";
            
            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{PERIODNAME}";
            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 0;
            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[YEAR,MONTH,PERIODNAME]";
            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "PERIODCODE";
            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "Period";
            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[PERIODNAME]";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
