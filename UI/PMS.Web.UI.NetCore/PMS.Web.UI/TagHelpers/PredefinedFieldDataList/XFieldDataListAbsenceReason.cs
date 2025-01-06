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
    public class XFieldDataListAbsenceReason : XFieldDataList
    {
        
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/absencereason/list";
            if (string.IsNullOrWhiteSpace(ApiUrlParam))
                ApiUrlParam = "Active=true";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{TEXT}";
            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 0;
            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[ID,TEXT]";
            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "ID";
            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "reason";
            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[TEXT]";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
