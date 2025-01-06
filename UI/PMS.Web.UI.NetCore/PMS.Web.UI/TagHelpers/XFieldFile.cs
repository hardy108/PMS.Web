using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Web.UI.TagHelpers
{
    public class XFieldFile : TagHelper
    {
        public string Caption { get; set; }


        public string PropertyName
        {
            get;
            set;
        }

        public bool ReadOnly
        {
            get;
            set;
        }

        public string BindingField { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            string databf = string.Empty;
            if (!string.IsNullOrWhiteSpace(BindingField))
                databf = $"data-bf='{BindingField}'";

            string html = "<div class='form-group form-group-sm'>" +
            $"<label>{Caption}</label>" +
            $"<input type='file' class='form-control' id='i{PropertyName}Upload' data-file-description='{PropertyName}' />" +
            $"<div id='i{PropertyName}Display' style='display:none'>" +
                $"<input type = 'number' {databf} x-type='file-upload' class='form-control' id='i{PropertyName}' name='{PropertyName}' style='display:none' />" +
                $"<input class='form-control' id='i{PropertyName}Name' name='{PropertyName}Name' " +
                $"style='display:inline-block;cursor:pointer;width:80%;color:blue;text-decoration:underline' readonly/>" +
                $"<button type = 'button' class='btn btn-danger' id='i{PropertyName}BtnDelete'>" +
                    "<i class='fa fa-trash-o'></i>" +
                "</button>" +
            "</div>" +
            "</div>";
            output.Content.SetHtmlContent(html);
            output.PostElement.SetHtmlContent($"<script>InitializeFileUploader('{PropertyName}',{(ReadOnly ? "true" : "false")})</script>");
        }
    }
}
