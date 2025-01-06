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
    public class XFieldTextArea:BaseElement
    {
        protected string _mainInputCssClass = "form-control";


        private string _containerId = string.Empty;
        public string ContainerId
        {
            set { _containerId = value; }
            get { return string.IsNullOrWhiteSpace(_containerId) ? (string.IsNullOrWhiteSpace(Id) ? string.Empty : Id + "Container") : _containerId; }
        }

        
        public string BindingField { get; set; }
        public virtual bool ReadOnly { get; set; }
        public string PlaceHolder { get; set; }

        public int Rows { get; set; }


        public XFieldTextArea()
        {
            _tagName = "div";
            _mainCssClass = "form-group";
            _tagMode = TagMode.StartTagAndEndTag;
            _xType = "text-area";
            _autoId = true;
            _readOnlyAttribute = "disabled";
        }

        protected virtual TagHelperAttributeList SetAttributeForMainInput(TagHelperContext context, TagHelperOutput output)
        {
            TagHelperAttributeList attributes = new TagHelperAttributeList();
            
            if (!string.IsNullOrWhiteSpace(PlaceHolder))
                attributes.SetAttribute("placeholder", PlaceHolder);
            if (ReadOnly)
                attributes.SetAttribute(_readOnlyAttribute, "disabled");
            
            if (!string.IsNullOrWhiteSpace(BindingField))
                attributes.SetAttribute("data-bf", BindingField);

            if (!string.IsNullOrWhiteSpace(Id))
                attributes.SetAttribute("Id", Id);
            if (!string.IsNullOrWhiteSpace(Name))
                attributes.SetAttribute("name", Name);
            if (!string.IsNullOrWhiteSpace(_xType))
                attributes.SetAttribute("x-type", _xType);
            if (ReadOnly)
                attributes.SetAttribute("read-only", "true");

            if (Rows>=0)
            {
                attributes.SetAttribute("rows", Rows.ToString());

            }
            attributes.SetAttribute("class", _mainInputCssClass);

            return attributes;
        }

        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            //base.BuildContentHtmlAsync(context, output);            

            TagHelperAttributeList attributes = SetAttributeForMainInput(context, output);

            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();
            if (!NoCaption)
                contentBuilder.AppendHtml(CaptionHtml);
            contentBuilder.AppendHtml(Helper.CreateElement("textarea", attributes, string.Empty));



            if (string.IsNullOrWhiteSpace(_innerCssClss))
                _contents = contentBuilder.GetHtmlContent();
            else
                _contents = Helper.WrapElementsWithDiv(contentBuilder, _innerCssClss).GetHtmlContent();




        }


        protected override void SetAttribute(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(ContainerId))
                output.Attributes.SetAttribute("id", ContainerId);

        }

        protected override void SetInitScript(TagHelperContext context, TagHelperOutput output)
        {
            _initScript += $"xRegisterEvent('{Id}','change');\r\n";
            base.SetInitScript(context, output);
        }
    }
}
