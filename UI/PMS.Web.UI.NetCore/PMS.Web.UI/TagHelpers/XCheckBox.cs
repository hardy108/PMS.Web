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
    public class XCheckBox: BaseElement
    {
        protected string _inputType = "checkbox";


        private string _containerId = string.Empty;
        public string ContainerId
        {
            set { _containerId = value; }
            get { return string.IsNullOrWhiteSpace(_containerId) ? (string.IsNullOrWhiteSpace(Id) ? string.Empty : Id + "Container") : _containerId; }
        }

        public bool Value
        { get; set; }
        public string BindingField { get; set; }
        public virtual bool ReadOnly { get; set; }
        

        public XCheckBox()
        {
            _tagName = "div";
            _mainCssClass = "form-group";
            _tagMode = TagMode.StartTagAndEndTag;
            _xType = "checkbox";
            _autoId = true;
            _readOnlyAttribute = "disabled";
        }

        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            //base.BuildContentHtmlAsync(context, output);            
            TagHelperAttributeList attributes = new TagHelperAttributeList();
            attributes.SetAttribute("type", _inputType);
            
            if (ReadOnly)
                attributes.SetAttribute(_readOnlyAttribute, $"{_readOnlyAttribute}");            
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
            attributes.SetAttribute("class", "form-control");
            if (Value)
                attributes.SetAttribute("checked", "checked");


            IHtmlContent content;
            if (NoCaption)
                content = Helper.CreateElement("input", attributes, string.Empty);
            else
                content = Helper.WrapElementsWithContainer("label", Helper.CreateElement("input", attributes, Caption), string.Empty);

            if (string.IsNullOrWhiteSpace(_innerCssClss))
                _contents = content.GetHtmlContent();
            else
                _contents = Helper.WrapElementsWithDiv(content, _innerCssClss).GetHtmlContent();


        }


        protected override void SetAttribute(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(ContainerId))
                output.Attributes.SetAttribute("id", ContainerId);

        }


    }
}
