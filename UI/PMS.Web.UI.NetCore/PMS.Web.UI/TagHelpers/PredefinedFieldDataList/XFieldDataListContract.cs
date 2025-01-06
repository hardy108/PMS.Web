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
    public class XFieldDataListContract : XFieldDataList
    {

        public string Status { get; set; }
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {

            string status = "A";
            DateTime startDate = Convert.ToDateTime("1/1/1753");
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(Status))
            {
                if (Status.Equals("*"))
                    status = string.Empty;
                else
                    status = Status;
            }

            ApiUrl = "api/contract/list?";
            if (!string.IsNullOrWhiteSpace(status))
                ApiUrl += $"Status={status}&StartDate={startDate}&EndDate={endDate}&";


            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{ID} - {CARDID}";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 3;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[ID,UNITID]";



            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "ID";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[ID,UNITID,CARDID,STATUS,DATE]";


            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 3 huruf";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
