using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;

namespace PMS.EFCore.Helper
{
    public static class DbTransactionExt
    {
        public static void RegisterDbContext(this IDbContextTransaction transaction, DbContext context)
        {
            if (transaction == null)
                transaction = context.Database.BeginTransaction();
            else
                context.Database.UseTransaction(transaction.GetDbTransaction());
        }
    }
}
