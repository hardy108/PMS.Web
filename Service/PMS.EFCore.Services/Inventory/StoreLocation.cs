using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using PMS.EFCore.Services.Utilities;
using PMS.Shared.Utilities;
using PMS.EFCore.Helper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


using FileStorage.EFCore;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Inventory
{
    public class StoreLocation : EntityFactory<MSTORELOCATION,MSTORELOCATION,FilterCompany, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public StoreLocation(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _authenticationService = authenticationService;
            _serviceName = "StoreLocation";
        }

        private void Validate(MSTORELOCATION record, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.CODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.LOCID)) result += "Lokasi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.NAME)) result += "Nama tidak boleh kosong." + Environment.NewLine;
            if (_context.MSTORELOCATION.SingleOrDefault(p => p.CODE.Equals(record.CODE)) != null)
                result += "Kode " + record.CODE + " sudah ada." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        protected override MSTORELOCATION BeforeSave(MSTORELOCATION record, string userName, bool newRecord)
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

        

        protected override MSTORELOCATION GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.MSTORELOCATION.SingleOrDefault(d => d.CODE.Equals(id));
        }

        public override IEnumerable<MSTORELOCATION> GetList(FilterCompany filter)
        {            
            var criteria = PredicateBuilder.True<MSTORELOCATION>();

            try
            {
                List<string> idVorg = new List<string>();

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                {
                    idVorg = _context.VORGANIZATION.Where(a => a.LV2ID.Equals(filter.UnitID)).Select(b => b.ID).ToList();
                }

                if (idVorg.Any())
                    criteria = criteria.And(d => idVorg.Contains(d.LOCID));

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p =>
                        p.LOCID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.CODE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm));

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(d => d.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.CODE == filter.Id);
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.CODE));

                if (filter.PageSize <= 0)
                    return _context.MSTORELOCATION.Where(criteria);
                return _context.MSTORELOCATION.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }

        protected override bool DeleteFromDB(MSTORELOCATION record, string userName)
        {
            record = GetSingle(record.CODE);
            record.ACTIVE = false;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }
    }
}