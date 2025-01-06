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
using PMS.Web.UI.Code;

namespace PMS.Web.UI.TagHelpers
{
    public enum XFieldLovType
    {
        Activity=0,
        Employee,
        Block,
        Material
    }

    public class XFieldLov : BaseElement
    {
        protected string _inputType = "select";
        protected string _defaultInnerDivClass = "input-group";
        protected string _mainInputCssClass = "form-control select2";

        private string _containerId = string.Empty;
        public string ContainerId
        {
            set { _containerId = value; }
            get { return string.IsNullOrWhiteSpace(_containerId) ? (string.IsNullOrWhiteSpace(Id) ? string.Empty : Id + "Container") : _containerId; }
        }

        public string Value
        { get; set; }
        public string BindingField { get; set; }
        public virtual bool ReadOnly { get; set; }
        public string PlaceHolder { get; set; }

        public bool LargeModal { get; set; }

        public bool Multiple { get; set; }
        
        
        private XFieldLovType _lovType = XFieldLovType.Activity;
        private string LovTypeId { get { return "LOV-" + LovType.ToString().ToUpper(); } }
        public XFieldLovType LovType
        {
            get { return _lovType; }
            set
            {
                _lovType = value;

            }
        }


        private List<HtmlFixedFilter> _FixedFilters;
        private List<string> _SourceFieldTexts;
        private List<string> _SourceFieldValues;

        public string AdditionalReturnFunction { get; set; } //Function must accept parameter selectedData i.e. additionFunction(selectedData)        
        public List<HtmlFixedFilter> FixedFilters
        {
            get
            {
                if (_FixedFilters == null)
                    _FixedFilters = new List<HtmlFixedFilter>();
                return _FixedFilters;
            }
            set
            {
                _FixedFilters = new List<HtmlFixedFilter>();
                _FixedFilters.AddRange(value);
            }
        }
        public string SourceFieldText
        {
            get
            {
                try
                { return _SourceFieldTexts[0]; }
                catch { return null; }
            }
            set
            {
                _SourceFieldTexts = new List<String>();
                _SourceFieldTexts.Add(value);
            }
        }
        public List<string> SourceFieldTexts
        {
            get
            {
                if (_SourceFieldTexts == null)
                    _SourceFieldTexts = new List<string>();
                return _SourceFieldTexts;
            }
            set
            {
                _SourceFieldTexts = new List<string>();
                _SourceFieldTexts.AddRange(value);
            }
        }
        public string SourceFieldValue
        {
            get
            {
                try
                { return _SourceFieldValues[0]; }
                catch { return null; }
            }
            set
            {
                _SourceFieldValues = new List<String>();
                _SourceFieldValues.Add(value);
            }
        }
        public List<string> SourceFieldValues
        {
            get
            {
                if (_SourceFieldValues == null)
                    _SourceFieldValues = new List<string>();
                return _SourceFieldValues;
            }
            set
            {
                _SourceFieldValues = new List<string>();
                _SourceFieldValues.AddRange(value);
            }
        }

        public XFieldLov()
        {
            

            _tagName = "div";
            _mainCssClass = "form-group";
            _tagMode = TagMode.StartTagAndEndTag;
            _readOnlyAttribute = "disabled";
            _xType = "lov";
            WithInitScript = true;
            _autoId = true;
        }

        
        protected override void SetAttribute(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(ContainerId))
                output.Attributes.SetAttribute("id", ContainerId);

        }
        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            TagHelperAttributeList attributes = new TagHelperAttributeList();

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
            attributes.SetAttribute("style", "width: 100%;");
            //if (Multiple)
            attributes.SetAttribute("multiple", "multiple");
            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();
            

            contentBuilder.AppendHtml(Helper.CreateElement("select", attributes, _contents));
            contentBuilder.AppendHtml($"<span class='input-group-btn'><button id='{Id}btn' type='button' class='btn btn-primary btn-flat'>...</button></span>");
            _contents = Helper.WrapElementsWithDiv(contentBuilder, "input-group").GetHtmlContent();

            contentBuilder = new HtmlContentBuilder();
            if (!NoCaption)                
                contentBuilder.AppendHtml(CaptionHtml);
            
            contentBuilder.AppendHtml(_contents);
            if (!string.IsNullOrWhiteSpace(_innerCssClss))
                _contents = Helper.WrapElementsWithDiv(contentBuilder, _innerCssClss).GetHtmlContent();

        }

        protected override void BeforeProcessing(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Id))
                Id = context.UniqueId;

            base.BeforeProcessing(context, output);
        }

        protected override void SetInitScript(TagHelperContext context, TagHelperOutput output)
        {
            _initScript = string.Empty;
            WithInitScript = true;

            if (String.IsNullOrWhiteSpace(Id))
                throw new Exception("Form id must not be empty");
            if (String.IsNullOrWhiteSpace(LovTypeId))
                throw new Exception("LoVid must not be empty");

            string result = @"
                    $('#{2}btn')[0].onclick = function() {{
                        helper.loadLoV({0}, {1}, function(selectedData) {{
                            $('#{2}>option').remove()
                            $.each(selectedData, function(i, val){{
                                var newOption = new Option({7}, {3}, true, true);
                                $('#{2}').append(newOption).trigger('change');                               
                            }});
                            {4}                      
                        }},{5},{6});
                        return false;
                    }};";

            string formattedSourceFieldValue = string.Empty;
            if (_SourceFieldValues != null && _SourceFieldValues.Count > 0)
            {
                foreach (string sourceFieldValue in _SourceFieldValues)
                {
                    formattedSourceFieldValue += string.Format("val.{0} + \" - \" + ", sourceFieldValue);
                }
                formattedSourceFieldValue = formattedSourceFieldValue.Substring(0, formattedSourceFieldValue.Length - 11);
            }
            else
                formattedSourceFieldValue = "val.KEY";

            string formattedSourceFieldText = string.Empty;
            if (_SourceFieldValues != null && _SourceFieldTexts.Count > 0)
            {
                foreach (string sourceFieldText in _SourceFieldTexts)
                {
                    formattedSourceFieldText += string.Format("val.{0} + \" - \" + ", sourceFieldText);
                }
                formattedSourceFieldText = formattedSourceFieldText.Substring(0, formattedSourceFieldText.Length - 11);
            }
            else
                formattedSourceFieldText = formattedSourceFieldValue;

            _initScript = string.Format(result,
                                   string.Format("\"{0}\"", LovTypeId),
                                   string.Format("\"{0}\"", Multiple.ToString().ToLower()),
                                   Id,
                                   formattedSourceFieldValue,
                                   !String.IsNullOrWhiteSpace(AdditionalReturnFunction) ? AdditionalReturnFunction + "(selectedData);" : string.Empty,
                                   FixedFilters.Count > 0 ? HtmlFixedFilter.ListToString(FixedFilters) : "null",
                                   string.Format("\"{0}\"", LargeModal.ToString().ToLower()),
                                   formattedSourceFieldText);
            _initScript += $"xSelectInit('{Id}',false, '{Value}', null, 'replace', false,'{SourceFieldValue}','{SourceFieldText}',null,null,null);";
            if (WithInitScript && !string.IsNullOrWhiteSpace(_initScript))
                output.PostElement.SetHtmlContent($"<script>{_initScript}</script>");
        }
    }
}
