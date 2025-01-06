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
    public class XFieldDataListOrganization : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/Organizations/list";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{ID}";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 3;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[ID,NAME]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "ID";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[ID,CODE,NAME]";

            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 3 huruf";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
