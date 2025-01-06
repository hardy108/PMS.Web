using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMS.EFCore.Helper
{
    public abstract class PagedResultBase
    {
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }

        public int FirstRowOnPage
        {

            get { return (CurrentPage - 1) * PageSize + 1; }
        }

        public int LastRowOnPage
        {
            get { return Math.Min(CurrentPage * PageSize, RowCount); }
        }
    }

    public class PagedResult<T> : PagedResultBase where T : class
    {
        public IList<T> Results { get; set; }

        public PagedResult()
        {
            Results = new List<T>();
        }
    }

    public static class PagingExtention
    {
        public static PagedResult<T> GetPaged<T>(this IQueryable<T> query,
                                         int page, int pageSize) where T : class
        {
            var result = new PagedResult<T>();
            result.CurrentPage = page;
            result.PageSize = pageSize;
            try
            {
                result.RowCount = query.Count();
            }
            catch { result.RowCount = 0; }

            if (pageSize > 0)
            {
                if (result.RowCount > 0)
                {
                    if (page < 1)
                        page = 1;
                    var pageCount = (double)result.RowCount / pageSize;
                    result.PageCount = (int)Math.Ceiling(pageCount);

                    var skip = (page - 1) * pageSize;
                    result.Results = query.Skip(skip).Take(pageSize).ToList();
                }
                else
                    result.Results = null;
            }
            else
                result.Results = query.ToList();
            

            return result;
        }
    }
}
