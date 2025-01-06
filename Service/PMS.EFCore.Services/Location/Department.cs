using System;
using System.Collections.Generic;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Location
{
    public class Department : EntityFactory<MDEPARTMENT,MDEPARTMENT,FilterDepartment, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Department(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Department";
            _authenticationService = authenticationService;
        }
        

        
        public override MDEPARTMENT CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MDEPARTMENT record = base.CopyFromWebFormData(formData, newRecord);//If Required Replace with your custom code
            return record;
        }

        protected override MDEPARTMENT BeforeSave(MDEPARTMENT record, string userName, bool newRecord)
        {
            if (string.IsNullOrWhiteSpace(record.DEPTID))
                throw new Exception("Kode departmen tidak boleh kosong");
            if (string.IsNullOrWhiteSpace(record.DEPTNAME))
                throw new Exception("Nama departmen tidak boleh kosong");


            DateTime currentDate = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.ACTIVE = true;
                record.CREATED = currentDate;
                record.CREATEDBY = userName;
            }
            record.UPDATED = currentDate;
            record.UPDATEBY = userName;
            return record;
        }

        

        

       

        

        

        public override IEnumerable<MDEPARTMENT> GetList(FilterDepartment filter)
        {            
            var criteria = PredicateBuilder.True<MDEPARTMENT>();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.DEPTNAME.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.DEPTID.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }

            if (filter.IsActive.HasValue)
                criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.DEPTID.Equals(filter.Id));

            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains( p.DEPTID));

            if (filter.PageSize <= 0)
                return _context.MDEPARTMENT.Where(criteria);
            return _context.MDEPARTMENT.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }
        
    }
}