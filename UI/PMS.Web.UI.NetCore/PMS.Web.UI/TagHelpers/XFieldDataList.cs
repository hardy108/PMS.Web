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
using PMS.Shared.Models;

namespace PMS.Web.UI.TagHelpers
{

    public class XFieldDataList : BaseElement
    {
        protected string _inputType = "text";
        protected string _defaultInnerDivClass = "input-group";
        protected string _mainInputCssClass = "form-control flexdatalist";

        private string _containerId = string.Empty;
        protected int maxRows = 100; //Only show 100 rows
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

        public bool Multiple { get; set; }
        public string ApiUrl { get; set; }
        public string ApiUrlParam { get; set; }
        public string ValueField { get; set; }
        public string TextFields { get; set; }
        public string SearchFields { get; set; }
        public int? MinLengthSearch { get; set; }
        public bool SearchContain { get; set; }
        public bool SearchByWord { get; set; }
        public bool SearchDisabled { get; set; }
        public string VisibleFields { get; set; }
        public string GroupingFields { get; set; }
        public List<FilterAscendant> Ascendants { get; set; }
        public bool ChainToRelatives { get; set; }

        public XFieldDataList()
        {

            _tagName = "div";
            _mainCssClass = "form-group";
            _tagMode = TagMode.StartTagAndEndTag;
            _readOnlyAttribute = "readonly";
            _xType = "xdatalist";
            WithInitScript = true;
            _autoId = true;
            Ascendants = new List<FilterAscendant>();

        }


        protected override void SetAttribute(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(ContainerId))
                output.Attributes.SetAttribute("id", ContainerId);

        }
        protected override async void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {

            string postContents = (await output.GetChildContentAsync()).GetContent();
            if (!string.IsNullOrWhiteSpace(postContents))
                postContents = $"<datalist id = '{Id}DataList'>{postContents}</datalist>";

            TagHelperAttributeList attributes = new TagHelperAttributeList();
            if (!string.IsNullOrWhiteSpace(postContents))
                attributes.SetAttribute("list", $"{Id}DataList");
            if (!string.IsNullOrWhiteSpace(PlaceHolder) && !ReadOnly)
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
            if (SearchDisabled)
                attributes.SetAttribute("data-search-disabled", "1");
            if (!string.IsNullOrWhiteSpace(SearchFields))
                attributes.SetAttribute("data-search-in", SearchFields);
            if (!string.IsNullOrWhiteSpace(TextFields))
                attributes.SetAttribute("data-text-property", TextFields);
            if (!string.IsNullOrWhiteSpace(ValueField))
                attributes.SetAttribute("data-value-property", ValueField);
            else
                attributes.SetAttribute("data-value-property", "*");

            if (SearchContain)
                attributes.SetAttribute("data-search-contain", SearchContain);
            if (SearchByWord)
                attributes.SetAttribute("data-search-by-word", SearchByWord);
            if (MinLengthSearch.HasValue)
                attributes.SetAttribute("data-min-length", MinLengthSearch.Value);
            if (!string.IsNullOrWhiteSpace(VisibleFields))
                attributes.SetAttribute("data-visible-properties", VisibleFields);
            if (!string.IsNullOrWhiteSpace(GroupingFields))
                attributes.SetAttribute("data-group-by", GroupingFields);

            bool hasAccendants = false;
            if (Ascendants != null && Ascendants.Count > 0 && !ReadOnly)
            {
                string relatives = string.Empty, relativeFields = string.Empty;
                Ascendants.ForEach(d =>
                {
                    if (!string.IsNullOrWhiteSpace(d.ElementID))
                    {
                        relatives += d.ElementID + ",";
                        relativeFields += (string.IsNullOrWhiteSpace(d.FieldID) ? d.ElementID : d.FieldID) + ",";
                    }
                });
                if (!string.IsNullOrWhiteSpace(relatives))
                {
                    attributes.SetAttribute("data-relative-elements", relatives.Substring(0, relatives.Length - 1));
                    attributes.SetAttribute("data-relative-fields", relativeFields.Substring(0, relativeFields.Length - 1));
                    hasAccendants = true;
                }


            }

            if (ChainToRelatives & !ReadOnly && hasAccendants)
                attributes.SetAttribute("data-chained-relatives", "true");
            if (!string.IsNullOrWhiteSpace(ApiUrl))
            {
                string apiUrl = ApiUrl;
                if (!string.IsNullOrWhiteSpace(ApiUrlParam))
                {
                    if (apiUrl.Contains("?"))
                        apiUrl = $"{apiUrl}&{ApiUrlParam}";
                    else
                        apiUrl = $"{apiUrl}?{ApiUrlParam}";
                }
                if (maxRows > 0)
                {
                    string pagingParam = $"PageSize={maxRows}&PageNo=1";
                    if (apiUrl.Contains("?"))
                        apiUrl = $"{apiUrl}&{pagingParam}";
                    else
                        apiUrl = $"{apiUrl}?{pagingParam}";
                }

                attributes.SetAttribute("data-url", apiUrl);

            }

            attributes.SetAttribute("class", _mainInputCssClass);
            attributes.SetAttribute("style", "width: 100%;");
            attributes.SetAttribute("multiple", "multiple");
            if (!Multiple)
                attributes.SetAttribute("data-limit-of-values", "1");

            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();
            if (!NoCaption)
                contentBuilder.AppendHtml(CaptionHtml);
            contentBuilder.AppendHtml(Helper.CreateElement("input", attributes, string.Empty));
            contentBuilder.AppendHtml(postContents);
            if (string.IsNullOrWhiteSpace(_innerCssClss))
                _contents = contentBuilder.GetHtmlContent();
            else
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
            WithInitScript = true;
            if (ReadOnly)
                _initScript = $"xDataListInit('{Id}',null,true);";
            else
                _initScript = $"xDataListInit('{Id}');";
            base.SetInitScript(context, output);

        }
    }
}