using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class FTColumn
    {
        public FTColumn() { }
        public FTColumn(ListField listField)
        {
            if (listField == null)
                return;
            name = listField.FieldName;
            title = listField.FieldCaption;
            visible = listField.Visible;
            sortable = (listField.Sort == XFieldSort.FNoSort) ? false : true;



            switch (listField.FieldType)
            {
                case XFieldType.FDate:
                    xtype = "date";
                    xformatString = listField.FormatString;
                    break;                
                case XFieldType.FBool:
                    xtype = "boolean";                    
                    break;
                case XFieldType.FInteger:
                    xtype = "integer";
                    break;
                case XFieldType.FFloat:
                    xtype = "float";
                    break;


            }

            switch (listField.Alignment)
            {
                case XFieldAlignment.FLeft:
                    style = "text-align:left";
                    break;
                case XFieldAlignment.FCenter:
                    style = "text-align:center";
                    break;
                case XFieldAlignment.FRight:
                    style = "text-align:right";
                    break;

                default:
                    style = "";
                    break;
            }
        }
        public string name { get; set; }
        public string title { get; set; }
        public string breakpoints { get; set; }
        public bool visible { get; set; } //true,false
        public bool filterable { get; set; } //true,false
        public bool sortable { get; set; } //true, false
        public string xtype { get; set; } //date, number
        public string type { get; set; } //date, number
        public string style { get; set; } //custom style
        public string xformatString { get; set; }
        public string formatter { get; set; }
    }
}
