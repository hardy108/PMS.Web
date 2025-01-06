using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services.Entities;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PMS.EFCore.Services.Security
{
    public class Audit
    {
        public static void Insert(string by,string to,DateTime dateTime,string note,PMSContextBase context)
        {
            context.Database.ExecuteSqlCommand($"Insert	Into TAUDIT([BY], [TO], [DATETIME], NOTE) Values ({by}, {to}, {dateTime}, {note})");
        }
    }
}
