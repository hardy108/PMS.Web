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
    public class XFieldDataListMill : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/mill/list";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{MILLCODE} - {MILLNAME}";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 2;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[MILLCODE,MILLNAME]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "MILLCODE";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[MILLCODE,MILLNAME]";

            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 2 huruf";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
