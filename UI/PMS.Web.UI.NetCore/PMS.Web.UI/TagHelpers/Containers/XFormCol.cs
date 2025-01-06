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
    public class XFormCol : BaseElement
    {
        public XFormCol()
        {
            NoCaption = true;
            _tagName = "div";
            _mainCssClass = "col";
        }
        
    }
}
