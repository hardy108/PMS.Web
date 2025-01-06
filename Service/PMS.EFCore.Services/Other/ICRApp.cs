using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;

using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using PMS.EFCore.Helper;
using AM.EFCore.Services;
using System.ComponentModel.DataAnnotations;

namespace PMS.EFCore.Services.General
{
    
    public class ICRApp : EntityFactory<MICRAPP,MICRAPP,FilterICRApp, PMSContextBase>
    {
        
        private AuthenticationServiceHO _authenticationService;
        public ICRApp(PMSContextHO context,AuthenticationServiceHO authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "ICR Application";
            _authenticationService = authenticationService;
        }

        public override MICRAPP CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MICRAPP record = base.CopyFromWebFormData(formData, newRecord);            
            return record;
        }

        

        

        protected override MICRAPP BeforeSave(MICRAPP record, string userName, bool newRecord)
        {
            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = currentDate;                
                record.CREATEDBY = userName;
            }
            record.UPDATEDBY = userName;
            record.UPDATED = currentDate;
            return record;
        }

        

    

    

        public override IEnumerable<MICRAPP> GetList(FilterICRApp filter)
        {
            
            var criteria = PredicateBuilder.True<MICRAPP>();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p =>
                        p.APPID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.APPNAME.ToLower().Contains(filter.LowerCasedSearchTerm));

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.APPID.Equals(filter.Id));

                if (filter.Ids.Any())
                    criteria = criteria.And(p => filter.Ids.Contains(p.APPID));
            }

            if (filter.PageSize <= 0)
                return _context.MICRAPP.Where(criteria);
            return _context.MICRAPP.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        


        public override MICRAPP NewRecord(string userName)
        {
            return new MICRAPP
            {
                ACTIVE = true
            };
        }
    }
}
