using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Inventory;
using PMS.EFCore.Services.Location;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;
using Newtonsoft.Json;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Inventory
{
    public class Stock : EntityFactory<TSTOCK,TSTOCK,FilterStock, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Stock(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Stock";
            _authenticationService = authenticationService;
        }

        private string FieldsValidation(TSTOCK stock)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(stock.LOCCODE)) result += "Id harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(stock.MATERIALID)) result += "Nama harus diisi." + Environment.NewLine;
            return result;
        }

        private void Validate(TSTOCK Stock)
        {
            string result = this.FieldsValidation(Stock);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        public decimal[] GetStock( string locationCode, string materialId, DateTime date)
        {
            var result = _context.sp_Stock_Get(locationCode, materialId, date.Date).SingleOrDefault();

            decimal[] data = {
                Convert.ToDecimal(result.STOCK),
                Convert.ToDecimal(result.AMOUNT),
                Convert.ToDecimal(result.PRICE)
            };
            return data;
        }

        public void SetStock1(string locationId, string materialId, int month, int year, bool isIn, decimal quantity, decimal amount, decimal expeditionPrice )
        {
            var direct = "OUT";
            if (isIn) direct = "IN";
            _context.Database.ExecuteSqlCommand($"Exec sp_Stock_SetStock1 {locationId},{materialId},{month},{year},{direct},{quantity},{amount},{expeditionPrice}");                            
        }

        public override IEnumerable<TSTOCK> GetList(FilterStock filter)
        {
            
            var criteria = PredicateBuilder.True<TSTOCK>();
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.LOCCODE))
                    criteria = criteria.And(p => p.LOCCODE.Equals(filter.LOCCODE));

                if (!string.IsNullOrWhiteSpace(filter.MATERIALID))
                    criteria = criteria.And(p => p.MATERIALID.Equals(filter.MATERIALID));

                if (!filter.Date.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.YEAR.Equals(filter.Date.Year));

                var result = _context.TSTOCK.Where(criteria);

                if (filter.PageSize <= 0)
                    return result;
                return result.GetPaged(filter.PageNo, filter.PageSize).Results;

            }
            catch { return new List<TSTOCK>(); }


        }
    }
}
