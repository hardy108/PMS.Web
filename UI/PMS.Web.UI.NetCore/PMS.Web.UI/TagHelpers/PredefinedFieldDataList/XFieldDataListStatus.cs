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
    public class XFieldDataListStatus : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/status/list";
            if (string.IsNullOrWhiteSpace(ApiUrlParam))
                ApiUrlParam = "Active=true";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "STATUSNAME";
            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 0;
            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[STATUSNAME,STATUSID,TAXSTATUS,FAMILYSTATUS]";
            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "STATUSID";            
            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[STATUSNAME]";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
