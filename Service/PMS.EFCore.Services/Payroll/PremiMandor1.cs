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
    public class PremiMandor1 : EntityFactory<TPREMIMANDOR1, TPREMIMANDOR1, GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public PremiMandor1(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PremiMandor1";
            _authenticationService = authenticationService;
        }


        protected override TPREMIMANDOR1 GetSingleFromDB(params  object[] keyValues)
        {
            return _context.TPREMIMANDOR1
            .SingleOrDefault(d => d.PAYMENTNO.Equals(keyValues[0]));
        }

        public override IEnumerable<TPREMIMANDOR1> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TPREMIMANDOR1>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.PAYMENTNO == filter.Id);
            }

            if (filter.PageSize <= 0)
                return _context.TPREMIMANDOR1.Where(criteria).ToList();
            return _context.TPREMIMANDOR1.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public IEnumerable<TPREMIMANDOR1> GetAll(object filterParameter)
        {
            try
            {
                return _context.TPREMIMANDOR1.ToList();
            }
            catch { return new List<TPREMIMANDOR1>(); }
        }

        public void DeleteByPaymentNo(string no)
        {
            _context.Database.ExecuteSqlCommand($"Exec sp_PremiMandor_DeleteByPaymentNo {no}");
        }
    }

}