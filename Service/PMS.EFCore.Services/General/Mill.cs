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



using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{
    public class Mill : EntityFactory<MMILL,MMILL,FilterMill, PMSContextBase>
    {
        public Mill(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Mill";
        }

        private string FieldsValidation(MMILL mill)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(mill.MILLCODE)) result += "Kode harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(mill.MILLNAME)) result += "Nama harus diisi." + Environment.NewLine;
            return result;
        }

        private void Validate(MMILL mill)
        {
            string result = this.FieldsValidation(mill);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void InsertValidate(MMILL mill)
        {
            this.Validate(mill);

            var millExist = GetSingle(mill.MILLCODE);
            if (millExist != null)
                throw new Exception("Mill sudah pernah diinput.");
        }

        protected override MMILL GetSingleFromDB(params  object[] keyValues)
        {
            return _context.MMILL.Where(d => d.MILLCODE.Equals(keyValues[0])).SingleOrDefault();
        }

        protected override MMILL BeforeSave(MMILL record, string userName, bool newRecord)
        {
            DateTime now = GetServerTime();
            record.UPDATEBY = userName;
            record.UPDATED = now;
            if (newRecord)
            {
                
                record.CREATEBY = userName;
                record.CREATED = now;
                record.ACTIVE = true;

                InsertValidate(record);
            }
            else
                Validate(record);
            
            return record;
        }

        

        public override IEnumerable<MMILL> GetList(FilterMill filter)
        {
            
            var criteria = PredicateBuilder.True<MMILL>();
            try
            {
                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.MILLCODE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.MILLNAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.MillCode))
                    criteria = criteria.And(p => p.MILLCODE.Equals(filter.MillCode));

                if (!string.IsNullOrWhiteSpace(filter.MillName))
                    criteria = criteria.And(p => p.MILLNAME.Equals(filter.MillName));

                return _context.MMILL.Where(criteria).ToList();

            }
            catch { return new List<MMILL>(); }
        }




    }
}
