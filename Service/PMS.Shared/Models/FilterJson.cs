using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public class FilterAscendant
    {
        public string ElementID { get; set; }
        public string FieldID { get; set; }
    }
    public class FilterJsonField
    {
        public string Id { get; set; }
        public string Id2 { get; set; }
        public string Caption { get; set; }
        public string Caption2 { get; set; }
        public string Type { get; set; }
        public string ApiUrl { get; set; }
        public List<FilterAscendant> Ascendants { get; set; }

        public string QueryFilter { get; set; }
        public int XSSize { get; set; }
        public int SMSize { get; set; }
        public int MDSize { get; set; }
        public int LGSize { get; set; }
        public int XLSize { get; set; }

        private string _textField = string.Empty;
        public string TextField
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_textField))
                    return "Text";
                return _textField;
            }
            set { _textField = value; }
        }
        private string _valueField = string.Empty;
        public string ValueField
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_valueField))
                    return "Id";
                return _valueField;
            }
            set { _valueField = value; }
        }

        private string _searchField = string.Empty;
        public string SearchField
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_searchField))
                    return "[Id,Text]";
                return _searchField;
            }
            set { _searchField = value; }
        }

        private string _visibleField = string.Empty;
        public string VisibleField
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_visibleField))
                    return TextField;
                return _visibleField;
            }
            set
            {
                _visibleField = value;
            }
        }

        public bool SearchDisabled { get; set; }

        public int MinLengthSearch { get; set; }
        public string Type2 { get; set; }


        public List<SelectOptionItem> OptionItems
        { get; set; }

        public FilterJsonField()
        {
            Ascendants = new List<FilterAscendant>();
            OptionItems = new List<SelectOptionItem>();
        }
    }

    public class FilterJsonRow : List<FilterJsonField>
    {
    }

    public class FilterJson : List<FilterJsonRow>
    {
    }
}
