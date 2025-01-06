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
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class SalaryType : EntityFactory<MSALARYTYPE,MSALARYTYPE,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public SalaryType(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "SalaryType";
            _authenticationService = authenticationService;
        }


        protected override MSALARYTYPE GetSingleFromDB(params  object[] keyValues)
        {
            return _context.MSALARYTYPE
            .SingleOrDefault(d => d.ID.Equals(keyValues[0]));
        }

        protected override MSALARYTYPE BeforeSave(MSALARYTYPE record, string userName, bool newRecord)
        {
            DateTime now = GetServerTime();
            if (newRecord)
            {
                if (_context.MSALARYTYPE.SingleOrDefault(d => d.ID.Equals(record.ID)) != null)
                    throw new Exception("ID sudah ada.");
            }

            record.UPDATED = now;

            return record;
        }

        public override IEnumerable<MSALARYTYPE> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<MSALARYTYPE>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.ID == filter.Id);

                if (!string.IsNullOrWhiteSpace(filter.UserName))
                    criteria = criteria.And(p => p.NAME == filter.UserName);

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(d => d.AUTO == filter.IsActive.Value);

            }

            if (filter.PageSize <= 0)
                return _context.MSALARYTYPE.Where(criteria).ToList();
            return _context.MSALARYTYPE.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public  IEnumerable<MSALARYTYPE> GetAll(object filterParameter)
        {
            try
            {
                return _context.MSALARYTYPE.ToList();
            }
            catch { return new List<MSALARYTYPE>(); }
        }
    }

}