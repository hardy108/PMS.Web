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
    public class XFieldDataListSPB : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/HarvestingBlockResult/getlistspb";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{SPBID} - Netto: {NETTO} - {SOURCE}";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 4;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[SPBID]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "SPBID";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[SPBID,NETTO,MILL]";

            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 4 huruf";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
