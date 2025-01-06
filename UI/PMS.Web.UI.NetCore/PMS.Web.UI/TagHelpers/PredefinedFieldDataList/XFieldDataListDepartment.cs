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
    public class XFieldDataListDepartment : XFieldDataList
    {
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/department/list";
            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{DEPTID} - {DEPTNAME}";
            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 0;
            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[DEPTID,DEPTNAME]";

            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "DEPTID";
            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[DEPTID,DEPTNAME]";
            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "Department";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
