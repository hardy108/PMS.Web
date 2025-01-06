using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PMS.Web.UI.TagHelpers
{
    public class BaseElement:TagHelper
    {
        protected bool _autoId = false;
        protected string _xType = "";
        protected string _tagName = "div";
        protected string _mainCssClass = "row";
        protected string _innerCssClss = "";
        protected TagMode _tagMode = TagMode.StartTagAndEndTag;
        protected string _readOnlyAttribute = "readonly";

        public string Id {get;set;}
        private string _name = string.Empty;
        public string Name
        {
            get
            {
                return string.IsNullOrWhiteSpace(_name) ? Id : _name;
            }
            set { _name = value; }
        }
        public bool IsHidden { get; set; }
        public bool IsRequired { get; set; }
        public int XSSize { get; set; }
        public int SMSize { get; set; }
        public int MDSize { get; set; }
        public int LGSize { get; set; }
        public int XLSize { get; set; }

        public bool NoCaption { get; set; }
        public string Caption { get; set; }

        protected virtual string CaptionHtml
        {
            get { return NoCaption ? string.Empty : $"<label>{Caption}</label>"; }
        }
        
        


        protected virtual string GetVisiblityStyle { get { return Helper.GetVisiblityStyle(IsHidden); } }
        protected virtual string GetSizeCssClass { get { return Helper.GetSizeCssClass(XSSize, SMSize, MDSize, LGSize, XLSize); } }
        
        public bool WithInitScript { get; set; }
        protected string _contents = string.Empty;
        protected string _initScript = string.Empty;
        
        
        protected virtual void SetAttribute(TagHelperContext context, TagHelperOutput output)
        {

            if (!string.IsNullOrWhiteSpace(Id))
                output.Attributes.SetAttribute("id", Id);
            if (!string.IsNullOrWhiteSpace(Name))
                output.Attributes.SetAttribute("name", Name);
            if (!string.IsNullOrWhiteSpace(_xType))
                output.Attributes.SetAttribute("x-type", _xType);



        }

        protected virtual void BeforeProcessing(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Id) && _autoId)
                Id = context.UniqueId;
            output.TagMode = _tagMode;
            output.TagName = _tagName;
            string sizeCss = GetSizeCssClass;
            if (string.IsNullOrWhiteSpace(sizeCss))
                _innerCssClss = string.Empty;
            else
            {
                _innerCssClss = _mainCssClass;
                _mainCssClass = sizeCss;
            }
            
        }

        protected virtual void SetCssClassAndStyle(TagHelperContext context, TagHelperOutput output)
        {
            string cssClass = string.Empty;
            try { cssClass = context.AllAttributes["class"].Value.ToString().Trim(); }
            catch { }
            
            if (!string.IsNullOrWhiteSpace(_mainCssClass))
                cssClass = cssClass.InsertClass(_mainCssClass);

            string s = string.Empty;            
            if (IsRequired)
                cssClass = cssClass.AppendClass("required");
            if (!string.IsNullOrWhiteSpace(cssClass))
                output.Attributes.SetAttribute("class", cssClass.Trim());
            s = GetVisiblityStyle;
            if (!string.IsNullOrWhiteSpace(s))
                output.Attributes.SetAttribute("style", s.Trim());
        }

        protected virtual async void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            _contents = string.Empty;
            try
            {
                _contents = (await output.GetChildContentAsync()).GetContent();
                
            }
            catch(Exception ex) { }
            if (!string.IsNullOrWhiteSpace(_innerCssClss))
                _contents = Helper.WrapElementsWithDiv(new HtmlContentBuilder().AppendHtml(_contents), _innerCssClss).GetHtmlContent();
            
        }

        protected virtual void SetCaption(TagHelperContext context, TagHelperOutput output)
        {
            
            
        }

        protected virtual void SetInitScript(TagHelperContext context, TagHelperOutput output)
        {
            if (WithInitScript && !string.IsNullOrWhiteSpace(_initScript))
                output.PostElement.SetHtmlContent($"<script>{_initScript}</script>");
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            BeforeProcessing(context, output);
            BuildContentHtmlAsync(context, output);
            SetAttribute(context, output);
            SetCssClassAndStyle(context, output);
            SetCaption(context, output);
            output.Content.SetHtmlContent(_contents);            
            SetInitScript(context, output);
        }



        
    }
}
