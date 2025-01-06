using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;

namespace PMS.EFCore.Model
{
    public class sp_Helper_GetDateTime_Result
    {
        public DateTime? ServerDate { get; set; }
    }

    public partial class PMSContextBase
    {
        public DbQuery<sp_Helper_GetDateTime_Result> sp_Helper_GetDateTime_Result { get; set; }
        public IQueryable<sp_Helper_GetDateTime_Result> sp_Helper_GetDateTime(long version)
        {
            return sp_Helper_GetDateTime_Result.FromSql($"execute sp_Helper_GetDateTime {version}");
        }
    }
}
