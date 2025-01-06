using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Web.UI.Code
{
    public class HtmlButton:HtmlElement
    {
        
        public override string ToString()
        {
            
            string html = @"<button id='{0}' type='button' class='btn {2} btn-flat' {3}>{1}</button>";
            return string.Format(html, Id, Caption, CssString, AttributeString);
        }
    }
}