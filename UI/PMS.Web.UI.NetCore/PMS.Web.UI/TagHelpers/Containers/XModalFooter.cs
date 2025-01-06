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
    public class XModalFooter : BaseElement
    {
        public XModalFooter()
        {
            NoCaption = true;
            _tagName = "div";
            _mainCssClass = "modal-footer";
        }
    }
}
