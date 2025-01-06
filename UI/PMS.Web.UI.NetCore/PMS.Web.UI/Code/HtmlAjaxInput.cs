using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PMS.Web.UI.Code
{
    public class HtmlAjaxInput : HtmlInput
    {
        public string ApiRouteURL { get; set; }
        public string AjaxMethod { get; set; }
        public string PostData { get; set; }

        public string DataFilter { get; set; }

        public override string ToString()
        {
            return string.Empty;
        }
    }

    public class HtmlAjaxInputSelect : HtmlAjaxInput
    {
        protected readonly string Html =
            @"<div class='form-group{8}'>
                <label>{0}</label>
                <select id='{1}' name='{2}' class='form-control select2' {3} data-placeholder='{4}' {5} style='width: 100%;' {6} data-bf={7}>
                </select>
              </div>";

        protected readonly string HtmlWithButton =
            @"<div class='form-group{8}'>
                <label>{0}</label>
                <div class='input-group'>
                <select id='{1}' name='{2}' class='form-control select2' {3} data-placeholder='{4}' {5} style='width: 100%;' {6} data-bf={7}>
                </select>
                {9}
                </div>
              </div>";

        public string Value { get; set; }
        public bool Multiple { get; set; }
        public bool WithButton { get; set; }
        public string ButtonCaption { get; set; }
        public string ButtonCssClass { get; set; }
        public bool AllowManualInput { get; set; }

        private bool _staticLoad = true;
        public bool StaticLoad
        {
            get { return _staticLoad; }
            set { _staticLoad = value; }
        }
        public string UnselectText { get; set; }

        public override string ReadOnlyAttribute => "disabled";

        public override string JsSetReadonly(bool readOnly)
        {
            
            return base.JsSetReadonly(readOnly);
        }


        public override string InitScript
        {
            get
            {
                string script = string.Empty;
                if (StaticLoad)
                {
                    script = LoadStaticScript;
                }

                else
                {
                    script = string.Format("helper.initSelect2Ajax('{0}','{1}','{2}',{3},{4});",
                                ApiRouteURL, AjaxMethod, Id,
                                string.IsNullOrWhiteSpace(DataFilter) ? "null" : DataFilter,
                                AllowManualInput ? "true" : "null"
                                );
                    script += EventScripts;
                }
                return script;
            }
        }

        public virtual string LoadStaticScript
        {
            get
            {
                string script = string.Format("helper.loadSelect2StaticFromAjax('{0}',{1},'{2}','{3}',{4},'{5}',{6});",
                        ApiRouteURL,
                        string.IsNullOrWhiteSpace(DataFilter) ? "null" : DataFilter,
                        AjaxMethod,
                        Id,
                        string.IsNullOrWhiteSpace(Value) ? "null" : "'" + Value + "'",
                        string.IsNullOrWhiteSpace(UnselectText) ? "--Please Select--" : UnselectText,
                        AllowManualInput ? "true" : "null");
                script += EventScripts;
                return script;
            }
        }

        public override string ToString()
        {
            string button = string.Empty;
            string result = string.Empty;
            if (WithButton)
            {
                button = string.Format("<span class='input-group-btn'><button id='{0}' type='button' class='btn{2} btn-flat'>{1}</button></span>",
                                    Id + "btn", ButtonCaption, " " + ButtonCssClass);
                result = string.Format(HtmlWithButton,
                Caption, Id, Name, (Multiple) ? "multiple='multiple'" : string.Empty,
                PlaceHolder, Attribute, DisabledString, BindingField, IsRequired ? " required" : string.Empty, button);
            }
            else
                result = string.Format(Html,
                    Caption, Id, Name, (Multiple) ? "multiple='multiple'" : string.Empty,
                    PlaceHolder, Attribute, DisabledString, BindingField, IsRequired ? " required" : string.Empty);


            if (WithInitScript)
                result += "<script>" + InitScript + "</script>";
            return result;
        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            jsDisplayFromJson = @"if ({1}) $('#{0}').val({1}).trigger('change');";

            return base.JsDisplayFromJson(jsonObjectName);

        }
    }

    public class HtmlAjaxInputSelectEstate : HtmlAjaxInputSelect
    {
        public override string ToString()
        {
            ApiRouteURL = "api/login/getunitsforselect";
            return base.ToString();
        }
    }

    public class HtmlAjaxInputSelectDivision : HtmlAjaxInputSelect
    {
        public override string ToString()
        {
            ApiRouteURL = "api/login/getdivisionsforselect";
            return base.ToString();
        }
    }

    public class HtmlAjaxInputSelectBlock : HtmlAjaxInputSelect
    {

        public override string ToString()
        {
            ApiRouteURL = "api/login/getblocksforselect";
            return base.ToString();
        }
    }

    public class HtmlAjaxInputSelectMandor1 : HtmlAjaxInputSelect
    {
        public override string ToString()
        {
            ApiRouteURL = "api/login/getmandor1forselect";
            return base.ToString();
        }
    }

    public class HtmlAjaxInputSelectMandor : HtmlAjaxInputSelect
    {
        public override string ToString()
        {
            ApiRouteURL = "api/login/getmandorforselect";
            return base.ToString();
        }
    }
    public class HtmlAjaxInputSelectKerani : HtmlAjaxInputSelect
    {
        public override string ToString()
        {
            ApiRouteURL = "api/login/getkeraniforselect";
            return base.ToString();
        }
    }
    public class HtmlAjaxInputSelectChecker : HtmlAjaxInputSelect
    {
        public override string ToString()
        {
            ApiRouteURL = "api/login/getcheckerforselect";
            return base.ToString();
        }
    }

    public class HtmlAjaxInputSelectEmployee : HtmlAjaxInputSelect
    {
        public override string ToString()
        {
            ApiRouteURL = "api/login/getemployeeforselect";
            return base.ToString();
        }
    }
}
