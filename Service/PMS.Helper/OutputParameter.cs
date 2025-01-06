﻿using System;
using System.Data.Common;

namespace PMS.EFCore.Helper
{
    internal class OutputParam<T> : IOutParam<T>
    {
        public OutputParam(DbParameter param)
        {
            _dbParam = param;
        }

        public T Value
        {
            get
            {
                if (_dbParam.Value is DBNull)
                {
                    if (default(T) == null)
                    {
                        return default;
                    }
                    else
                    {
                        throw new InvalidOperationException($"{_dbParam.ParameterName} is null and can't be assigned to a non-nullable type");
                    }
                }

                return (T) Convert.ChangeType(_dbParam.Value, typeof(T));
            }
        }

        public override string ToString() => _dbParam.Value.ToString();

        private readonly DbParameter _dbParam;
    }
}
