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
    public class PaymentAttributeEmployee : EntityFactory<TPAYMENTATTREMP, TPAYMENTATTREMP, GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public PaymentAttributeEmployee(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PaymentAttributeEmployee";
            _authenticationService = authenticationService;
        }


        protected override TPAYMENTATTREMP GetSingleFromDB(params  object[] keyValues)
        {
            return _context.TPAYMENTATTREMP
            .SingleOrDefault(d => d.DOCNO.Equals(keyValues[0]));
        }

        public override IEnumerable<TPAYMENTATTREMP> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TPAYMENTATTREMP>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.DOCNO == filter.Id);
            }

            if (filter.PageSize <= 0)
                return _context.TPAYMENTATTREMP.Where(criteria).ToList();
            return _context.TPAYMENTATTREMP.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public IEnumerable<TPAYMENTATTREMP> GetAll(object filterParameter)
        {
            try
            {
                return _context.TPAYMENTATTREMP.ToList();
            }
            catch { return new List<TPAYMENTATTREMP>(); }
        }

        public void sp_PaymentAttributeEmployee_DeleteByNo(string no)
        {
            _context.Database.ExecuteSqlCommand($"Exec sp_PaymentAttributeEmployee_DeleteByNo {no}");
        }
    }

}