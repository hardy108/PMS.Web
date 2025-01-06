using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.EFCoreUtilities
{
    public class GeneralPagingFilter
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int TopCount { get; set; }

        public int RowStart
        {
            get
            {
                if (PageSize <= 0)
                    return 0;
                if (PageNo <= 0)
                    PageNo = 1;
                return (PageNo - 1) * PageSize + 1;
            }
        }

        public int RowEnd
        {
            get
            {
                if (PageSize <= 0)
                    return 0;
                if (PageNo <= 0)
                    PageNo = 1;
                return RowStart + PageSize - 1;
            }
        }
        private int _maxRows = 0;
        public int MaxRows
        {
            get { return _maxRows; }
            set
            {
                _maxRows = value;
                if (_maxRows > 0)
                {
                    if (PageSize <= 0)
                    {
                        PageSize = _maxRows;
                        PageNo = 1;
                    }
                }


            }
        }

        private string searchTerm = string.Empty;
        private string lowerCasedSearchTerm = string.Empty;
        private string upperCasedSearchTerm = string.Empty;
        public string SearchTerm
        {
            get
            {
                return searchTerm;
            }
            set
            {
                searchTerm = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                upperCasedSearchTerm = searchTerm.ToUpper();
                lowerCasedSearchTerm = searchTerm.ToLower();
            }
        }

        public string Keyword
        {
            get { return SearchTerm; }
            set { SearchTerm = value; }
        }

        public string LowerCasedSearchTerm
        {
            get
            {
                return lowerCasedSearchTerm;
            }
        }

        public string UpperCasedSearchTerm
        {
            get
            {
                return upperCasedSearchTerm;
            }
        }
    }
}
