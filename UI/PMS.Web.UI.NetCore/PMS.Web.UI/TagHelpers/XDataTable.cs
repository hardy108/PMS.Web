using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PMS.Web.UI.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    
    public class XDataTable : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            output.SuppressOutput();
        }
    }
}
