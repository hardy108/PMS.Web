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
    public class Tph : EntityFactory<MTPH,MTPH,FilterBlock, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Tph(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Tph";
            _authenticationService = authenticationService;
        }

        private void Validate(MTPH record, string userName)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.TPHID)) result += "Id harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.UNITID)) result += "Estate harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Divisi harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.BLOCKID)) result += "Blok harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.CODE)) result += "Kode TPH harus diisi." + Environment.NewLine;
            if (_context.MTPH.SingleOrDefault(d => d.TPHID.Equals(record.TPHID)) != null) result += "Tph sudah pernah diinput." + Environment.NewLine;
            if (result != string.Empty) throw new Exception(result);
        }

        protected override MTPH GetSingleFromDB(params  object[] keyValues)
        {
            string tphId = keyValues[0].ToString();
            return _context.MTPH.SingleOrDefault(d => d.TPHID.Equals(tphId));
        }

        public override MTPH CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MTPH record = base.CopyFromWebFormData(formData, newRecord);//If Required Replace with your custom code
            return record;
        }

        protected override MTPH BeforeSave(MTPH record, string userName, bool newRecord)
        {
            record.LAT = StandardUtility.IsNull<string>(record.LAT, string.Empty);
            record.LONG = StandardUtility.IsNull<string>(record.LONG, string.Empty);
            DateTime now = GetServerTime();
            if (newRecord)
            {
                string newId = record.BLOCKID.Replace("-", "");
                record.TPHID = newId + record.CODE;
                record.ACTIVE = true;
            }
            record.UPDATED = now;
            this.Validate(record, userName);
            return record;
        }

        
        

        public override IEnumerable<MTPH> GetList(FilterBlock filter)
        {
            
            var criteria = PredicateBuilder.True<MTPH>();
            
            if (filter.IsActive.HasValue)
                criteria = criteria.And(d => d.ACTIVE == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(d => d.TPHID.Equals(filter.Id));


            if (filter.Ids.Any())
                criteria = criteria.And(d => filter.Ids.Contains(d.TPHID));

            if (!string.IsNullOrWhiteSpace(filter.BlockID))
                criteria = criteria.And(d => d.BLOCKID.Equals(filter.BlockID));

            if (filter.BlockIDs.Any())
                criteria = criteria.And(d => filter.BlockIDs.Contains(d.BLOCKID));

            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(d => d.DIVID.Equals(filter.DivisionID));

            if (filter.DivisionIDs.Any())
                criteria = criteria.And(d => filter.DivisionIDs.Contains(d.DIVID));

            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(d => d.UNITID.Equals(filter.UnitID));

            if (filter.UnitIDs.Any())
                criteria = criteria.And(d => filter.UnitIDs.Contains(d.UNITID));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And
                    (
                        d => d.DIVID.Contains(filter.SearchTerm) ||
                            d.UNITID.Contains(filter.SearchTerm) ||
                            d.BLOCKID.Contains(filter.SearchTerm) ||
                            d.TPHID.Contains(filter.SearchTerm)
                    );
            }

            if (filter.PageSize <= 0)
                return _context.MTPH.Where(criteria);
            return _context.MTPH.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            
        }
    }
}
