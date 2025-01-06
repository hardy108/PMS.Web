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
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project    
    public class XFieldInput : BaseElement
    {

        protected string _inputType = "text";
        protected string _mainInputCssClass = "form-control";
        

        private string _containerId = string.Empty;
        public string ContainerId
        {
            set { _containerId = value; }
            get { return string.IsNullOrWhiteSpace(_containerId) ? (string.IsNullOrWhiteSpace(Id) ? string.Empty : Id + "Container") : _containerId; }
        }

        public virtual string Value
        { get; set; }
        public string BindingField { get; set; }
        public virtual bool ReadOnly { get; set; }
        public string PlaceHolder { get; set; }
        

        public XFieldInput() 
        {
            _tagName = "div";
            _mainCssClass = "form-group";
            _tagMode = TagMode.StartTagAndEndTag;
            _xType = "text";
            _autoId = true;
        }

        protected virtual TagHelperAttributeList SetAttributeForMainInput(TagHelperContext context, TagHelperOutput output)
        {
            TagHelperAttributeList attributes = new TagHelperAttributeList();
            attributes.SetAttribute("type", _inputType);
            if (!string.IsNullOrWhiteSpace(PlaceHolder))
                attributes.SetAttribute("placeholder", PlaceHolder);
            if (ReadOnly)
                attributes.SetAttribute(_readOnlyAttribute, "readonly");
            if (!string.IsNullOrWhiteSpace(Value))
                attributes.SetAttribute("value", Value);
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
            contentBuilder.AppendHtml(Helper.CreateElement("input", attributes, string.Empty));
            


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
    public class XFieldText : XFieldInput
    {
        public XFieldText() : base()
        {
            _inputType = "text";
        }
    }

    public class XFieldPassword : XFieldInput
    {
        private string inputGroupId = string.Empty;
        public XFieldPassword() : base()
        {
            _inputType = "password";
            WithInitScript = true;
        }

        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            //base.BuildContentHtmlAsync(context, output);            

            TagHelperAttributeList attributes = SetAttributeForMainInput(context, output);

            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();

            inputGroupId = $"{Id}_InputGroup";
            contentBuilder.AppendHtml(Helper.CreateElement("input", attributes, string.Empty));
            contentBuilder.AppendHtml("<div class='input-group-addon'><a href=''><i class='fa fa-eye-slash' aria-hidden='true'></i></a></div>");
            _contents = $"<div class='input-group' id='{inputGroupId}'>{contentBuilder.GetHtmlContent()}</div>";


            contentBuilder = new HtmlContentBuilder();
            if (!NoCaption)
                contentBuilder.AppendHtml(CaptionHtml);
            contentBuilder.AppendHtml(_contents);



            if (string.IsNullOrWhiteSpace(_innerCssClss))
                _contents = contentBuilder.GetHtmlContent();
            else
                _contents = Helper.WrapElementsWithDiv(contentBuilder, _innerCssClss).GetHtmlContent();
        }

        protected override void SetInitScript(TagHelperContext context, TagHelperOutput output)
        {

            _initScript += $"xShowHidePassword('{inputGroupId}');\r\n";
            base.SetInitScript(context, output);
        }
    }

    public class XFieldNumber : XFieldInput
    {
        public int DecimalDigit { get; set; }
        public bool OnlyInteger { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public int Step { get; set; }

        private string StepAttribute
        {
            get
            {
                string step = string.Empty;
                if (Step <= 0)
                    return step;

                if (OnlyInteger)                
                    step = Step.ToString();
                else
                {
                    if (DecimalDigit > 0)
                    {
                        step = string.Empty;
                        for (int i = 1; i < DecimalDigit; i++)
                        {
                            step += "0";
                        }
                        step = "0." + step + "1";
                    }
                }

                return step;

            }
        }

        protected override TagHelperAttributeList SetAttributeForMainInput(TagHelperContext context, TagHelperOutput output)
        {
            TagHelperAttributeList attributes = base.SetAttributeForMainInput(context, output);
            string step = StepAttribute;
            if (!string.IsNullOrWhiteSpace(step))
                attributes.SetAttribute("step", step);
            return attributes;
        }

        public XFieldNumber() : base()
        {
            _inputType = "number";
        }
    }

    public class XFieldEmail : XFieldInput
    {
        public XFieldEmail() : base()
        {
            _inputType = "email";
        }
    }


    
}
