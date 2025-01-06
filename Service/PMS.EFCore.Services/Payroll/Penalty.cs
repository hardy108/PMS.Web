using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class Penalty : EntityFactory<MPENALTY,MPENALTY,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Penalty(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Penalty";
            _authenticationService = authenticationService;
        }


        protected override MPENALTY GetSingleFromDB(params  object[] keyValues)
        {
            return _context.MPENALTY
            .Include(d => d.PENALTYCODENavigation)
            .Include(d => d.UNITCODENavigation)
            .SingleOrDefault(d => d.PENALTYID.Equals(keyValues[0]));
        }

        //public override IEnumerable<MPENALTY> GetList(GeneralFilter filter)
        //{
            
        //    var result = _context.MPENALTY.Where
        //        (
        //            d =>
        //            (d.UNITCODE.Equals(filter.UnitID) || string.IsNullOrWhiteSpace(filter.UnitID)) &&
        //            (d.PENALTYCODE.Equals(filter.Id) || string.IsNullOrWhiteSpace(filter.Id))

        //        );

        //    return result.ToList();
        //}

        private void Validate(MPENALTY PNLTY, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(PNLTY.PENALTYID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(PNLTY.UNITCODE)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(PNLTY.PENALTYCODE)) result += "Kode tidak boleh kosong." + Environment.NewLine;

           

            if (result != string.Empty) throw new Exception(result);
        }

        protected override MPENALTY BeforeSave(MPENALTY record, string userName, bool newRecord)
        {
            DateTime now = GetServerTime();
            record.UPDATEDDATE = now;
            record.UPDATEBY = userName;
            if (newRecord)
            {
                if (_context.MPENALTY.SingleOrDefault(d => d.PENALTYID.Equals(record.PENALTYID)) != null)
                    throw new Exception("Kode sudah ada.");

                record.CREATEDDATE = now;
                record.CREATEBY = userName;                
                record.PENALTYID = record.UNITCODE + "-" + record.PENALTYCODE;
            }

            this.Validate(record, userName);

            return record;
        }

    

        public override IEnumerable<MPENALTY> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<MPENALTY>();

            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));

            //if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
            //    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));


            if (filter.PageSize <= 0)
                return _context.MPENALTY.Where(criteria);
            return _context.MPENALTY.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;


        }
       
    }

}