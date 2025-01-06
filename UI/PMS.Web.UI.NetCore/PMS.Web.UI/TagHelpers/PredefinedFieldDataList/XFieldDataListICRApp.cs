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
    public class XFieldDataListICRApp : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/icrapp/list";
            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{APPID} - {APPNAME}";
            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 0;
            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[APPID,APPNAME]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "APPID";
            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[APPNAME]";
            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "Application Type";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
