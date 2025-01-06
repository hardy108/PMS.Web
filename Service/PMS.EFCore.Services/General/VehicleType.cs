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
    public class VehicleType : EntityFactory<MVEHICLETYPE,MVEHICLETYPE,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public VehicleType(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "VehicleType";
            _authenticationService = authenticationService;
        }

        private void Validate(MVEHICLETYPE record, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.NAME)) result += "Nama tidak boleh kosong." + Environment.NewLine;
            if (_context.MVEHICLETYPE.SingleOrDefault(p => p.ID.Equals(record.ID)) != null)
                result += "Id " + record.ID + " sudah ada." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        protected override MVEHICLETYPE GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.MVEHICLETYPE.SingleOrDefault(d => d.ID.Equals(id));
        }

        public override IEnumerable<MVEHICLETYPE> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<MVEHICLETYPE>();
            try
            {
                if (filter.IsActive.HasValue)
                    criteria = criteria.And(d => d.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.ID == filter.Id);
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.ID));

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    criteria = criteria.And(d => d.ID.Contains(filter.Keyword) || d.NAME.Contains(filter.Keyword));

                if (filter.PageSize <= 0)
                    return _context.MVEHICLETYPE.Where(criteria).ToList();
                return _context.MVEHICLETYPE.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }
        
        protected override MVEHICLETYPE BeforeSave(MVEHICLETYPE record, string userName, bool newRecord)
        {
            if (record.HMKM == "*")
                record.HMKM = "";
            Validate(record, userName);
            return record;
        }
        
        
    }
}
