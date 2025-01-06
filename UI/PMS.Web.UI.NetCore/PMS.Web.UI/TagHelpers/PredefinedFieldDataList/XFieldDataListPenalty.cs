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
    public class XFieldDataListPenalty : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            //ApiUrl = "api/penaltytype/listforselect?active=true";
            //if (string.IsNullOrWhiteSpace(TextFields))
            //    TextFields = "Text";

            //if (!MinLengthSearch.HasValue)
            //    MinLengthSearch = 0;

            //if (string.IsNullOrWhiteSpace(SearchFields))
            //    SearchFields = "[Id,Text]";

            //if (string.IsNullOrWhiteSpace(ValueField))
            //    ValueField = "Id";

            //if (string.IsNullOrWhiteSpace(VisibleFields))
            //    VisibleFields = "Text";
            //SearchContain = true;
            //base.BuildContentHtmlAsync(context, output);

            ApiUrl = "api/penaltytype/list";
            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{PENALTYCODE} - {DESCRIPTION}";
            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 2;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[PENALTYCODE,DESCRIPTION]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "PENALTYCODE";

            if (string.IsNullOrWhiteSpace(SearchFields))
                VisibleFields = "[PENALTYCODE,DESCRIPTION]";

            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 2 huruf";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
