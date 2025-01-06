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
    public class XFieldDataListVehicle : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/vehicle/list";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{CODE} - {NO}";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 2;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[CODE,NO]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "ID";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[ID,TYPEID,CODE,NO]";

            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 2 huruf";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
