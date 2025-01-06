using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PMS.Web.UI.TagHelpers
{
    public enum EDateTimePickerOption
    {
        DateTime = 0,
        DateOnly = 1,
        TimeOnly = 2
    }

    public class XFieldDate:XFieldInput
    {
        
        private DateTime? _dateValue;
        private DateTime? _minDateValue;
        private DateTime? _maxDateValue;
        private string _displayFormat;
        private string _displayFormatJS;


        public string MaxDate { get; set; }
        public string MinDate { get; set; }
        
        private string ValueFormat
        {
            get
            {
                switch (PickerOption)
                {
                    case EDateTimePickerOption.DateOnly:
                        return "yyyy-MM-dd";
                    case EDateTimePickerOption.TimeOnly:
                        return "HH:mm:ss";                    
                    default:
                        return "yyyy-MM-dd HH:mm:ss";
                }
            }
        }

        private string DefaultDateFormat
        {
            get
            {
                if (PickerOption != EDateTimePickerOption.TimeOnly)
                    return "dd-MMM-yyyy";
                return string.Empty;
            }
        }
        private string DefaultTimeFormat
        {
            get
            {
                if (PickerOption != EDateTimePickerOption.DateOnly)
                    return "HH:mm:ss";
                return string.Empty;
            }
        }

        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }



        public EDateTimePickerOption PickerOption
        {
            get;
            set;
        }
        protected string _iconName = "calendar";
        
        public XFieldDate():base()
        {
            _xType = "xdate";
            PickerOption = EDateTimePickerOption.DateTime;
            _mainInputCssClass = "form-control pull-right";
            WithInitScript = true;
            
        }

       

        



        
        
        protected override TagHelperAttributeList SetAttributeForMainInput(TagHelperContext context, TagHelperOutput output)
        {
            TagHelperAttributeList attributes = base.SetAttributeForMainInput(context, output);
            attributes.SetAttribute("display-format", _displayFormatJS);
            if (_dateValue.HasValue)
                attributes.SetAttribute("value", _dateValue.Value.ToString(ValueFormat));
            return attributes;

        }

        private void BuildInternalProperties()
        {
            DateTime date;

            _dateValue = null;
            _minDateValue = null;
            _maxDateValue = null;


            if (DateTime.TryParse(Value, out date))
                _dateValue = date;

            if (DateTime.TryParse(MaxDate, out date))
                _maxDateValue = date;
            if (DateTime.TryParse(MinDate, out date))
                _minDateValue = date;


            string _dateFormat = string.Empty;
            if (string.IsNullOrWhiteSpace(DateFormat))
                _dateFormat = DefaultDateFormat.Replace("D", "d").Replace("m", "M").Replace("Y", "y");
            else
                _dateFormat = DateFormat.Replace("D", "d").Replace("m", "M").Replace("Y", "y");

            string _timeFormat = string.Empty;
            if (string.IsNullOrWhiteSpace(TimeFormat))
                _timeFormat = DefaultTimeFormat.Replace("M", "m");
            else
                _timeFormat = TimeFormat.Replace("M", "m");

            if (PickerOption == EDateTimePickerOption.DateTime)
            {
                _displayFormat = $"{_dateFormat} {_timeFormat}";
                _displayFormatJS = $"{_dateFormat.Replace("d", "D").Replace("m", "M").Replace("y", "Y")} {_timeFormat.Replace("M", "m")}";
            }
            else if (PickerOption == EDateTimePickerOption.DateOnly)
            {
                _displayFormat = _dateFormat;
                _displayFormatJS = _dateFormat.Replace("d", "D").Replace("m", "M").Replace("y", "Y");
            }
            else
            {
                _displayFormat = _timeFormat;
                _displayFormatJS = _timeFormat.Replace("M", "m");
            }


        
        }

        protected override void BuildContentHtmlAsync(TagHelperContext context, TagHelperOutput output)
        {
            BuildInternalProperties();
            TagHelperAttributeList attributes = SetAttributeForMainInput(context, output);

            HtmlContentBuilder contentBuilder = new HtmlContentBuilder();
            if (!NoCaption)
                contentBuilder.AppendHtml(CaptionHtml);

            
            HtmlContentBuilder mainInput = new HtmlContentBuilder();
            mainInput
                .AppendHtml(Helper.CreateElement("input", attributes, string.Empty));

            if (ReadOnly)
            {
                contentBuilder.AppendHtml(mainInput);
            }
            else
            {
                mainInput.AppendHtml($"<span class='input-group-addon'><span class='glyphicon glyphicon-{_iconName}'></span></span>");
                contentBuilder.AppendHtml(Helper.WrapElementsWithDiv(mainInput, "input-group"));
            }
            

            if (string.IsNullOrWhiteSpace(_innerCssClss))
                _contents = contentBuilder.GetHtmlContent();
            else
                _contents = Helper.WrapElementsWithDiv(contentBuilder, _innerCssClss).GetHtmlContent();


            string minDateString = "null", maxDateString = "null";

            if (_minDateValue.HasValue)
                minDateString = $"'{_minDateValue.Value.ToString(ValueFormat)}'";
            if (_maxDateValue.HasValue)
                maxDateString = $"'{_maxDateValue.Value.ToString(ValueFormat)}'";

            _initScript = $"helper.initDateTimePicker('{Id}',{minDateString},{maxDateString},'{_displayFormatJS}',{(ReadOnly ? "true" : "false")});";
            if (_dateValue.HasValue)
                _initScript += $"\r\nhelper.setValueDateTimePicker('{Id}','{_dateValue.Value.ToString("yyyy-MM-dd HH:mm:ss")}','{_displayFormatJS}',null,{(ReadOnly ? "true" : "false")});";
            _initScript += $"xRegisterEvent('{Id}','dp.change');\r\n";

        }
    }
}
