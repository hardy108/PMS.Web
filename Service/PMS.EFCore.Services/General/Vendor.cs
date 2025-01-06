using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;

using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PMS.Shared.Utilities;
using PMS.EFCore.Helper;
using FileStorage.EFCore;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{    
    public class Vendor : EntityFactory<MCARD,MCARD,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Vendor(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Vendor";
            _authenticationService = authenticationService;
        }

        private void Validate(MCARD record, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.CODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.TYPE)) result += "Tipe tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.NAME)) result += "Nama tidak boleh kosong." + Environment.NewLine;
            if (_context.MCARD.SingleOrDefault(p => p.CODE.Equals(record.CODE)) != null)
                result += "Kode sudah ada." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        protected override MCARD BeforeSave(MCARD record, string userName, bool newRecord)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.UPDATED = now;
            record.UPDATEBY = userName;
            if (newRecord)
            {
                record.CREATED = now;
                record.CREATEBY = userName;
             
            }
            this.Validate(record, userName);
            
            return record;
        }

        

        protected override MCARD GetSingleFromDB(params object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.MCARD.SingleOrDefault(d => d.CODE.Equals(id));
        }

        public override MCARD CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MCARD record = base.CopyFromWebFormData(formData, newRecord);//If Required Replace with your custom code
            
            if (string.IsNullOrWhiteSpace(formData["ACCOUNTCODE"]))
                record.ACCOUNTCODE = null;

            if (string.IsNullOrWhiteSpace(formData["ADDRESS1"]))
                record.ADDRESS1 = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["ADDRESS2"]))
                record.ADDRESS2 = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["ADDRESS3"]))
                record.ADDRESS3 = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["ADDRESS4"]))
                record.ADDRESS4 = string.Empty;

            return record;
        }

        public override IEnumerable<MCARD> GetList(GeneralFilter filter)
        {            
            var criteria = PredicateBuilder.True<MCARD>();
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p =>
                        string.IsNullOrEmpty(p.ACCOUNTCODE).ToString().Contains(filter.SearchTerm) ||
                        p.CODE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.TYPE_IN_TEXT.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm));

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(d => d.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.CODE == filter.Id);
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.CODE));

                if (filter.PageSize <= 0)
                    return _context.MCARD.Where(criteria).ToList();
                return _context.MCARD.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }
    }
}