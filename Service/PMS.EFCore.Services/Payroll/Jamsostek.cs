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
    public class Jamsostek : EntityFactory<MJAMSOSTEK,MJAMSOSTEK,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Jamsostek(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Jamsostek";
            _authenticationService = authenticationService;
        }

        private string FieldsValidation(MJAMSOSTEK jamsostek)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(jamsostek.UNITCODE)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (jamsostek.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        private void Validate(MJAMSOSTEK jamsostek)
        {
            string result = this.FieldsValidation(jamsostek);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        protected override MJAMSOSTEK BeforeSave(MJAMSOSTEK record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                if (_context.MJAMSOSTEK.SingleOrDefault(d => d.UNITCODE.Equals(record.UNITCODE)) != null)
                    throw new Exception("Id sudah terdaftar.");

                record.STATUS = "A";
                record.CREATED = Utilities.HelperService.GetServerDateTime(1, _context);                
                record.CREATEBY = userName;
            }
            record.UPDATEBY = userName;
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            this.Validate(record);
            return record;
        }

       

        protected override MJAMSOSTEK BeforeDelete(MJAMSOSTEK record, string userName)
        {
            this.Validate(record);
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            record.UPDATEBY = userName;
            record.STATUS = "D";
            return record;
        }

        //protected override bool DeleteFromDB(MJAMSOSTEK record, string userName)
        //{
        //    _context.MJAMSOSTEK.Update(record);
        //    _context.SaveChanges();

        //    return true;
        //}

        protected override MJAMSOSTEK GetSingleFromDB(params  object[] keyValues)
        {
            return _context.MJAMSOSTEK
            .SingleOrDefault(d => d.UNITCODE.Equals(keyValues[0]));
        }

        public override IEnumerable<MJAMSOSTEK> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<MJAMSOSTEK>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITCODE == filter.UnitID);
            }

            if (filter.PageSize <= 0)
                return _context.MJAMSOSTEK.Where(criteria).ToList();
            return _context.MJAMSOSTEK.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public  IEnumerable<MJAMSOSTEK> GetAll(object filterParameter)
        {
            try
            {
                return _context.MJAMSOSTEK.ToList();
            }
            catch { return new List<MJAMSOSTEK>(); }
        }
    }

}