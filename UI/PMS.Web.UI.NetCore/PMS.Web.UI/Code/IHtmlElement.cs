using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Web.UI.Code
{
    public class HtmlElement
    {
        public string Id { get; set; }
        private string _name = string.Empty;
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name))
                    return Id;
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string Caption
        { get; set; }
        private List<string> _cssClasses = new List<string>();        
        public List<string> CssClasses
        {
            get
            {
                if (_cssClasses == null)
                    _cssClasses = new List<string>();
                return _cssClasses;
            }
            set
            {
                if (_cssClasses == null)
                    _cssClasses = new List<string>();
                _cssClasses.Clear();
                _cssClasses.AddRange(value);
            }
        }

        public string CssClass
        {
            
            set
            {
                if (_cssClasses == null)
                    _cssClasses = new List<string>();
                _cssClasses.Clear();
                _cssClasses.Add(value);
            }
            get
            {
                if (_cssClasses.Any())
                    return _cssClasses[0];
                return string.Empty;
            }
        }

        public string CssString
        {
            get
            {
                string css = string.Empty;
                foreach (string x in _cssClasses)
                {
                    css += x + " ";
                }
                if (!string.IsNullOrWhiteSpace(css))
                    css = css.Substring(0, css.Length);
                return css;
            }
        }

        public string AttributeString
        {
            get
            {
                string attribute = string.Empty;
                foreach (string x in _attributes)
                {
                    attribute += x + " ";
                }
                if (!string.IsNullOrWhiteSpace(attribute))
                    attribute = attribute.Substring(0, attribute.Length);
                return attribute;
            }
        }
        private List<string> _attributes = new List<string>();
        public List<string> Attributes
        {
            get
            {
                if (_attributes == null)
                    _attributes = new List<string>();
                return _attributes;
            }
            set
            {
                if (_attributes == null)
                    _attributes = new List<string>();
                _attributes.Clear();
                _attributes.AddRange(value);
            }
        }

        public string Attribute
        {

            set
            {
                if (_attributes == null)
                    _attributes = new List<string>();
                _attributes.Clear();
                _attributes.Add(value);
            }
            get
            {
                if (_attributes.Any())
                    return _attributes[0];
                return string.Empty;
            }
        }
    }
}