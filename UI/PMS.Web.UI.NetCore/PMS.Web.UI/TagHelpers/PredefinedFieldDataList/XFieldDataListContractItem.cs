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
    public class XFieldDataListContractItem : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {

            ApiUrl = "api/contract/getitem?";
            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{ACTID} - {ACTNAME}";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 0;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[ACTID,ACTNAME]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "ACTID";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[ACTID,ACTNAME]";

            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 0 huruf";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
