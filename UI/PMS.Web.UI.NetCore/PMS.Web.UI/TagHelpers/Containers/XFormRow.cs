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
    public class XFormRow : BaseElement
    {
        public bool NoGutters
        { get; set; }
        public XFormRow()
        {
            NoCaption = true;
            _tagName = "div";
            _mainCssClass = "row";
        }

        protected override void BeforeProcessing(TagHelperContext context, TagHelperOutput output)
        {
            if (NoGutters)
                _mainCssClass = "row no-gutters";
            else
                _mainCssClass = "row";
            base.BeforeProcessing(context, output);
        }
    }
}
