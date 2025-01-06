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
    public class XModalBody : BaseElement
    {
        public XModalBody()
        {
            NoCaption = true;
            _tagName = "div";
            _mainCssClass = "modal-body";
            _innerCssClss = "container-fluid";
        }

        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.BuildContentHtmlAsync(context, output);
            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();
            contentBuilder.AppendHtml(_contents);
            _contents = Helper.WrapElementsWithDiv(contentBuilder, _innerCssClss).GetHtmlContent();
        }
    }
}
