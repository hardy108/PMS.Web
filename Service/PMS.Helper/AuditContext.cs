using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Helper
{
    public class AuditContext:DbContext
    {   
        public AuditContext(DbContextOptions<AuditContext> options) : base(options)
        {
            
        }

        public void SaveAuditTrail(string by, string to, string note, DateTime date)
        {
            try
            {
                Database.ExecuteSqlCommand($"Insert Into TAUDIT([BY], [TO], [DATETIME], NOTE) Values ({by}, {to}, {date}, {note})");
#if DEBUG
                Console.WriteLine($"[{date.ToString("yyyy-MM-dd HH:mm:ss")}]{note}");
#endif
            }
            catch (Exception ex)
            { }
        }

        public void SaveAuditTrail(string by, string to, string note)
        {
            try
            {
                DateTime date = GetServerTime();
                Database.ExecuteSqlCommand($"Insert Into TAUDIT([BY], [TO], [DATETIME], NOTE) Values ({by}, {to}, {date}, {note})");
#if DEBUG
                Console.WriteLine($"[{date.ToString("yyyy-MM-dd HH:mm:ss")}][{by}][{to}]{note}");
#endif
            }
            catch (Exception ex)
            { }
        }


        public DateTime GetServerTime()
        {
            DateTime serverTime;
            this.ExecuteSqlText("Select GETDATE()")                
                .ExecScalar<DateTime>(out serverTime);
            return serverTime;
        }
    }
}
