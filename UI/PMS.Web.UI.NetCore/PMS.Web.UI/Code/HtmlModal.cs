using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Web.UI.Code
{
    public class HtmlModal
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public bool UsingGridSystem { get; set; }
        List<HtmlElement> _bodyHtmlInputs = new List<HtmlElement>();
        public List<HtmlElement> BodyHtmlInputs
        {
            get
            {
                if (_bodyHtmlInputs == null)
                    _bodyHtmlInputs = new List<HtmlElement>();
                return _bodyHtmlInputs;
            }
            set
            {
                _bodyHtmlInputs = new List<HtmlElement>();
                _bodyHtmlInputs.AddRange(value);
            }
        }

        List<HtmlElement> _footerHtmlInputs = new List<HtmlElement>();
        public List<HtmlElement> FooterHtmlInputs
        {
            get
            {
                if (_footerHtmlInputs == null)
                    _footerHtmlInputs = new List<HtmlElement>();
                return _footerHtmlInputs;
            }
            set
            {
                _footerHtmlInputs = new List<HtmlElement>();
                _footerHtmlInputs.AddRange(value);
            }
        }
        protected string Html = @"
        <div class='modal fade' id='{0}'>
            <div class='modal-dialog' style='width:80%;'>
                <div class='modal-content'>
                    <div class='modal-header'>
                        <button type='button' class='close' data-dismiss='modal' aria-label='Close'>
                            <span aria-hidden='true'>&times;</span>
                        </button>
                        <h4 class='modal-title'>{1}</h4>
                    </div>
                    <div class='modal-body'>
                    {2}
                    </div>
                    <div class='modal-footer'>
                        {3}                    
                    </div>
                </div>
            </div>
        </div>";
        public override string ToString()
        {
            string bodyInputs = string.Empty;
            foreach (HtmlElement input in _bodyHtmlInputs)
            {
                bodyInputs += input;
            }

            if (UsingGridSystem)            
                bodyInputs = "<div class='container-fluid'>" + bodyInputs + "</div>";
            
            
            string footerInputs = string.Empty;
            foreach (HtmlElement input in _footerHtmlInputs)
            {
                footerInputs += input;
            }
            return string.Format(Html, Id, Caption, bodyInputs, footerInputs);
        }
    }
}