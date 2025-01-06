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
    public enum ESelectType
    {
        Single=0,
        Multiple=1,
        Boolean=2
    }
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project    
    public class XFieldSelect : BaseElement
    {

        protected string _inputType = "select";
        protected string _defaultInnerDivClass = "input-group";
        protected string _mainInputCssClass = "form-control select2";
        protected string _trueText = "Yes";
        protected string _falseText = "No";
        protected bool? _defaultBooleanValue = null;

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
        

        public string ApiUrl { get; set; }
        

        public string IdField { get; set; }
        public string TextField { get; set; }

        public string ApiUrlMode { get; set; }

        public bool AllowManualInput { get; set; }
        
        public ESelectType SelectType { get; set; }
        


        public List<string> AscendantIds { get; set; }
        public List<string> AscendantFieldIds { get; set; }
        public List<string> DescendantIds { get; set; }
        

        public XFieldSelect() 
        {
            _tagName = "div";
            _mainCssClass = "form-group";
            _tagMode = TagMode.StartTagAndEndTag;
            _readOnlyAttribute = "disabled";
            _xType = "select";
            AscendantIds = new List<string>();
            DescendantIds = new List<string>();
            WithInitScript = true;
            _autoId = true;
        }

        protected override void BeforeProcessing(TagHelperContext context, TagHelperOutput output)
        {
            
            if (string.IsNullOrWhiteSpace(Id))
                Id = context.UniqueId;

            if (SelectType == ESelectType.Boolean)
            {
                ApiUrl = string.Empty;
                AllowManualInput = false;
                ApiUrlMode = string.Empty;
            }

            base.BeforeProcessing(context, output);

            
        }
        
        protected override async void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (SelectType == ESelectType.Boolean)
            {
                if (!_defaultBooleanValue.HasValue)
                    _contents = "<option value=true>" + _trueText + "</option>\r\n" +
                                "<option value=false>" + _falseText + "</option>\r\n";
                else
                    _contents = "<option value=true " + (_defaultBooleanValue.Value ? "selected = 'selected'" : string.Empty) + ">" + _trueText + "</option>\r\n" +
                                "<option value=false " + (!_defaultBooleanValue.Value ? "selected = 'selected'" : string.Empty) + ">" + _falseText + "</option>\r\n";
            }
            else
                _contents = (await output.GetChildContentAsync()).GetContent();
            TagHelperAttributeList attributes = new TagHelperAttributeList();

            if (!string.IsNullOrWhiteSpace(PlaceHolder))
                attributes.SetAttribute("placeholder", PlaceHolder);
            if (ReadOnly)
                attributes.SetAttribute(_readOnlyAttribute, "readonly");
            if (!string.IsNullOrWhiteSpace(Value))
                attributes.SetAttribute("value",Value);
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

            if (SelectType != ESelectType.Boolean)
            {
                if (!string.IsNullOrWhiteSpace(ApiUrl))
                    attributes.SetAttribute("api-url", ApiUrl);
                if (!string.IsNullOrWhiteSpace(ApiUrlMode))
                    attributes.SetAttribute("api-url-mode", ApiUrlMode);
            }

            attributes.SetAttribute("class", _mainInputCssClass);
            attributes.SetAttribute("style", "width: 100%;");
            
            attributes.SetAttribute("selecttype", SelectType);
            if (SelectType == ESelectType.Multiple)
                attributes.SetAttribute("multiple", "multiple");
            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();
            if (!NoCaption)
                contentBuilder.AppendHtml(CaptionHtml);
            contentBuilder.AppendHtml(Helper.CreateElement("select", attributes, _contents));

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
            _initScript = string.Empty;
            WithInitScript = true;

            string strDecendants = string.Empty;
            if (DescendantIds != null)
            {
                if (DescendantIds.Any())
                {
                    foreach (string id in DescendantIds)
                    {
                        strDecendants += $"'{id}',";
                    }
                    strDecendants = "[" + strDecendants.Substring(0, strDecendants.Length - 1) + "]";
                }
            }

            string strAscendants = string.Empty;
            if (AscendantIds != null)
            {
                if (AscendantIds.Any())
                {
                    foreach (string id in AscendantIds)
                    {
                        strAscendants += $"'{id}',";
                    }
                    strAscendants = "[" + strAscendants.Substring(0, strAscendants.Length - 1) + "]";
                }
            }

            string strAscendantFields = string.Empty;
            if (AscendantFieldIds != null)
            {
                if (AscendantFieldIds.Any())
                {
                    foreach (string id in AscendantFieldIds)
                    {
                        strAscendantFields += $"'{id}',";
                    }
                    strAscendantFields = "[" + strAscendantFields.Substring(0, strAscendantFields.Length - 1) + "]";
                }
            }

            if (string.IsNullOrWhiteSpace(strAscendants))
                strAscendants = "null";
            if (string.IsNullOrWhiteSpace(strAscendantFields))
                strAscendantFields = "null";
            if (string.IsNullOrWhiteSpace(strDecendants))
                strDecendants = "null";
            if (string.IsNullOrWhiteSpace(IdField))
                IdField = "Id";
            if (string.IsNullOrWhiteSpace(TextField))
                TextField = "Text";

            _initScript = $"xSelectInit('{Id}',{ReadOnly.ToHtml()}, '{Value}', '{ApiUrl}', '{ApiUrlMode}', {AllowManualInput.ToHtml()},'{IdField}','{TextField}',{strDecendants},{strAscendants},{strAscendantFields});";

            //base.SetInitScript(context, output);
            if (WithInitScript && !string.IsNullOrWhiteSpace(_initScript))
                output.PostContent.SetHtmlContent($"<script>{_initScript}</script>");
        }

    }

    public class XFieldSelectBoolean : XFieldSelect
    {
        public XFieldSelectBoolean() : base()
        {
            SelectType = ESelectType.Boolean;
        }

        public string TrueText
        {
            get { return _trueText; }
            set { _trueText = value; }
        }

        public string FalseText
        {
            get { return _falseText; }
            set { _falseText = value; }
        }

        public bool? DefaultValue
        {
            get { return _defaultBooleanValue; }
            set { _defaultBooleanValue = value; }
        }


    }



}
