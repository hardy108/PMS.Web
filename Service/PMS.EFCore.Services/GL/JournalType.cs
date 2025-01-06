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
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.GL
{
    public class JournalType : EntityFactory<MJOURNALTYPE,MJOURNALTYPE,FilterJournal, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public JournalType(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "JournalType";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<MJOURNALTYPE> GetList(FilterJournal filter)
        {
            
            var criteria = PredicateBuilder.True<MJOURNALTYPE>();

            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.CODE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.Modul))
                criteria = criteria.And(p => p.MODUL.Contains(filter.Modul));

            if (!string.IsNullOrWhiteSpace(filter.JournalType))
                criteria = criteria.And(p => p.CODE.Equals(filter.JournalType));

            if (filter.PageSize<=0)
                return _context.MJOURNALTYPE.Where(criteria);

            return _context.MJOURNALTYPE.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }


        protected override MJOURNALTYPE GetSingleFromDB(params  object[] keyValues)
        {
            return _context.MJOURNALTYPE.Find(keyValues);
        }

        public List<MJOURNALTYPE> GetByModul(string modul)
        {
            modul = "," + modul + ",";
            return _context.MJOURNALTYPE.Where(d => d.MODUL.Contains(modul)).ToList();
        }
                     
    }
}
