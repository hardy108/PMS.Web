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
    public class XFieldDataListEmployee : XFieldDataList
    {


        //public string Status { get; set; }
        public bool ShowInactive { get; set; }
        public bool ShowDeleted { get; set; }
        public bool IsUnitMandatory { get; set; }
        public bool IsDivisionMandatory { get; set; }
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {

            //string status = "A";


            //if (!string.IsNullOrWhiteSpace(Status))
            //{
            //    if (Status.Equals("*"))
            //        status = string.Empty;
            //    else
            //        status = Status;
            //}

            ApiUrl = "api/employee/list?";
            if (ShowDeleted)
                ApiUrl += $"ShowDeleted=true&";
            if (ShowInactive)
                ApiUrl += $"ShowInactive=true&";
            //if (!string.IsNullOrWhiteSpace(status))
            //    ApiUrl += $"RecordStatus={status}&";
            if (IsUnitMandatory)
                ApiUrl += $"IsUnitMandatory=true&";
            if (IsDivisionMandatory)
                ApiUrl += $"IsDivisionMandatory=true&";

            if (string.IsNullOrWhiteSpace(TextFields))
                TextFields = "{EMPID} - {EMPNAME}";

            if (!MinLengthSearch.HasValue)
                MinLengthSearch = 3;

            if (string.IsNullOrWhiteSpace(SearchFields))
                SearchFields = "[EMPID,DIVID,EMPNAME,KTPID,EMPCODE,POSITIONNAME]";



            if (string.IsNullOrWhiteSpace(ValueField))
                ValueField = "EMPID";

            if (string.IsNullOrWhiteSpace(VisibleFields))
                VisibleFields = "[EMPID,EMPNAME,POSITIONNAME,EMPTYPE]";


            if (string.IsNullOrWhiteSpace(PlaceHolder))
                PlaceHolder = "input minimal 3 huruf";
            SearchContain = true;
            base.BuildContentHtmlAsync(context, output);
        }
    }
}
