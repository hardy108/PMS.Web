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
    public class PenaltyType:EntityFactory<MPENALTYTYPE,MPENALTYTYPE,GeneralFilter, PMSContextBase>
    {

        private AuthenticationServiceBase _authenticationService;
        public PenaltyType(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PenaltyType";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<MPENALTYTYPE> GetList(GeneralFilter filter)
        {
            var criteria = PredicateBuilder.True<MPENALTYTYPE>();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p =>
                        p.DESCRIPTION.ContainsIgnoreCase(filter.LowerCasedSearchTerm) ||
                        p.PENALTYCODE.ContainsIgnoreCase(filter.LowerCasedSearchTerm));

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.PENALTYCODE.Equals(filter.Id));

                //Added By Junaidi 2020-03-29 - Start
                if (filter.Ids.Any())
                    criteria = criteria.And(p => filter.Ids.Contains(p.PENALTYCODE));
                //Added By Junaidi 2020-03-29 - End

                

            

            if (filter.PageSize <= 0)
                return _context.MPENALTYTYPE.Where(criteria);
            return _context.MPENALTYTYPE.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;


            
        }

    }
}
