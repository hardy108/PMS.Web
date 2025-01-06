using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace PMS.Shared.EFCoreUtilities
{
    public class StoredProcMultiResults
    {
        //  private DbCommand _command;
        private DbDataReader _reader;

        public StoredProcMultiResults(DbDataReader reader)
        {
            // _command = command;
            _reader = reader;
        }

        public IList<T> ReadToList<T>()
        {
            return MapToList<T>(_reader);
        }

        public List<object> ReadToValues<T>()
        {
            return MapToValues(_reader,0);
        }
        public List<object> ReadToValues(int columnIndex)
        {
            return MapToValues(_reader, columnIndex);
        }

        public List<object> ReadToValues(string columnName)
        {
            return MapToValues(_reader, columnName);
        }


        public T? ReadToValue<T>() where T : struct
        {
            return MapToValue<T>(_reader);
        }

        public Task<bool> NextResultAsync()
        {
            return _reader.NextResultAsync();
        }

        public Task<bool> NextResultAsync(CancellationToken ct)
        {
            return _reader.NextResultAsync(ct);
        }

        public bool NextResult()
        {
            return _reader.NextResult();
        }

        /// <summary>
        /// Retrieves the column values from the stored procedure and maps them to <typeparamref name="T"/>'s properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns>IList<<typeparamref name="T"/>></returns>
        private IList<T> MapToList<T>(DbDataReader dr)
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties().ToList();



            var colMapping = dr.GetColumnSchema()
                .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
                .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    T obj = Activator.CreateInstance<T>();
                    foreach (var prop in props)
                    {
                        if (colMapping.ContainsKey(prop.Name.ToLower()))
                        {
                            var column = colMapping[prop.Name.ToLower()];

                            if (column?.ColumnOrdinal != null)
                            {
                                var val = dr.GetValue(column.ColumnOrdinal.Value);
                                prop.SetValue(obj, val == DBNull.Value ? null : val);
                            }

                        }
                    }
                    objList.Add(obj);
                }
            }
            return objList;
        }


        /// <summary>
        ///Attempts to read the first value of the first row of the resultset at first column.
        /// </summary>
        private T? MapToValue<T>(DbDataReader dr) where T : struct
        {
            return MapToValue<T>(dr, 0);
        }

        /// <summary>
        ///Attempts to read the first value of the first row of the resultset at specified column index.
        /// </summary>
        private T? MapToValue<T>(DbDataReader dr, int colIndex) where T : struct
        {
            if (dr.HasRows)
            {
                if (dr.Read())
                {
                    return dr.IsDBNull(colIndex) ? new T?() : new T?(dr.GetFieldValue<T>(colIndex));
                }
            }
            return new T?();
        }

        /// <summary>
        ///Attempts to read the first value of the first row of the resultset at specified column name.
        /// </summary>
        private T? MapToValue<T>(DbDataReader dr, string colName) where T : struct
        {
            var colMapping = dr.GetColumnSchema()
                    .SingleOrDefault(x => x.ColumnName.Equals(colName));

            if (colMapping.ColumnOrdinal.HasValue)
                return MapToValue<T>(dr, colMapping.ColumnOrdinal.Value);
            return new T?();
        }



        
        /// <summary>
        ///Attempts to read all rows of the resultset at specified column index.
        /// </summary>
        private List<object> MapToValues(DbDataReader dr, int colIndex)
        {
            var result = new List<object>();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    
                    object value = dr.IsDBNull(colIndex) ? null : dr[colIndex];
                    result.Add(value);
                }
            }
            return result;
        }

        /// <summary>
        ///Attempts to read all rows of the resultset at specified column name.
        /// </summary>
        private List<object> MapToValues(DbDataReader dr, string colName)
        {
            var colMapping = dr.GetColumnSchema()
                    .SingleOrDefault(x => x.ColumnName.Equals(colName));

            if (colMapping.ColumnOrdinal.HasValue)
                return MapToValues(dr, colMapping.ColumnOrdinal.Value);
            return new List<object>();
        }
    }
}
