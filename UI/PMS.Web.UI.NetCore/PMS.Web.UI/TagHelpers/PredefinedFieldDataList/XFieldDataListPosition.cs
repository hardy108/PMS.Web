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
    public class XFieldDataListPosition : XFieldDataList
    {
        
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/position/list";
            if (string.IsNullOrWhiteSpace(ApiUrlParam))
                ApiUrlParam = "Active=true";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "POSITIONNAME";
            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 2;
            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[POSITIONNAME]";
            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "POSITIONID";
            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 2 huruf";
            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[POSITIONNAME]";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
