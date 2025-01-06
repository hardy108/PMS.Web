using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using PMS.EFCore.Helper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{
    public class PositionDetail:EntityFactory<MPOSITIONDETAIL,MPOSITIONDETAIL, FilterPosition,PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public PositionDetail(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Position Detail";
            _authenticationService = authenticationService;
        }

        protected override MPOSITIONDETAIL GetSingleFromDB(params  object[] keyValues)
        {
            string unitId = (string)keyValues[0], positionId = (string)keyValues[1];
            return _context.MPOSITIONDETAIL.Include(d => d.UNIT).SingleOrDefault(d => d.POSID.Equals(positionId) && d.UNITID.Equals(unitId));

        }
        public override IEnumerable<MPOSITIONDETAIL> GetList(FilterPosition filter)

        {
            
            var criteria = PredicateBuilder.True<MPOSITIONDETAIL>();
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.UNITID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.POSID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        (p.UNIT != null && p.UNIT.NAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.POSID.Equals(filter.Id));
                if (filter.Ids.Any())
                    criteria = criteria.And(p => filter.Ids.Contains(p.POSID));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITID.Equals(filter.UnitID));
                if (filter.UnitIDs.Any())
                    criteria = criteria.And(p => filter.UnitIDs.Contains(p.UNITID));


                if (filter.PageSize<=0)
                    return _context.MPOSITIONDETAIL.Include(d => d.UNIT).Where(criteria);
                return _context.MPOSITIONDETAIL.Include(d => d.UNIT).Where(criteria).GetPaged(filter.PageNo,filter.PageSize).Results;
            }
            catch { return new List<MPOSITIONDETAIL>(); }
        }
    }
}
