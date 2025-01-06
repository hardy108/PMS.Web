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
    public class AbsenceReason : EntityFactory<MABSENCEREASON, MABSENCEREASON,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;

        public AbsenceReason(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "AbsentReason";
            _authenticationService = authenticationService;
            
        }

        private void Validate(MABSENCEREASON record, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.ID)) result += "Kode tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.TEXT)) result += "Keterangan tidak boleh kosong." + Environment.NewLine;
            if (_context.MABSENCEREASON.SingleOrDefault(p => p.ID.Equals(record.ID)) != null)
                result += "Kode " + record.ID + " sudah ada." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        protected override MABSENCEREASON GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.MABSENCEREASON.SingleOrDefault(d => d.ID.Equals(id));
        }

        protected override MABSENCEREASON BeforeSave(MABSENCEREASON record, string userName,bool newRecord)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = now;
                record.CREATEBY = userName;
             
                
            }
            record.UPDATED = now;
            record.UPDATEBY = userName;
            this.Validate(record, userName);
            return record;
        }

        

        public override IEnumerable<MABSENCEREASON> GetList(GeneralFilter filter)
        {
            var criteria = PredicateBuilder.True<MABSENCEREASON>();
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p =>
                        p.ID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.TEXT.ToLower().Contains(filter.LowerCasedSearchTerm));

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(d => d.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.ID == filter.Id);

                if (filter.PageSize <= 0)
                    return _context.MABSENCEREASON.Where(criteria).ToList();
                return _context.MABSENCEREASON.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }
    }
}