using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services.Payroll;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Organization;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class PaymentTax : EntityFactory<TPAYMENTTAX, TPAYMENTTAX, FilterSalary, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;
        private Employee _serviceEmp;
        private AuthenticationServiceBase _authenticationService;

        public PaymentTax(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PaymentTax";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context,_authenticationService,auditContext);
            _serviceDivisiName = new Divisi(context,_authenticationService,auditContext);
            _serviceEmp = new Employee(context,_authenticationService,auditContext);
        }

       

        protected override TPAYMENTTAX GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            string empid = keyValues[1].ToString();
            return _context.TPAYMENTTAX.SingleOrDefault(d => d.DOCNO.Equals(id) && d.EMPID.Equals(empid));
        }

        public override IEnumerable<TPAYMENTTAX> GetList(FilterSalary filter)
        {
            var criteria = PredicateBuilder.True<TPAYMENTTAX>();
            try
            {
                //if (filter.AUTO.HasValue)
                //    criteria = criteria.And(d => d.AUTO == filter.AUTO.Value);

                if (!string.IsNullOrEmpty(filter.Id))
                    criteria = criteria.And(d => d.DOCNO == filter.Id);

                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.DOCNO));

                if (!string.IsNullOrEmpty(filter.DOCNO))
                    criteria = criteria.And(d => d.DOCNO == filter.DOCNO);

                if (!string.IsNullOrEmpty(filter.EMPID))
                    criteria = criteria.And(d => d.EMPID == filter.EMPID);

                if (filter.PageSize <= 0)
                    return _context.TPAYMENTTAX.Where(criteria).ToList();
                return _context.TPAYMENTTAX.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }
    }
}
