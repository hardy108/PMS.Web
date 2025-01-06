using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace PMS.Web.UI.TagHelpers
{
    
    public class XCard : BaseElement
    {
        public XCard()
        {
            NoCaption = true;
            _tagName = "div";
            _mainCssClass = "card";
            _xType = "card";            
        }







        protected override string CaptionHtml
        {
            get
            {
                return $"<h5 class='card-header h5'>{Caption}</h5>";
            }
        }

        protected override void SetCaption(TagHelperContext context, TagHelperOutput output)
        {
            //Override to do nothing
        }

        protected override async void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            _contents = string.Empty;
            try
            {
                _contents = (await output.GetChildContentAsync()).GetContent();
            }
            catch { };
            _contents = Helper.WrapElementsWithDiv(new HtmlContentBuilder().AppendHtml(_contents), "card-body").GetHtmlContent();
            _contents = CaptionHtml + _contents;
        }
    }
}
