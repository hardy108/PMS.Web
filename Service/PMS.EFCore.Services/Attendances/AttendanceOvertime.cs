using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Attendances
{
    public class AttendanceOvertime : EntityFactory<TATTENDANCESPL, TATTENDANCESPL,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public AttendanceOvertime(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "AttendanceOvertime";
            _authenticationService = authenticationService;
        }

        private void Validate(TATTENDANCESPL overtime, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(overtime.UNITID)) result += "UNIT tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(overtime.NO)) result += "Nomer tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(overtime.EMPID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;
            if (overtime.DATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;


            if (result != string.Empty) throw new Exception(result);
        }

        protected override TATTENDANCESPL GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            TATTENDANCESPL record = _context.TATTENDANCESPL
                .Include(a => a.EMP)
                .Include(b => b.UNIT)             
                .Where(i => i.ID.Equals(Id)).SingleOrDefault();

            return record;
        }

        public override TATTENDANCESPL NewRecord(string userName)
        {
            TATTENDANCESPL record = new TATTENDANCESPL();
            record.DATE = GetServerTime().Date;
            record.STATUS = "";

            return record;
        }

        public override IEnumerable<TATTENDANCESPL> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<TATTENDANCESPL>();

                criteria = criteria.And(d =>
                (d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITID.Equals(filter.UnitID));

                //if (!string.IsNullOrWhiteSpace(filter.Id))
                //    criteria = criteria.And(p => p.EMPID.Equals(filter.Id));


                if (filter.PageSize <= 0)
                    return _context.TATTENDANCESPL.Where(criteria);
                return _context.TATTENDANCESPL.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
      

        }

        protected override TATTENDANCESPL BeforeSave(TATTENDANCESPL record, string userName, bool newRecord)
        {
            this.Validate(record, userName);
            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                if (_context.TATTENDANCESPL.SingleOrDefault(d => d.EMPID.Equals(record.EMPID) && d.DATE.Equals(record.DATE)) != null)
                    throw new Exception("Karyawan sudah terdaftar di tanggal tersebut");
                record.STATUS = "A";
                record.NO = HelperService.GetCurrentDocumentNumber(PMSConstants.AttendanceOvertimeIdPrefix + record.UNITID, _context).ToString("0000");
                record.ID = PMSConstants.AttendanceOvertimeIdPrefix + "-" + record.DATE.ToString("yyyyMMdd") + "-" + HelperService.GetCurrentDocumentNumber(PMSConstants.AttendanceOvertimeIdPrefix + record.UNITID, _context).ToString("0000");
            }

            
            record.UPDATED = now;
            return record;
        }


        protected override TATTENDANCESPL AfterSave(TATTENDANCESPL record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.AttendanceOvertimeIdPrefix + record.UNITID, _context);
            return record;
        }

       

        protected override bool DeleteFromDB(TATTENDANCESPL record, string userName)
        {
            record = GetSingle(record.ID);
            record.STATUS = "D";
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            _context.Entry<TATTENDANCESPL>(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;

        }
    }

}
