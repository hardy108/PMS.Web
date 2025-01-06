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
    
    public class XTabPanel : BaseElement
    {
        public XTabPanel()
        {
            NoCaption = true;
            _tagName = "div";
            _mainCssClass = "tab-pane";
            _xType = "tab-panel";
            _autoId = true;
        }

        
        

      

        
        protected override async void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            _contents = string.Empty;
            try
            {
                _contents = (await output.GetChildContentAsync()).GetContent();

            }
            catch (Exception ex) { }
            _contents = Helper.WrapElementsWithDiv(new HtmlContentBuilder().AppendHtml(_contents), "panel-body").GetHtmlContent();
            _contents = Helper.WrapElementsWithDiv(new HtmlContentBuilder().AppendHtml(_contents), "panel panel-default").GetHtmlContent();
        }

        protected override void SetAttribute(TagHelperContext context, TagHelperOutput output)
        {
            base.SetAttribute(context, output);            
            if (!string.IsNullOrWhiteSpace(Caption))
                output.Attributes.SetAttribute("caption",Caption );
            
        }
    }
}
