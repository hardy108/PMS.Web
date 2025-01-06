using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using PMS.EFCore.Services.General;
using AM.EFCore.Services;
using PMS.Shared.Models;

namespace PMS.EFCore.Services.Logistic
{
    public class EMInspection : EntityFactory<EM_TINSPECTION, EM_TINSPECTION, GeneralFilter, PMSContextBase>
    {

        
        
        private AuthenticationServiceBase _authenticationService;
        public EMInspection(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "EM-Inspection";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<EM_TINSPECTION> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<EM_TINSPECTION>();
            
            if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => !d.status.Equals(PMSConstants.TransactionStatusDeleted));
            else
                criteria = criteria.And(d => d.status.Equals(filter.RecordStatus));


            criteria = criteria.And(d => d.date >= filter.StartDate.Date && d.date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.title.ToLower().Contains(filter.LowerCasedSearchTerm));

            if (filter.PageSize <= 0)
                return _context.EM_TINSPECTION.Where(criteria);
            return _context.EM_TINSPECTION.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }



       
       
    }
}
