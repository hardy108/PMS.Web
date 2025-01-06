using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Text.Encodings.Web;

namespace PMS.Web.UI.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project    
    public class XTab : BaseElement
    {
        public XTab()
        {
            NoCaption = true;
            _tagName = "div";
            _mainCssClass = "tab-content";
            _autoId = true;
            WithInitScript = true;
        }
        public string TabStyle { get; set; }

        protected override void SetInitScript(TagHelperContext context, TagHelperOutput output)
        {
            string navCss = "nav";
            if (!string.IsNullOrWhiteSpace(TabStyle))
                navCss += " " + TabStyle;
            string preElement = $"<ul id='{Id}_Nav' class='{navCss}'></ul>";
            output.PreElement.SetHtmlContent(preElement);
            _initScript = $"xTabInit('{Id}')";
            base.SetInitScript(context, output);
        }
    }
}
