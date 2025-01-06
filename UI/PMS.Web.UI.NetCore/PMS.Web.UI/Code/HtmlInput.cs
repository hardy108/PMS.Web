using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PMS.Web.UI.Code
{

    public class HtmlEvent
    {
        public string EventName { get; set; }
        public string EventFunction { get; set; }
    }

    public class HtmlRaw : HtmlInput
    {
        public string Html { get; set; }
        public override string ToString()
        {
            return Html;
        }
    }

    public class HtmlInput : HtmlElement
    {
        HtmlJsonFormatter _jsonFormatter = new HtmlJsonFormatter();

        public HtmlJsonFormatter JsonFormatter
        {
            get { return _jsonFormatter; }
            set
            {
                _jsonFormatter = value;
                _jsonFormatter.Children.Add(this);
            }
        }
        private List<string> _readOnlyModes = new List<string>();
        public List<string> ReadOnlyModes
        {
            get
            {
                if (_readOnlyModes == null)
                    _readOnlyModes = new List<string>();
                return _readOnlyModes;
            }
            set
            {
                _readOnlyModes = new List<string>();
                _readOnlyModes.AddRange(value);
            }
        }
        public bool UseCustomDisplayScripts { get; set; }
        public string CustomDisplayScripts { get; set; }
        public bool UseCustomSaveScripts { get; set; }
        public string CustomSaveScripts { get; set; }
        public string PreDisplayScripts { get; set; }
        public string PostDisplayScripts { get; set; }
        public string PreSaveScripts { get; set; }
        public string PostSaveScripts { get; set; }


        public bool ReadOnly { get; set; }
        public virtual string ReadOnlyAttribute
        {
            get { return "readonly"; }
        }
        public virtual string JsSetReadonly(bool readOnly)
        {

            return string.Format("$('#{0}').attr('{2}', {1});", Id, readOnly ? "true" : "false", ReadOnlyAttribute);
        }
        public string ReadOnlyString
        {
            get
            {

                return ReadOnly ? "readonly" : string.Empty;
            }
        }
        public string DisabledString
        {
            get
            {
                return ReadOnly ? "disabled='disabled'" : string.Empty;
            }
        }

        
        public string PlaceHolder { get; set; }
        private List<HtmlEvent> _events = new List<HtmlEvent>();
        public List<HtmlEvent> Events
        {
            get { return _events; }
            set
            {
                if (value == null)
                    _events = new List<HtmlEvent>();
                else
                    _events = value;
            }
        }
        public string BindingField { get; set; }
        public bool WithInitScript { get; set; }
        public bool IsRequired { get; set; }
        public virtual string EventScripts
        {
            get
            {
                string script = string.Empty;
                foreach (HtmlEvent htmlEvent in _events)
                {
                    script += "$('#" + Id + "').on('" + htmlEvent.EventName + "'," + htmlEvent.EventFunction + ");";
                }
                return script;
            }
        }

        public virtual string InitScript
        {
            get { return string.Empty; }
        }

        public override string ToString()
        {
            return base.ToString();
        }



        public virtual string JsSetValue(string value)
        {
            return string.Empty;
        }
        public virtual string JsSetValueFromVariable(string variableName)
        {
            return string.Empty;
        }


        public virtual string JsGetValue(string variableName)
        {
            return string.Empty;
        }


        protected string jsDisplayFromJson = "if ({1}) $('#{0}').val({1});";



        public virtual string JsDisplayFromJson(string jsonObjectName)
        {
            if (string.IsNullOrWhiteSpace(jsonObjectName) || string.IsNullOrWhiteSpace(BindingField))
                return string.Empty;
            string script = string.Empty;
            if (!string.IsNullOrWhiteSpace(PreDisplayScripts))
                script += PreDisplayScripts + "\r\n";
            if (UseCustomDisplayScripts)
            {
                if (!string.IsNullOrWhiteSpace(CustomDisplayScripts))
                    script += CustomDisplayScripts;
            }
            else
                script += string.Format(jsDisplayFromJson, Id, jsonObjectName + "." + BindingField) + "\r\n";
            if (!string.IsNullOrWhiteSpace(PostDisplayScripts))
                script += PostDisplayScripts + "\r\n";
            return script;
        }

        protected string jsSaveToJson = "{1}=$('#{0}').val();";
        public virtual string JsSaveToJson(string jsonObjectName)
        {
            if (string.IsNullOrWhiteSpace(jsonObjectName) || string.IsNullOrWhiteSpace(BindingField))
                return string.Empty;
            string script = string.Empty;
            if (!string.IsNullOrWhiteSpace(PreSaveScripts))
                script += PreSaveScripts + "\r\n";
            if (UseCustomSaveScripts)
            {
                if (!string.IsNullOrWhiteSpace(CustomSaveScripts))
                    script += CustomSaveScripts;
            }
            else
                script += string.Format(jsSaveToJson, Id, jsonObjectName + "." + BindingField) + "\r\n";
            if (!string.IsNullOrWhiteSpace(PostSaveScripts))
                script += PostSaveScripts + "\r\n";

            return script;
        }
    }



    public class HtmlInputCheckbox : HtmlInput
    {
        public bool Checked
        { get; set; }
        public string Value
        { get; set; }
        protected string optionType = "checkbox";
        protected readonly string Html =
            @"<div class='{8}'>
                    <label>
                      <input type='{8}' id='{1}' name='{2}' value={3} data-bf={4} {5} {6} {7}>
                      {0}
                    </label>
                  </div>";
        public override string ReadOnlyAttribute => "disabled";
        public override string JsSetReadonly(bool readOnly)
        {

            return base.JsSetReadonly(readOnly);
        }
        public override string ToString()
        {
            return string.Format(Html, Caption, Id, Name, Value, BindingField, DisabledString, Attribute, Checked ? "checked='checked'" : string.Empty, optionType);
        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            jsDisplayFromJson =
            @"if ({1}==true) $('#{0}').prop('checked',true);
              else $('#{0}').prop('checked',true)=false;";
            return base.ToString();
        }
        public override string JsSaveToJson(string jsonObjectName)
        {
            jsSaveToJson =
            @"if ($('#{0}').prop('checked')==true)) {1}=true;
              else {1}=false;";
            return base.ToString();
        }
    }

    public class HtmlInputCheckboxGroup : HtmlInput
    {
        private List<HtmlInputCheckbox> _listCheckboxes = new List<HtmlInputCheckbox>();
        public List<HtmlInputCheckbox> ListCheckBoxes
        {
            get { return _listCheckboxes; }
            set { _listCheckboxes = value; }
        }
        public override string ToString()
        {
            string html = "<div class='form-group'>";
            foreach (HtmlInputCheckbox item in _listCheckboxes)
            {
                html += item.ToString();
            }
            return html + "</div>";
        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            if (string.IsNullOrWhiteSpace(jsonObjectName) || string.IsNullOrWhiteSpace(BindingField))
                return string.Empty;

            string jsScript = string.Empty;
            foreach (HtmlInputCheckbox item in _listCheckboxes)
            {
                jsScript += item.JsDisplayFromJson(jsonObjectName);
            }
            return jsScript;
        }

        public override string JsSaveToJson(string jsonObjectName)
        {
            if (string.IsNullOrWhiteSpace(jsonObjectName) || string.IsNullOrWhiteSpace(BindingField))
                return string.Empty;

            string jsScript = string.Empty;
            foreach (HtmlInputCheckbox item in _listCheckboxes)
            {
                jsScript += item.JsSaveToJson(jsonObjectName);
            }
            return jsScript;
        }

    }

    public class HtmlInputOption : HtmlInputCheckbox
    {
        public override string ToString()
        {
            optionType = "radio";
            return base.ToString();
        }


        public override string JsDisplayFromJson(string jsonObjectName)
        {
            jsDisplayFromJson =
            @"if ({1}==$('#{0}').val()) $('#{0}').prop('checked',true);";

            return base.ToString();
        }
        public override string JsSaveToJson(string jsonObjectName)
        {
            jsSaveToJson =
            @"if ($('#{0}').prop('checked')==true)) {1}=$('#{0}').val();";

            return base.ToString();
        }
    }
    public class HtmlInputOptionGroup : HtmlInput
    {
        private List<HtmlInputOption> _listOptions = new List<HtmlInputOption>();
        public List<HtmlInputOption> ListOptions
        {
            get { return _listOptions; }
            set { _listOptions = value; }
        }
        public override string ToString()
        {
            string html = "<div class='form-group" + (IsRequired ? "required" : string.Empty) + "'>";
            foreach (HtmlInputOption item in _listOptions)
            {

                item.Name = this.Name;
                html += item.ToString();
            }
            return html + "</div>";
        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            string jsScript = string.Empty;
            foreach (HtmlInputOption item in _listOptions)
            {
                jsScript += item.JsDisplayFromJson(jsonObjectName);
            }
            return jsScript;
        }

        public override string JsSaveToJson(string jsonObjectName)
        {
            string jsScript = string.Empty;
            foreach (HtmlInputOption item in _listOptions)
            {
                jsScript += item.JsSaveToJson(jsonObjectName);
            }
            return jsScript;
        }

    }

    public class HtmlInputText : HtmlInput
    {
        protected string Html =
            @"<div class='form-group{9}'>
                <label>{0}</label>                
                <input type='{8}' id='{1}' name='{2}' class='form-control' value='{3}' placeholder='{4}' {5} data-bf={6} {7}/>
              </div>";

        protected string HtmlWithButton =
            @"<div class='form-group{9}'>
                <label>{0}</label>
                <div class='input-group'>
                <input type='{8}' id='{1}' name='{2}' class='form-control' value='{3}' placeholder='{4}' {5} data-bf={6} {7}/>                                
                {10}
                </div>
              </div>";

        protected string inputType = "text";

        public string Value { get; set; }
        public bool WithButton { get; set; }
        public string ButtonCaption { get; set; }
        public string ButtonCssClass { get; set; }
        public override string ToString()
        {
            string button = string.Empty;
            string result = string.Empty;
            if (WithButton)
            {
                button = string.Format("<span class='input-group-btn'><button id='{0}' type='button' class='btn{2} btn-flat'>{1}</button></span>",
                                    Id + "btn", ButtonCaption, " " + ButtonCssClass);
                return string.Format(HtmlWithButton,
                Caption, Id, Name, Value, PlaceHolder, ReadOnlyString, BindingField, Attribute, inputType, IsRequired ? " required" : string.Empty, button);
            }
            return string.Format(Html,
                Caption, Id, Name, Value, PlaceHolder, ReadOnlyString, BindingField, Attribute, inputType, IsRequired ? " required" : string.Empty);
        }
    }

    public class HtmlInputPassword : HtmlInputText
    {
        public override string ToString()
        {
            inputType = "password";
            return base.ToString();
        }
    }

    public class HtmlInputNumber : HtmlInputText
    {
        public int DecimalDigit { get; set; }
        public bool OnlyInteger { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public int Step { get; set; }

        public override string ToString()
        {
            inputType = "number";
            string step = "any";
            if (OnlyInteger)
            {
                step = "1";

            }
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

            Attribute += " step='" + step + "' ";

            return base.ToString();
        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            jsDisplayFromJson = "if ({1}) $('#{0}').val({1}); else $('#{0}').val(0);";
            return base.JsDisplayFromJson(jsonObjectName);
        }
    }

    public class HtmlInputEmail : HtmlInputText
    {
        public override string ToString()
        {
            inputType = "email";
            return base.ToString();
        }
    }

    public class HtmlInputHidden : HtmlInputText
    {
        public override string ToString()
        {
            inputType = "hidden";
            Html = "<input type='{8}' id='{1}' name='{2}' class='form-control' value='{3}' placeholder='{4}' {5} data-bf={6} {7}></input>";
            return base.ToString();
        }
    }

    public class HtmlInputDateTimeReadOnly : HtmlInputText
    {
        protected string FormatJs = "DD-MMM-YYYY HH:mm";
        protected string Format = "yyyy-MM-dd HH:mm:ss";
        public override string ToString()
        {
            ReadOnly = true;
            return base.ToString();
        }
        public override string JsDisplayFromJson(string jsonObjectName)
        {
            jsDisplayFromJson = "if ({1}) $('#{0}').val(moment({1}).format('" + FormatJs + "'));";
            return base.JsDisplayFromJson(jsonObjectName);
        }
    }

    public class HtmlInputDateReadOnly : HtmlInputDateTimeReadOnly
    {


        public override string JsDisplayFromJson(string jsonObjectName)
        {
            FormatJs = "DD-MMM-YYYY";
            Format = "yyyy-MM-dd";
            return base.JsDisplayFromJson(jsonObjectName);
        }

    }



    public class HtmlInputTimeReadOnly : HtmlInputDateTimeReadOnly
    {


        public override string JsDisplayFromJson(string jsonObjectName)
        {
            FormatJs = "HH:mm";
            Format = "HH:mm";
            return base.JsDisplayFromJson(jsonObjectName);
        }

    }



    public class HtmlInputTimeReadonly : HtmlInputDateTimeReadOnly
    {



        public override string JsDisplayFromJson(string jsonObjectName)
        {
            FormatJs = "HH:mm";
            Format = "HH:mm";
            return base.JsDisplayFromJson(jsonObjectName);
        }
    }

    public enum HtmlInputDateTimePickerOption
    {
        DateTime = 0,
        DateOnly = 1,
        TimeOnly = 2,
        MonthOnly = 3
    }
    public class HtmlInputDateTimePicker : HtmlInput
    {
        protected readonly string Html =
            @"
            <div class='form-group{8}'>
                <label>{0}</label>
                <div class='input-group'>        
                    <input type='text' class='form-control pull-right' id='{1}' name={2} placeholder='{3}' data-bf={4} {5} {6}>
                    <span class='input-group-addon'>
                        <span class='glyphicon glyphicon-{7}'></span>
                    </span>
                </div>
                <!-- /.input group -->
            </div>";

        public DateTime? Value { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

        private HtmlInputDateTimePickerOption _pickerOption = HtmlInputDateTimePickerOption.DateTime;
        protected string FormatJs = "DD-MMM-YYYY HH:mm";
        protected string Format = "yyyy-MM-dd HH:mm:ss";
        protected string Icon = "calendar";
        public HtmlInputDateTimePickerOption PickerOption
        {
            get
            {
                return _pickerOption;
            }
            set
            {
                _pickerOption = value;
                switch (_pickerOption)
                {
                    case HtmlInputDateTimePickerOption.DateOnly:
                        FormatJs = "DD-MMM-YYYY";
                        Format = "yyyy-MM-dd";
                        Icon = "calendar";
                        break;
                    case HtmlInputDateTimePickerOption.TimeOnly:
                        FormatJs = "HH:mm";
                        Format = "yyyy-MM-dd HH:mm";
                        Icon = "time";
                        break;
                    case HtmlInputDateTimePickerOption.MonthOnly:
                        FormatJs = "MMMM YYYY";
                        Format = "yyyy-MM-dd";
                        Icon = "calendar";
                        break;
                    default:
                        FormatJs = "DD-MMM-YYYY HH:mm";
                        Format = "yyyy-MM-dd HH:mm:ss";
                        Icon = "calendar";
                        break;
                }
            }
        }

        public override string InitScript
        {
            get
            {

                return string.Format("helper.initDateTimePicker({0},{1},{2},{3});",
                    "'" + Id + "'",
                    MinDate.HasValue ? "'" + MinDate.Value.ToString(Format) + "'" : "null",
                    MaxDate.HasValue ? "'" + MaxDate.Value.ToString(Format) + "'" : "null",
                    "'" + FormatJs + "'");
            }
        }
        public override string ToString()
        {

            string result = string.Format(Html,
                Caption, Id, Name, PlaceHolder,
                BindingField,
                Attribute,
                ReadOnlyString,
                Icon,
                IsRequired ? " required" : string.Empty
                );
            if (WithInitScript)
            {
                result += "<script>" +
                            InitScript +
                            EventScripts +
                          "</script>";
            }
            return result;
        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            jsDisplayFromJson = "if ({1}) {{helper.setValueDateTimePicker('{0}',{1},'" + FormatJs + "');}}";
            //if (PickerOption== HtmlInputDateTimePickerOption.TimeOnly)
            //    jsDisplayFromJson = "if ({1}) {{var d" + Id + " = helper.convertStringToTime({1}); $('#{0}').data('DateTimePicker').date(d" + Id + ");}}";
            //else
            //    jsDisplayFromJson = "if ({1}) {{var d" + Id + " = new Date({1}); $('#{0}').data('DateTimePicker').date(d" + Id + ");}}";

            return base.JsDisplayFromJson(jsonObjectName);
        }
    }



    public class HtmlInputDateTimeRangePicker : HtmlInputDateTimePicker
    {
        public DateTime? Value2 { get; set; }
        public string BindingField2 { get; set; }
        public string PlaceHolder2 { get; set; }
        public string Caption2 { get; set; }


        private HtmlInputDateTimePicker _startInput = null;
        private HtmlInputDateTimePicker _endInput = null;

        private void InitStartInput()
        {
            if (_startInput == null)
            {
                _startInput =
                    new HtmlInputDateTimePicker
                    {
                        Attribute = this.Attribute,
                        BindingField = this.BindingField,
                        Caption = this.Caption,
                        Events = this.Events,
                        Id = this.Id + "Start",
                        MaxDate = this.MaxDate,
                        MinDate = this.MinDate,
                        Name = this.Name + "Start",
                        PlaceHolder = this.PlaceHolder,
                        ReadOnly = this.ReadOnly,
                        Value = this.Value,
                        PickerOption = this.PickerOption,
                        IsRequired = this.IsRequired
                    };
            }
        }

        private void InitEndInput()
        {
            if (_endInput == null)
            {
                _endInput = new HtmlInputDateTimePicker
                {
                    Attribute = this.Attribute,
                    BindingField = this.BindingField2,
                    Caption = string.IsNullOrWhiteSpace(Caption2) && !string.IsNullOrWhiteSpace(Caption) ? " " : this.Caption2,
                    Events = this.Events,
                    Id = this.Id + "End",
                    MaxDate = this.MaxDate,
                    MinDate = this.MinDate,
                    Name = this.Name + "End",
                    PlaceHolder = this.PlaceHolder2,
                    ReadOnly = this.ReadOnly,
                    Value = this.Value2,
                    PickerOption = this.PickerOption,
                    IsRequired = this.IsRequired
                };
            }

        }

        public override string InitScript
        {
            get
            {
                return string.Format("helper.initDateRange({0},{1},{2},{3});",
                    "'" + Id + "'",
                    MinDate.HasValue && PickerOption != HtmlInputDateTimePickerOption.TimeOnly ? "'" + MinDate.Value.ToString(Format) + "'" : "null",
                    MaxDate.HasValue && PickerOption != HtmlInputDateTimePickerOption.TimeOnly ? "'" + MaxDate.Value.ToString(Format) + "'" : "null",
                    "'" + FormatJs + "'");
            }
        }

        public override string ToString()
        {
            InitStartInput();
            InitEndInput();
            string html = "<div class='row no-gutters'><div class='col-xs-6'>" +
                        _startInput.ToString() +
                    "</div>" +
                    "<div class='col-xs-6'>" +
                        _endInput.ToString() +
                    "</div></div>";



            if (WithInitScript)
            {
                html += "<script>" +
                            InitScript +
                          "</script>";
            }
            return html;


        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            InitStartInput();
            InitEndInput();
            return _startInput.JsDisplayFromJson(jsonObjectName) + _endInput.JsDisplayFromJson(jsonObjectName);
        }

        public override string JsSaveToJson(string jsonObjectName)
        {
            InitStartInput();
            InitEndInput();
            return _startInput.JsSaveToJson(jsonObjectName) + _endInput.JsSaveToJson(jsonObjectName);
        }

        public override string JsSetReadonly(bool readOnly)
        {
            return _startInput.JsSetReadonly(readOnly) + _endInput.JsSetReadonly(readOnly);
        }
    }



    public class HtmlInputSelect : HtmlInput
    {
        protected readonly string Html =
            @"<div class='form-group{9}'>
                <label>{0}</label>
                <select id = '{1}' name='{2}' class='form-control select2' {3} data-placeholder='{4}' {5} style='width: 100%;' data-bf={7} {8}>
                {6}
                </select>
              </div>";

        protected string HtmlWithButton =
            @"<div class='form-group{9}'>
                <label>{0}</label>
                <div class='input-group'>
                <select id = '{1}' name='{2}' class='form-control select2' {3} data-placeholder='{4}' {5} style='width: 100%;' data-bf={7} {8}>    
                {6}
                </select>
                {10}
                </div>
              </div>";

        public bool WithButton { get; set; }
        public string ButtonCaption { get; set; }
        public string ButtonCssClass { get; set; }

        public bool AllowManualInput { get; set; }
        public string Value { get; set; }
        public bool Multiple { get; set; }
        public string UnselectText { get; set; }
        List<HtmlSelectOptionItem> _optionList = new List<HtmlSelectOptionItem>();
        public List<HtmlSelectOptionItem> OptionList
        {
            get { return _optionList; }
            set { _optionList = value; }
        }
        public override string ReadOnlyAttribute => "disabled";
        public override string JsSetReadonly(bool readOnly)
        {

            return base.JsSetReadonly(readOnly);
        }
        public override string ToString()
        {
            string optionTexts = string.Empty;
            foreach (HtmlSelectOptionItem selectOption in _optionList)
            {
                optionTexts += string.Format("<option value={0} {2}>{1}</option>",
                        selectOption.Id,
                        selectOption.Text,
                        selectOption.Id.Equals(Value) ? "selected='selected'" : string.Empty);
            }

            string result = string.Empty;
            string button = string.Empty;
            if (WithButton)
            {
                button = string.Format("<span class='input-group-btn'><button id='{0}' type='button' class='btn{2} btn-flat'>{1}</button></span>",
                                    Id + "btn", ButtonCaption, " " + ButtonCssClass);
                result = string.Format(HtmlWithButton, Caption, Id, Name, (Multiple) ? "multiple='multiple'" : string.Empty,
                                       PlaceHolder, Attribute, optionTexts, BindingField, ReadOnlyString, IsRequired ? " required" : string.Empty, button);
            }
            else
                result = string.Format(Html, Caption, Id, Name, (Multiple) ? "multiple='multiple'" : string.Empty,
                                       PlaceHolder, Attribute, optionTexts, BindingField, ReadOnlyString, IsRequired ? " required" : string.Empty);

            if (WithInitScript)
                result += "<script>" + InitScript + "</script>";

            return result;
        }

        public override string InitScript
        {
            get
            {
                string script = string.Empty;
                if (AllowManualInput)
                    script = @"{
                        createSearchChoice: function (term, data) {
                            if ($(data).filter(function () {
                                return this.text.localeCompare(term) === 0;
                            }).length === 0) {
                                return {
                                    id: term,
                                    text: term
                                };
                            }
                        }
                    }";


                return "$('#" + Id + "').select2(" + script + ");" + EventScripts;
            }

        }


        public override string JsDisplayFromJson(string jsonObjectName)
        {
            return string.Format("$('#{0}').val({1}).trigger('change');", Id, jsonObjectName + "." + BindingField);
        }
    }

    public class HtmlInputTextArea : HtmlInput
    {
        string Html = @"<div class='form-group{8}'>
                        <label>{0}</label>
                        <textarea id='{1}' name='{2}' class='form-control' rows='{7}' placeholder='{4}' {5} data-bf='{6}' {9}>{3}</textarea>
                       </div>";
        public int Rows { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return string.Format(Html, Caption, Id, Name, Value, PlaceHolder, Attribute, BindingField, Rows, IsRequired ? " required" : string.Empty, ReadOnlyString);
        }
    }

    

}