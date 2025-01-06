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
    public class PremiChecker : EntityFactory<TPREMICHECKER, TPREMICHECKER, FilterSalary, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;
        private Employee _serviceEmp;
        private AuthenticationServiceBase _authenticationService;
        public PremiChecker(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PremiChecker";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context,_authenticationService,auditContext);
            _serviceDivisiName = new Divisi(context,_authenticationService,auditContext);
            _serviceEmp = new Employee(context,_authenticationService,auditContext);
        }

      
        protected override TPREMICHECKER GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.TPREMICHECKER.Include(a => a.PAYMENTNONavigation).Include(b => b.CHECKER).SingleOrDefault(d => d.PAYMENTNO.Equals(id));
        }

        public override IEnumerable<TPREMICHECKER> GetList(FilterSalary filter)
        {
            var criteria = PredicateBuilder.True<TPREMICHECKER>();
            try
            {
                //if (filter.AUTO.HasValue)
                //    criteria = criteria.And(d => d.AUTO == filter.AUTO.Value);

                if (!string.IsNullOrEmpty(filter.Id))
                    criteria = criteria.And(d => d.PAYMENTNO == filter.Id);

                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.PAYMENTNO));

                if (!string.IsNullOrEmpty(filter.DOCNO))
                    criteria = criteria.And(d => d.PAYMENTNO == filter.DOCNO);

                if (!string.IsNullOrEmpty(filter.EMPID))
                    criteria = criteria.And(d => d.CHECKERID == filter.EMPID);

                if (filter.PageSize <= 0)
                    return _context.TPREMICHECKER.Where(criteria).ToList();
                return _context.TPREMICHECKER.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }
    }
}
