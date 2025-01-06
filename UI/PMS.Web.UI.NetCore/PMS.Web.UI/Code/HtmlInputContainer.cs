using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Web.UI.Code
{
    public class HtmlInputContainer:HtmlInput
    {

        List<HtmlInput> _htmlInputs = new List<HtmlInput>();
        public bool Hidden { get; set; }
        public List<HtmlInput> HtmlInputs
        {
            get
            {
                if (_htmlInputs == null)
                    _htmlInputs = new List<HtmlInput>();
                return _htmlInputs;
            }
            set
            {
                _htmlInputs = new List<HtmlInput>();
                _htmlInputs.AddRange(value);
            }
        }
        public HtmlInput HtmlInput
        {
            get
            {
                try
                { return _htmlInputs[0]; }
                catch { return null; }
            }
            set
            {
                _htmlInputs = new List<HtmlInput>();
                _htmlInputs.Add(value);
            }
        }
        public override string ToString()
        {
            string result = "<div id={0}>" + "\n";
            result = string.Format(result, Id);
            foreach (HtmlInput htmlInput in _htmlInputs)
                result += htmlInput.ToString();
            result += "\n" + "</div>";
            return result;
        }
    }

    public class HtmlForm:HtmlInputContainer
    {
        public string Method { get; set; }
        public string Action { get; set; }
        public override string ToString()
        {
            if (Method != "Post")
                Method = "Get";
            if (string.IsNullOrWhiteSpace(Name))
                Name = Id;
            string html = string.Format( "<form id='{0}' name='{1}' method='{2}' action='{3}'>", Id, Name, Method, Action);
            
            foreach (HtmlInput input in HtmlInputs)
            {
                html += input;
            }
            html += "</form>";
            return html;
        }

    }

    public class HtmlFormRow:HtmlInputContainer
    {

        public override string ToString()
        {
            string html = "<div id='{0}' name='{1}' class='row'";
            if (!string.IsNullOrWhiteSpace(Attribute))
                html = "<div id='{0}' name='{1}'  class='row " + Attribute + "'";

            html = string.Format(html, Id, Name);
            if (Hidden)
            {
                html += "style='display:none'";
            }
            html += ">";

            foreach (HtmlInput input in HtmlInputs)
            {
                html += input;
            }
            
            html += "</div>";
            return html;
        }
        
    }

    public class HtmlFormColumn : HtmlInputContainer
    {
        public int XSSize { get; set; }
        public int SMSize { get; set; }
        public int MDSize { get; set; }
        public int LGSize { get; set; }
        public int XLSize { get; set; }

        public override string ToString()
        {
            if (XSSize < 0 || XSSize > 12)
                XSSize = 12;
            if (SMSize < 0 || SMSize > 12)
                SMSize = 12;
            if (MDSize < 0 || MDSize > 12)
                MDSize = 12;
            if (LGSize < 0 || LGSize > 12)
                LGSize = 12;
            if (XLSize < 0 || XLSize > 12)
                XLSize = 12;
            string html = string.Empty;
            if (XSSize != 0)
                html += "col-xs-" + XSSize.ToString() + " ";
            if (SMSize != 0)
                html += "col-sm-" + SMSize.ToString() + " ";
            if (MDSize != 0)
                html += "col-md-" + MDSize.ToString() + " ";
            if (LGSize != 0)
                html += "col-lg-" + LGSize.ToString() + " ";
            if (XLSize != 0)
                html += "col-xl-" + XLSize.ToString() + " ";
            if (!string.IsNullOrWhiteSpace(html))
                html = " class='" + (html + Attribute).Trim() + "'";
            html = "<div Id='{0}' name='{1}' " + html;
            if (Hidden)
            {
                html += "style='display:none'";
            }
            html += ">";

            html = string.Format(html, Id, Name);

            foreach (HtmlInput input in HtmlInputs)
            {
                html += input;                
            }
            html += "</div>";
            return html;
        }
    }
}