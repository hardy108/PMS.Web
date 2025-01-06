using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using PMS.EFCore.Helper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{
    public class Bank:EntityFactory<MBANK,MBANK,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Bank(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Bank";
            _authenticationService = authenticationService;
        }

        
        
        public override IEnumerable<MBANK> GetList(GeneralFilter filter)
        {
            var criteria = PredicateBuilder.True<MBANK>();
            
                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);                    

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.BANKID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.BANKNAME.ToLower().Contains(filter.LowerCasedSearchTerm) 
                        );
                }

                
            if (filter.PageSize <= 0)
                return _context.MBANK.Where(criteria);
            return _context.MBANK.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        

        

        

        protected override MBANK BeforeSave(MBANK record, string userName, bool newRecord)
        {
            string errrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(record.BANKID))
                errrorMessage += "Kode tidak boleh kosong.\r\n";
            if (string.IsNullOrWhiteSpace(record.BANKNAME))
                errrorMessage += "Nama tidak boleh kosong.\r\n";

            if (!string.IsNullOrWhiteSpace(errrorMessage))
                throw new Exception(errrorMessage);

            DateTime serverTime = GetServerTime();
            if (newRecord)
            {
                record.CREATEBY = userName;
                record.CREATED = serverTime;
            }
            record.UPDATEBY = userName;
            record.UPDATED = serverTime;
            return record;
        }

      
        
    }
}
