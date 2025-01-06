using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model
{
    public class sp_Account_CloseYearResult
    {
        public string ID { get; set; }
    }

    public partial class PMSContextBase
    {
        public string sp_Account_CloseYear(string unitCode, int year, string accountCode, DateTime dateTime)
        {
            string rows = string.Empty;
            this.LoadStoredProc("sp_Account_CloseYear")
                .AddParam("UnitCode", unitCode)
                .AddParam("Year", year)
                .AddParam("AccountCode", accountCode)
                .AddParam("Updated", dateTime)
                .ExecScalar(out rows);
            return rows;
        }

    }
}
