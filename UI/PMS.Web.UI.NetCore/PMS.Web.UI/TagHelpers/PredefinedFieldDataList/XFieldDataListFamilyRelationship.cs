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
    public class XFieldDataListFamilyRelationship : XFieldDataList
    {
        
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/reference/familyrelation";
            ApiUrlParam = string.Empty;
            TextFields = "Text";
            MinLengthSearch = 0;
            SearchFields = "Text";
            ValueField = "Id";            
            VisibleFields = "[Text]";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
