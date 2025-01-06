using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace PMS.Web.UI.Code
{
    public class HtmlJsonFormatter
    {
        private List<HtmlInput> _children = new List<HtmlInput>();        
        public List<HtmlInput> Children
        {
            set { _children = value; }
            get { return _children; }
        }
        Dictionary<string, string> readonlyScript = new Dictionary<string, string>();
        public void AddChildren(HtmlInput htmlInput)
        {
            if (string.IsNullOrWhiteSpace(htmlInput.Id))
            _children.Add(htmlInput);
        }

        public string Id { get; set; }

        public override string ToString()
        {
            return JsFunction;
        }

        public string JsObject
        {
            get
            {
                if (_children.Count <= 0)
                    return string.Empty;
                string jsObject = string.Empty;
                foreach (HtmlInput input in _children)
                {
                    if (!string.IsNullOrWhiteSpace(input.BindingField))
                        jsObject += input.BindingField + ":'',\r\n";
                }
                
                if (string.IsNullOrWhiteSpace(jsObject))
                    jsObject = "var record = null;";
                else
                    jsObject = "var record = { " + jsObject.Substring(0,jsObject.Length-1) + " };";
                return jsObject;
            }
        }
        public string JsFunction
        {
            get
            {
                

                

                return "<script>\r\n" + JsFunctionDisplay + "\r\n\r\n" + JsFunctionSave + "</script>\r\n";
                
            }
        }
        
        public string JsFunctionDisplayName
        {
            get { return (string.IsNullOrWhiteSpace(Id) ? string.Empty : Id + "_") + "displayJson"; }
        }
        public string JsFunctionDisplay
        {
            get
            {
                string script = "var " + JsFunctionDisplayName + " = function(record) {";
                foreach (HtmlInput input in _children)
                {
                    script += input.JsDisplayFromJson("record") + "\r\n";
                }
                script += "};";
                return script;
            }
            
        }

        public string JsFunctionSaveName
        {
            get { return (string.IsNullOrWhiteSpace(Id) ? string.Empty : Id + "_") + "saveJson"; }
        }
        public string JsFunctionSave
        {
            get
            {
                string script = "var " + JsFunctionSaveName  + " = function() {" + JsObject + "\r\n";
                foreach (HtmlInput input in _children)
                {
                    script += input.JsSaveToJson("record") + "\r\n";
                }
                script += "return record;};";
                return script;
            }
        }



    }
    
}