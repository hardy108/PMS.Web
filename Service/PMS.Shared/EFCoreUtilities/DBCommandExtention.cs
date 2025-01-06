using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Text;

namespace PMS.Shared.EFCoreUtilities
{
    public static class DBCommandExtention
    {
        public static IEnumerable<dynamic> CollectionFromSql(this DbContext sourceContext,
                                                            string sql,
                                                            Dictionary<string, object> Parameters)
        {
            using (var cmd = sourceContext.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = sql;
                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                    cmd.Connection.Open();
                if (Parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in Parameters)
                    {

                        DbParameter dbParameter = cmd.CreateParameter();
                        dbParameter.ParameterName = param.Key;
                        dbParameter.Value = param.Value;
                        cmd.Parameters.Add(dbParameter);
                    }
                }

                var retObject = new List<dynamic>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var dataRow = new ExpandoObject() as IDictionary<string, object>;
                        for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                            dataRow.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);

                        retObject.Add((ExpandoObject)dataRow);
                    }
                }

                return retObject;
            }
        }
    }
}
