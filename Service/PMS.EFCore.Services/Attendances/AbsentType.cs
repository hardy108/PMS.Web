using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;
using PMS.EFCore.Helper;
using PMS.EFCore.Services.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Entities
{
    public class AbsentType : EntityFactory<MABSENTTYPE, MABSENTTYPE,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;

        public AbsentType(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "AbsentType";
            _authenticationService = authenticationService;
            
        }

        private void Validate(MABSENTTYPE record, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.ABSENTCODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.ABSENTDESCRIPTION)) result += "Keterangan tidak boleh kosong." + Environment.NewLine;
            if (_context.MABSENTTYPE.SingleOrDefault(p => p.ABSENTCODE.Equals(record.ABSENTCODE)) != null)
                result += "Kode " + record.ABSENTCODE + " sudah ada." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        protected override MABSENTTYPE GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.MABSENTTYPE.SingleOrDefault(d => d.ABSENTCODE.Equals(id));
        }

        protected override MABSENTTYPE BeforeSave(MABSENTTYPE record, string userName, bool newRecord)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = now;
                record.CREATEBY = userName;
            }
            record.UPDATED = now;
            record.UPDATEBY = userName;
            

            if (record.ABSENSEX == "*")
                record.ABSENSEX = "";
            Validate(record, userName);
            return record;
        }

        

        

        public override IEnumerable<MABSENTTYPE> GetList(GeneralFilter filter)
        {
            var criteria = PredicateBuilder.True<MABSENTTYPE>();
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p =>
                        p.ABSENTCODE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.ABSENTDESCRIPTION.ToLower().Contains(filter.LowerCasedSearchTerm));

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(d => d.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.ABSENTCODE == filter.Id);

                if (filter.PageSize <= 0)
                    return _context.MABSENTTYPE.Where(criteria).ToList();
                return _context.MABSENTTYPE.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }
    }
}