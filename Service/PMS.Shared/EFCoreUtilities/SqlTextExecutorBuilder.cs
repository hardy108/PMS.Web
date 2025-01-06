﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace PMS.Shared.EFCoreUtilities
{
    internal class SqlTextExecutorBuilder : IStoredProcBuilder
    {
        private const string _retParamName = "_retParam";
        private DbCommand _cmd;

        public SqlTextExecutorBuilder(DbContext ctx, string sqlText)
        {
            if (sqlText is null)
                throw new ArgumentNullException(nameof(sqlText));

            DbCommand cmd = ctx.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sqlText;
            cmd.Transaction = ctx.Database.CurrentTransaction?.GetDbTransaction();
            cmd.CommandTimeout = ctx.Database.GetCommandTimeout().GetValueOrDefault(cmd.CommandTimeout);

            _cmd = cmd;
        }



        public IStoredProcBuilder AddParam<T>(string name, T val)
        {
            AddParamInner(name, val, ParameterDirection.Input);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam)
        {
            outParam = AddOutputParamInner(name, default(T), ParameterDirection.Output);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam, ParamExtra extra)
        {
            outParam = AddOutputParamInner(name, default(T), ParameterDirection.Output, extra.Size, extra.Precision, extra.Scale);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam, int size = 0, byte precision = 0, byte scale = 0)
        {
            outParam = AddOutputParamInner(name, val, ParameterDirection.Output, size, precision, scale);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, out IOutParam<T> outParam, int size = 0, byte precision = 0, byte scale = 0)
        {
            outParam = AddOutputParamInner(name, default(T), ParameterDirection.Output, size, precision, scale);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam)
        {
            outParam = AddOutputParamInner(name, val, ParameterDirection.InputOutput);
            return this;
        }

        public IStoredProcBuilder AddParam<T>(string name, T val, out IOutParam<T> outParam, ParamExtra extra)
        {
            outParam = AddOutputParamInner(name, val, ParameterDirection.InputOutput, extra.Size, extra.Precision, extra.Scale);
            return this;
        }

        public IStoredProcBuilder AddParam(DbParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            _cmd.Parameters.Add(parameter);
            return this;
        }

        public IStoredProcBuilder ReturnValue<T>(out IOutParam<T> retParam)
        {
            retParam = AddOutputParamInner(_retParamName, default(T), ParameterDirection.ReturnValue);
            return this;
        }

        public IStoredProcBuilder ReturnValue<T>(out IOutParam<T> retParam, ParamExtra extra)
        {
            retParam = AddOutputParamInner(_retParamName, default(T), ParameterDirection.ReturnValue, extra.Size, extra.Precision, extra.Scale);
            return this;
        }

        public IStoredProcBuilder SetTimeout(int timeout)
        {
            _cmd.CommandTimeout = timeout;
            return this;
        }

        public void Exec(Action<DbDataReader> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                OpenConnection();
                using (DbDataReader r = _cmd.ExecuteReader())
                {
                    action(r);
                }
            }
            finally
            {
                Dispose();
            }
        }

        public void ExecMultiResults(Action<StoredProcMultiResults> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                OpenConnection();
                using (DbDataReader r = _cmd.ExecuteReader())
                {
                    var multiResults = new StoredProcMultiResults(r);
                    action(multiResults);
                }
            }
            finally
            {
                Dispose();
            }
        }

        public Task ExecAsync(Func<DbDataReader, Task> action)
        {
            return ExecAsync(action, CancellationToken.None);
        }

        public async Task ExecAsync(Func<DbDataReader, Task> action, CancellationToken cancellationToken)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                await OpenConnectionAsync();
                using (DbDataReader r = await _cmd.ExecuteReaderAsync(cancellationToken))
                {
                    await action(r);
                }
            }
            finally
            {
                Dispose();
            }
        }

        public void ExecNonQuery()
        {
            try
            {
                OpenConnection();
                _cmd.ExecuteNonQuery();
            }
            finally
            {
                Dispose();
            }
        }

        public Task ExecNonQueryAsync()
        {
            return ExecNonQueryAsync(CancellationToken.None);
        }


        public async Task ExecNonQueryAsync(CancellationToken cancellationToken)
        {
            try
            {
                await OpenConnectionAsync();
                await _cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                Dispose();
            }
        }

        public void ExecScalar<T>(out T val)
        {
            try
            {
                OpenConnection();
                object scalar = _cmd.ExecuteScalar();
                val = DefaultIfDBNull<T>(scalar);
            }
            finally
            {
                Dispose();
            }
        }

        public Task ExecScalarAsync<T>(Action<T> action)
        {
            return ExecScalarAsync(action, CancellationToken.None);
        }

        public async Task ExecScalarAsync<T>(Action<T> action, CancellationToken cancellationToken)
        {
            try
            {
                await OpenConnectionAsync();
                object scalar = await _cmd.ExecuteScalarAsync(cancellationToken);
                T val = DefaultIfDBNull<T>(scalar);
                action(val);
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            _cmd.Connection.Close();
            _cmd.Dispose();
        }

        private OutputParam<T> AddOutputParamInner<T>(string name, T val, ParameterDirection direction, int size = 0, byte precision = 0, byte scale = 0)
        {
            DbParameter param = AddParamInner(name, val, direction, size, precision, scale);
            return new OutputParam<T>(param);
        }

        private DbParameter AddParamInner<T>(string name, T val, ParameterDirection direction, int size = 0, byte precision = 0, byte scale = 0)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            DbParameter param = _cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = (object)val ?? DBNull.Value;
            param.Direction = direction;
            param.DbType = DbTypeConverter.ConvertToDbType<T>();
            param.Size = size;
            param.Precision = precision;
            param.Scale = scale;

            _cmd.Parameters.Add(param);
            return param;
        }

        private void OpenConnection()
        {
            if (_cmd.Connection.State == ConnectionState.Closed)
            {
                _cmd.Connection.Open();
            }
        }

        private Task OpenConnectionAsync()
        {
            if (_cmd.Connection.State == ConnectionState.Closed)
            {
                return _cmd.Connection.OpenAsync();
            }
            return Task.CompletedTask;
        }

        private T DefaultIfDBNull<T>(object o)
        {

            return (o == null || o == DBNull.Value) ? default(T) : (T)o;
        }
    }
}
