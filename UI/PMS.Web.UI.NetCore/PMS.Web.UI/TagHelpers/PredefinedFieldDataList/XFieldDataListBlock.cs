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
    public class XFieldDataListBlock : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/block/list";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{BLOCKID}";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 1;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[BLOCKID,THNTANAM]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "BLOCKID";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[BLOCKID,LUASBLOCK,THNTANAM]";

            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "Blok";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
