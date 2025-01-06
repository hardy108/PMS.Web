using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class fn_PMSW_List_GetFields_Result
    {
        public string ListID { get; set; }
        public int FieldSequence { get; set; }
        public string FieldName { get; set; }
        public string FieldCaption { get; set; }
        public int FieldType { get; set; }
        public bool IsKey { get; set; }
        public bool ForSearch { get; set; }
        public int Sort { get; set; }
        public string Format { get; set; }
        public int Alignment { get; set; }
        public bool Visible { get; set; }
        public int SortSequence { get; set; }
    }

}
