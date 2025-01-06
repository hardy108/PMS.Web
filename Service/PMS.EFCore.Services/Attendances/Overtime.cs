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
    public class Overtime : EntityFactory<TOVERTIME,TOVERTIME,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Overtime(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Overtime";
            _authenticationService = authenticationService;
        }

        private void Validate(TOVERTIME overtime, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(overtime.DOCNO)) result += "No tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(overtime.DIVID)) result += "Divisi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(overtime.EMPLOYEEID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;
            if (overtime.DOCDATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (overtime.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        protected override TOVERTIME GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            TOVERTIME record = _context.TOVERTIME
                .Include(d => d.EMPLOYEE)
                .Include(d => d.ACT)
                .Include(d => d.DIV)
                .Include(d => d.LOC)
                .Where(i => i.DOCNO.Equals(Id)).SingleOrDefault();
            return record;
        }

        public override TOVERTIME NewRecord(string userName)
        {
            TOVERTIME record = new TOVERTIME();
            record.DOCDATE = GetServerTime().Date;
            record.STATUS = "";

            return record;
        }

        public override IEnumerable<TOVERTIME> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<TOVERTIME>();

            criteria = criteria.And(d =>
                (d.DOCDATE.Date >= filter.StartDate.Date && d.DOCDATE.Date <= filter.EndDate.Date));

            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                    criteria = criteria.And(p => p.DIVID.Equals(filter.DivisionID));

            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));


            if (filter.PageSize <= 0)
                return _context.TOVERTIME.Where(criteria);
            return _context.TOVERTIME.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;


        }

        private void ApproveValidate(TOVERTIME overtime)
        {
           
            if (overtime.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (overtime.STATUS == "C")
                throw new Exception("Data sudah di Cancel.");
        }

        public bool Approve(string code, string userName)
        {
            var record = GetSingle(code);
            this.ApproveValidate(record);

            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.UPDATEBY = userName;
            record.UPDATED = now;
            record.STATUS = "A";

            var local = _context.Set<TOVERTIME>()
                .Local
                .FirstOrDefault(entry => entry.DOCNO.Equals(code));

            if (local != null)
            { _context.Entry(local).State = EntityState.Detached; }

            _context.Entry<TOVERTIME>(record).State = EntityState.Modified;
            _context.SaveChanges();

            return true;
        }

        private void CancelValidate(TOVERTIME overtime)
        {
            if (overtime.STATUS != "A")
                throw new Exception("Data belum di approve.");
        }

        public bool Cancel(string code, string userName)
        {
            var record = GetSingle(code);
            this.CancelValidate(record);

            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.UPDATEBY = userName;
            record.UPDATED = now;
            record.STATUS = "C";

            _context.Entry<TOVERTIME>(record).State = EntityState.Modified;
            _context.SaveChanges();


            return true;
        }

        protected override TOVERTIME BeforeSave(TOVERTIME record, string userName, bool newRecord)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.CREATED = now;
                record.CREATEBY = userName;
                record.STATUS = "P";
                record.DOCNO = PMSConstants.OvertimeCodePrefix + "-" + record.DIVID + "-" + record.DOCDATE.ToString("yyMM") + "-" + HelperService.GetCurrentDocumentNumber(PMSConstants.OvertimeCodePrefix + record.DIVID, _context).ToString("0000");
            }
            record.UPDATED = now;
            record.UPDATEBY = userName;
            

            this.Validate(record, userName);

            return record;
        }

        

        protected override TOVERTIME AfterSave(TOVERTIME record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.OvertimeCodePrefix + record.DIVID, _context);
            return record;
        }

        

        private void DeleteValidate(TOVERTIME overtime)
        {
            if (overtime.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (overtime.STATUS == "C")
                throw new Exception("Data sudah di cancel.");
        }

        protected override TOVERTIME BeforeDelete(TOVERTIME record, string userName)
        {
            DeleteValidate(record);
            return record;
        }
    }

}
