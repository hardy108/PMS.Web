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
    public class XFieldDataListEmployeeTypeHarvest : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/employeeType/GetHarvestingType";
            SearchDisabled = true;
            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "Text";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 0;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[ID,CODE]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "ID";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "CODE";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
