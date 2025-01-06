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

namespace PMS.Web.UI.TagHelpers
{
    public class XHidden:BaseElement
    {
        protected string _inputType = "text";
        public string Value
        { get; set; }
        public string BindingField { get; set; }

        public XHidden()
        {
            _tagName = "input";
            _mainCssClass = "form-group";
            _tagMode = TagMode.StartTagAndEndTag;
            _xType = "hidden";
            _autoId = true;
        }

        protected override void BeforeProcessing(TagHelperContext context, TagHelperOutput output)
        {
            NoCaption = true;
            LGSize = 0;
            MDSize = 0;
            SMSize = 0;
            XLSize = 0;
            XSSize = 0;
        }

        protected override void SetCssClassAndStyle(TagHelperContext context, TagHelperOutput output)
        {
            //Do Nothing
        }

        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            _contents = string.Empty;
        }

        protected override void SetAttribute(TagHelperContext context, TagHelperOutput output)
        {
            foreach (TagHelperAttribute attribute in context.AllAttributes)
            {
                output.Attributes.SetAttribute(attribute.Name, attribute.Value);
            }
            output.Attributes.SetAttribute("type", _inputType);
            output.Attributes.SetAttribute("x-type", _xType);
        }


    }
}
