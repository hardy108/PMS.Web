using System;
using System.Collections;
using System.Collections.Generic;

using System.Text;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class PremiMandor : EntityFactory<TPREMIMANDOR, TPREMIMANDOR, GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public PremiMandor(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PremiMandor";
            _authenticationService = authenticationService;
        }


        protected override TPREMIMANDOR GetSingleFromDB(params  object[] keyValues)
        {
            return _context.TPREMIMANDOR
            .SingleOrDefault(d => d.PAYMENTNO.Equals(keyValues[0]));
        }

        public override IEnumerable<TPREMIMANDOR> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TPREMIMANDOR>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.PAYMENTNO == filter.Id);
            }

            if (filter.PageSize <= 0)
                return _context.TPREMIMANDOR.Where(criteria).ToList();
            return _context.TPREMIMANDOR.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public IEnumerable<TPREMIMANDOR> GetAll(object filterParameter)
        {
            try
            {
                return _context.TPREMIMANDOR.ToList();
            }
            catch { return new List<TPREMIMANDOR>(); }
        }

        public void DeleteByPaymentNo(string no)
        {
            _context.Database.ExecuteSqlCommand($"Exec sp_PremiMandor_DeleteByPaymentNo {no}");
        }
    }

}