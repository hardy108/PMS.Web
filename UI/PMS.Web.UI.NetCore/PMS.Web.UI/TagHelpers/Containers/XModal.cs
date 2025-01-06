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
    public class XModal : BaseElement
    {

        public XModal()
        {
            _tagName = "div";
            _mainCssClass = "modal fade";
        }

        protected override string CaptionHtml 
        {
            get
            {
                return string.Format(
                   @"<div class='modal-header'>
                        <button type='button' class='close' data-dismiss='modal' aria-label='Close'>
                            <span aria-hidden='true'>&times;</span>
                        </button>
                        <h4 class='modal-title'>{0}</h4>
                    </div>",
                   string.IsNullOrWhiteSpace(Caption) ? string.Empty : Caption);
            }
        }

        protected override void SetCaption(TagHelperContext context, TagHelperOutput output)
        {
            //Override to do nothing
        }

        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.BuildContentHtmlAsync(context, output);
            _contents = CaptionHtml + _contents;

            _contents =
               "<div class='modal-dialog'>" +
                    "<div class='modal-content'>" +                    
                    _contents +
                    "</div>" +
                "</div>";
        }

        


        
    }


    

    
    
}
