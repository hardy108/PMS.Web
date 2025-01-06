using System;
using System.Collections;
using System.Collections.Generic;

using System.Text;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class PaymentAttribute : EntityFactory<TPAYMENTATTR, TPAYMENTATTR, GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public PaymentAttribute(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PaymentAttribute";
            _authenticationService = authenticationService;
        }


        protected override TPAYMENTATTR GetSingleFromDB(params  object[] keyValues)
        {
            return _context.TPAYMENTATTR
            .SingleOrDefault(d => d.DOCNO.Equals(keyValues[0]));
        }

        public override IEnumerable<TPAYMENTATTR> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TPAYMENTATTR>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.DOCNO == filter.Id);
            }

            if (filter.PageSize <= 0)
                return _context.TPAYMENTATTR.Where(criteria).ToList();
            return _context.TPAYMENTATTR.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public IEnumerable<TPAYMENTATTR> GetAll(object filterParameter)
        {
            try
            {
                return _context.TPAYMENTATTR.ToList();
            }
            catch { return new List<TPAYMENTATTR>(); }
        }

        public void sp_PaymentAttribute_DeleteByNo(string no)
        {
            _context.Database.ExecuteSqlCommand($"Exec sp_PaymentAttribute_DeleteByNo {no}");
        }
    }

}