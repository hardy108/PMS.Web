using System;
using System.Collections;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using PMS.EFCore.Services.General;
using PMS.EFCore.Helper;
using PMS.EFCore.Services;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{    
    public class Holiday : EntityFactory<TCALENDAR,TCALENDAR,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Holiday(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Holiday";
            _authenticationService = authenticationService;            
        }

        private void Validate(TCALENDAR record, string user)
        {
            string result = string.Empty;
            if (record.DTDATE == new DateTime()) result += "Tanggal harus dipilih." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.REMARK)) result += "Keterangan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DAYTYPE)) result += "Pilih apakah hari minggu atau hari libur." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.STATUS)) result += "Status harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.UNITCODE)) result += "Estate tidak boleh kosong." + Environment.NewLine;
            if (_context.TCALENDAR.SingleOrDefault(d => d.HOLIDAY.Equals(record.HOLIDAY) && d.STATUS.Equals(record.STATUS == "A")) != null) result += "Tanggal sudah ada." + Environment.NewLine;
            
            if (result != string.Empty) throw new Exception(result);
        }

        public override TCALENDAR NewRecord(string userName)
        {
            TCALENDAR record = new TCALENDAR
            {
                DTDATE = DateTime.Today,
                REMARK = string.Empty,
                STATUS = "A"
            };
            return record;
        }

        protected override TCALENDAR GetSingleFromDB(params object[] keyValues)
        {
            string unitCode = null;
            DateTime date = new DateTime();

            if (keyValues.Count() == 1)
            {
                string Id = keyValues[0].ToString();
                string[] key = Id.Split('_');
                unitCode = key[0];
                date = DateTime.ParseExact(key[1], "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            else
            {
                unitCode = keyValues[0].ToString();
                date = DateTime.Parse(keyValues[1].ToString());
            }

            return _context.TCALENDAR
                .Where(a => a.UNITCODE.Equals(unitCode) && a.DTDATE.Date.Equals(date))
                .SingleOrDefault();
        }

        public override TCALENDAR CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TCALENDAR record = base.CopyFromWebFormData(formData, newRecord);

            if (string.IsNullOrWhiteSpace(formData["REMARK"]))
                record.REMARK = string.Empty;

            if (formData["DAYTYPE"] == "HOLIDAY")
            {
                record.HOLIDAY = true;
                record.SUNDAY = false;
            }
            else
            {
                record.HOLIDAY = false;
                record.SUNDAY = true;
            }
            return record;
        }

        protected override TCALENDAR BeforeSave(TCALENDAR record, string userName, bool newRecord)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = now;
                record.CREATEBY = userName;                
                record.STATUS = PMSConstants.TransactionStatusApproved;
        
            }
            record.UPDATED = now;
            record.UPDATEBY = userName;
            this.Validate(record, userName);
            return record;
        }

        

        public override IEnumerable<TCALENDAR> GetList(GeneralFilter filter)
        {            
            var criteria = PredicateBuilder.True<TCALENDAR>();
            try
            {
                if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS == PMSConstants.TransactionStatusApproved);
                else
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));

                if (filter.StartDate != null && filter.EndDate != null)
                   criteria = criteria.And(p => p.DTDATE >= filter.StartDate.Date && p.DTDATE <= filter.EndDate.Date);
                
                return _context.TCALENDAR.Where(criteria).ToList();
            }
            catch { return new List<TCALENDAR>(); }
        }

        protected override bool DeleteFromDB(TCALENDAR record, string userName)
        {
            TCALENDAR Holidays = (from p in _context.TCALENDAR
                                 where p.DTDATE == record.DTDATE && p.UNITCODE == record.UNITCODE
                                 select p).SingleOrDefault();

            Holidays.STATUS = PMSConstants.TransactionStatusDeleted;
            Holidays.UPDATEBY = userName;
            Holidays.UPDATED = HelperService.GetServerDateTime(1, _context);
            _context.TCALENDAR.Update(Holidays);
            _context.SaveChanges();

            return true;
        }
    }
}
