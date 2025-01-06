using System;
using System.Collections.Generic;
using AM.EFCore.Services;
using Microsoft.EntityFrameworkCore;
using PMS.Shared.Utilities;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services;
using System.Linq;

namespace PMS.EFCore.Services.General
{
    public class EmployeeStatus: EntityFactory<MSTATUS,MSTATUS,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public EmployeeStatus(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "EmployeeStatus";
            _authenticationService = authenticationService;
        }

        protected override MSTATUS GetSingleFromDB(params object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.MSTATUS.SingleOrDefault(d => d.STATUSID.Equals(id));
        }

        public override IEnumerable<MSTATUS> GetList(GeneralFilter filter)
        {            
            var criteria = PredicateBuilder.True<MSTATUS>();
            
            if (filter.IsActive.HasValue)
                criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(d => d.STATUSID.Equals(filter.Id));

            if (filter.Ids.Any())
                criteria = criteria.And(d => filter.Ids.Contains(d.STATUSID));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.STATUSNAME.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.STATUSID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.TAXSTATUS.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.FAMILYSTATUS.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }

            if (filter.PageSize <= 0)
                return _context.MSTATUS.Where(criteria);
            return _context.MSTATUS.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        private void Validate(MSTATUS record, string userName)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.STATUSID)) result += "Kode Status tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.STATUSNAME)) result += "Nama Status tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.FAMILYSTATUS)) result += "Status Keluarga tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.TAXSTATUS)) result += "Status Pajak tidak boleh kosong." + Environment.NewLine;

            if (_context.MSTATUS.SingleOrDefault(d => d.STATUSID.Equals(record.STATUSID)) != null) result += "Status dengan kode tersebut sudah terdaftar." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        protected override MSTATUS BeforeSave(MSTATUS record, string userName, bool newRecord)
        {
            if (record.ABSENSEX == "*")
                record.ABSENSEX = "";

            DateTime currentDate = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATEBY = userName;
                record.CREATED = currentDate;
            }
            record.UPDATEBY = userName;
            record.UPDATED = currentDate;

            Validate(record, userName);
            return record;
        }

       

        protected override MSTATUS AfterSave(MSTATUS record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.DHSUpdateMaster(userName, record.UPDATED, _serviceName, _context);
            return record;
            
        }
        

       

        protected override bool DeleteFromDB(MSTATUS record, string userName)
        {
            record = GetSingle(record.STATUSID);
            record.ACTIVE = false;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }
    }
}
