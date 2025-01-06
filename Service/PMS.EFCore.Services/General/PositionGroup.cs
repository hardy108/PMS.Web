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
    public class PositionGroup:EntityFactory<MPOSITIONGROUP,MPOSITIONGROUP,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public PositionGroup(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Position Group";
            _authenticationService = authenticationService;
        }
        
        public override IEnumerable<MPOSITIONGROUP> GetList(GeneralFilter filter)
        {            
            var criteria = PredicateBuilder.True<MPOSITIONGROUP>();
            try
            {                
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p => p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm));
                }
                
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.ID.ToString() == filter.Id);

                if (filter.PageSize <= 0)
                    return _context.MPOSITIONGROUP.Where(criteria);
                return _context.MPOSITIONGROUP.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return new List<MPOSITIONGROUP>(); }
        }

        protected override MPOSITIONGROUP BeforeSave(MPOSITIONGROUP record, string userName, bool newRecord)
        {
            if (string.IsNullOrWhiteSpace(record.NAME))
                throw new Exception("Nama tidak boleh kosong");
            if (newRecord)
            {
                if (_context.MPOSITIONGROUP.SingleOrDefault(d => d.NAME.Equals(record.NAME)) != null)
                    throw new Exception("Nama kelompok sudah ada");
            }
            return record;
        }

        
    }
}
