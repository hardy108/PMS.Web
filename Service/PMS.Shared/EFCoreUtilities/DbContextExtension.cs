using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;
using PMS.Shared.Utilities;

namespace PMS.Shared.EFCoreUtilities
{
    public static class DbContextExtension
    {
        /// <summary>
        /// Load a stored procedure
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Procedure's name</param>
        /// <returns></returns>
        public static IStoredProcBuilder LoadStoredProc(this DbContext ctx, string name)
        {
            return new StoredProcBuilder(ctx, name);
        }


        /// <summary>
        /// Load a sql Text / Function
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name">Procedure's name</param>
        /// <returns></returns>
        public static IStoredProcBuilder ExecuteSqlText(this DbContext ctx, string sqlText)
        {
            return new SqlTextExecutorBuilder(ctx, sqlText);
        }


        public static IEnumerable<string> FindPrimaryKeyNames<T>(this DbContext ctx, T entity)
        {
            return from p in ctx.FindPrimaryKeyProperties(entity)
                   select p.Name;
        }

        public static IEnumerable<object> FindPrimaryKeyValues<T>(this DbContext ctx, T entity)
        {
            return from p in ctx.FindPrimaryKeyProperties(entity)
                   select entity.GetPropertyValue(p.Name);
        }

        public static IEnumerable<object> FindPrimaryKeyValues<T>(this T entity,IEnumerable<string> primaryKeyNames)
        {

            if (primaryKeyNames == null)
                return null;
            if (!primaryKeyNames.Any())
                return null;
            
            return from p in primaryKeyNames
                   select entity.GetPropertyValue(p);
        }

        public static IReadOnlyList<IProperty> FindPrimaryKeyProperties<T>(this DbContext ctx, T entity)
        {

            return ctx.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties;
        }

        
    }
}
