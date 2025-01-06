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
    public class XFieldDataListJournalType : XFieldDataList
    {
        public string Modul { get; set; }
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            ApiUrl = "api/JournalType/list";
           if (!string.IsNullOrWhiteSpace(Modul))
                ApiUrl += "?Modul=" + Modul;
            
            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{CODE} - {NAME}";
            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 0;
            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[CODE,NAME]";
            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "CODE";            
            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[CODE,NAME]";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
