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
    public class XFieldDataListVendor : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/vendor/listforselect?active=true";
            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "Text";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 2;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[Id,Text]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "Id";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "Text";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
