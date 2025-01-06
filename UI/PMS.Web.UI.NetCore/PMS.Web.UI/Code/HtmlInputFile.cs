using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Web.UI.Code
{
    public class HtmlInputFile : HtmlInput
    {
        public int MaxSize = 0;
        public string PostAPI = string.Empty;
        
        public override string ToString()
        {
            string html = @"
               <div class='form-group'>
                    <label for='{0}'>{1}</label>
                    <div class='input-group'>
                      <div class='custom-file'>
                        <input type='file' class='custom-file-input' id='{0}'>
                        <label class='custom-file-label' for='{0}'>Choose file</label>
                      </div>                      
                    </div>
                  </div>";

            //string html = "<div class='form-group" + (IsRequired ? "required" : string.Empty) + "'>";

            return string.Format(html, Id, Caption);
        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            string jsScript = string.Empty;
        
            return jsScript;
        }

        public override string JsSaveToJson(string jsonObjectName)
        {
            string jsScript = string.Empty;
            
            return jsScript;
        }

    }
}
