using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class DTColumn
    {
        public DTColumn() { }
        public DTColumn(ListField listField)
        {
            if (listField == null)
                return;
            name = listField.FieldName;
            data = name;
            title = listField.FieldCaption;
            visible = listField.Visible;
            orderable = (listField.Sort == 0) ? false : true;
            switch (listField.FieldType)
            {
                case XFieldType.FText:
                    type = "string";
                    break;
                case XFieldType.FInteger:
                case XFieldType.FFloat:
                    type = "num-fmt";
                    break;
                case XFieldType.FBool:
                    type = "boolean";
                    break;
                case XFieldType.FDate:
                    type = "date";
                    break;
                default:
                    type = "";
                    break;
            }
            formatString = listField.FormatString;
            switch (listField.Alignment)
            {
                case XFieldAlignment.FLeft:
                    className = "tdt-head-center dt-body-left";
                    break;
                case XFieldAlignment.FCenter:
                    className = "dt-center";
                    break;
                case XFieldAlignment.FRight:
                    className = "dt-head-center dt-body-right";
                    break;

                default:
                    className = "";
                    break;
            }
        }
        public string name { get; set; }
        public string data { get; set; }
        public string title { get; set; }
        public bool visible { get; set; } //true,false
        public bool filterable { get; set; } //true,false
        public bool orderable { get; set; } //true, false
        public string type { get; set; } //date, number
        public string className { get; set; } //custom style
        public string formatString { get; set; }
    }
}
