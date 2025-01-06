using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class ListField
    {
        public string ListID { get; set; }
        public int FieldSequence { get; set; }
        public string FieldName { get; set; }
        public string FieldCaption { get; set; }
        public XFieldType FieldType { get; set; }
        public bool IsKey { get; set; }
        public bool ForSearch { get; set; }
        public XFieldSort Sort { get; set; }
        public string FormatString { get; set; }
        public XFieldAlignment Alignment { get; set; }
        public bool Visible { get; set; }
        public int SortSequence { get; set; }
    }
}
