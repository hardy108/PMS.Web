using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace PMS.Shared.Models
{
    public class DataList
    {
        public string ListID { get; set; }
        public List<ListField> ListFields
        {
            get { return _allFields; }
            set
            {
                _allFields = new List<ListField>();
                if (value != null)
                    _allFields.AddRange(value);
                if (_allFields.Any())
                {
                    _keyFields = _allFields.Where(d => d.IsKey == true).ToList();
                    _sortFields = _allFields.Where(d => d.Sort != 0).ToList();
                    if (_sortFields == null || !_sortFields.Any())
                        _sortFields = _keyFields;                        
                    _searchFields = _allFields.Where(d => d.ForSearch == true).ToList();
                }

            }
        }
        private List<ListField> _allFields = new List<ListField>();
        private List<ListField> _keyFields = new List<ListField>();
        private List<ListField> _sortFields = new List<ListField>();
        private List<ListField> _searchFields = new List<ListField>();


        public string GetSQLQueryMain(string userName, string srcTable, string filter, string keyword)
        {


            string SQLMain = string.Empty;

            SQLMain = "Select " + GetKey();

            string sqlSort = BuildSort();

            foreach (ListField field in _allFields)
            {
                SQLMain += ",[" + field.FieldName + "] ";
            }


            if (sqlSort == string.Empty)
                SQLMain += ",ROW_NUMBER() AS ROWNUM ";
            else
                SQLMain += ",ROW_NUMBER() OVER( ORDER BY " + sqlSort + ") AS ROWNUM ";


            string sourceTable = string.Format(srcTable, "'" + userName + "'");
            string strSearch = BuildSearchFilter(keyword);

            SQLMain += " From " + sourceTable + " Where " + strSearch;


            if (!string.IsNullOrWhiteSpace(filter))
                SQLMain += " And " + filter;



            return SQLMain;
        }
        public string GetSQLQueryRecordCount(string userName, string srcTable, string filter, string keyword)
        {
            string SQLRecordCount = string.Empty;


            SQLRecordCount = "Select Count(*) Result ";
            string sqlSort = BuildSort();




            string sourceTable = string.Format(srcTable, "'" + userName + "'");
            string strSearch = BuildSearchFilter(keyword);


            SQLRecordCount += " From " + sourceTable + " Where " + strSearch;

            if (!string.IsNullOrWhiteSpace(filter))
            {

                SQLRecordCount += " And " + filter;
            }


            return SQLRecordCount;
        }
        public string GetKey()
        {
            string strKeyFields = string.Empty;
            if (_keyFields.Any())
            {
                foreach (ListField field in _keyFields)
                {
                    switch ((XFieldType)field.FieldType)
                    {
                        case XFieldType.FInteger:
                        case XFieldType.FFloat:
                        case XFieldType.FBool:
                            strKeyFields += " + Cast([" + field.FieldName + "] as varchar(50))";
                            break;
                        case XFieldType.FDate:
                            strKeyFields += " + CONVERT(Varchar(23),[" + field.FieldName + "],121)";
                            break;
                        default:
                            strKeyFields += " + [" + field.FieldName + "]";
                            break;

                    }
                }
                return strKeyFields.Substring(3) + " As [Key]";
            }
            else
                return "1 as [Key]";
        }
        private string BuildSearchFilter(string searchKeyword)
        {
            if (string.IsNullOrWhiteSpace(searchKeyword))
            {
                return "(1 = 1)";
            }
            string strSearchFilter = string.Empty;
            try
            {

                foreach (ListField field in _searchFields)
                {
                    if ((XFieldType)field.FieldType == XFieldType.FText)
                        strSearchFilter += " Or [" + field.FieldName + "] Like '%" + searchKeyword + "%'";
                    else if ((XFieldType)field.FieldType == XFieldType.FFloat || (XFieldType)field.FieldType == XFieldType.FInteger)
                    {
                        decimal x = 0;
                        if (decimal.TryParse(searchKeyword, out x))
                        {
                            strSearchFilter += " Or [" + field.FieldName + "] = " + searchKeyword + " ";
                        }
                    }
                    else if ((XFieldType)field.FieldType == XFieldType.FBool)
                    {
                        if (string.Compare(searchKeyword, "true", true) == 0 || string.Compare(searchKeyword, "yes", true) == 0)
                            strSearchFilter += " Or [" + field.FieldName + "] = 1 ";
                        else if (string.Compare(searchKeyword, "false", true) == 0 || string.Compare(searchKeyword, "no", true) == 0)
                            strSearchFilter += " Or [" + field.FieldName + "] = 0 ";
                    }

                }
            }
            catch { }
            if (!string.IsNullOrWhiteSpace(strSearchFilter))
                strSearchFilter = "(" + strSearchFilter.Substring(4) + ")";
            else
                strSearchFilter = "(1 = 1)";
            return strSearchFilter;
        }

        private string BuildSort()
        {
            string sort = string.Empty;

            try
            {

                foreach (ListField field in _sortFields)
                {
                    if ((XFieldSort)field.Sort == XFieldSort.FSorcDesc)
                        sort += "[" + field.FieldName + "] DESC,";
                    else
                        sort += "[" + field.FieldName + "] ASC,";
                }
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(sort))
                sort = sort.Substring(0, sort.Length - 1);

            return sort;
        }

        

    }
}
