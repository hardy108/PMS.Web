using System;
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
using PMS.EFCore.Services.General;
using AM.EFCore.Services;
using PMS.EFCore.Services.Organization;

namespace PMS.EFCore.Services.Attendances
{
    public class ScheduleEmpoyee:EntityFactory<MSCHEDULEEMPLOYEE,MSCHEDULEEMPLOYEE,GeneralFilter, PMSContextBase>
    {
        private Employee _serviceEmpPos;

        public ScheduleEmpoyee(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "ScheduleEmpoyee";
        }

        public override IEnumerable<MSCHEDULEEMPLOYEE> GetList(GeneralFilter filter)
        {
            var criteria = PredicateBuilder.True<MSCHEDULEEMPLOYEE>();
            try
            {
                if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS == PMSConstants.TransactionStatusApproved);
                else
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITID.Equals(filter.UnitID));

                if (filter.StartDate != null && filter.EndDate != null)
                    criteria = criteria.And(p => p.DATE >= filter.StartDate.Date && p.DATE <= filter.EndDate.Date);

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    criteria = criteria.And(p => p.UNITID.Contains(filter.Keyword) 
                    || p.EMPLOYEEID.Contains(filter.Keyword)
                    || p.EMPLOYEE.EMPNAME.Contains(filter.Keyword)
                    );

                if (filter.PageSize <= 0)
                    return _context.MSCHEDULEEMPLOYEE.Include(d => d.EMPLOYEE).Where(criteria);

                return _context.MSCHEDULEEMPLOYEE.Include(d => d.EMPLOYEE).Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return new List<MSCHEDULEEMPLOYEE>(); }
        }

        public override MSCHEDULEEMPLOYEE NewRecord(string userName)
        {
            return new MSCHEDULEEMPLOYEE
            {
                DATE = DateTime.Today,
                HOLIDAY = false
            };
        }

        protected override MSCHEDULEEMPLOYEE GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.MSCHEDULEEMPLOYEE
                .Include(d => d.EMPLOYEE)
                .SingleOrDefault(d => d.ID.Equals(Id));
        }

        public override MSCHEDULEEMPLOYEE CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MSCHEDULEEMPLOYEE record = base.CopyFromWebFormData(formData, newRecord);//If Required Replace with your custom code
            if (record.EMPLOYEE == null)
                record.EMPLOYEE = _context.MEMPLOYEE.Find(record.EMPLOYEEID);

            return record;
        }

        private void Validate(MSCHEDULEEMPLOYEE record, string userName)
        {
            if (string.IsNullOrEmpty(record.UNITID)) throw new Exception("Estate tidak boleh kosong.");
            if (string.IsNullOrEmpty(record.EMPLOYEEID)) throw new Exception("Karyawan tidak boleh kosong.");

            if (record.HOLIDAY == false)
            {
                if (record.INSTART.Hour == 0 && record.INSTART.Minute == 0) throw new Exception ("Batas Datang Awal tidak boleh kosong.");
                if (record.INEND.Hour == 0 && record.INEND.Minute == 0) throw new Exception("Batas Datang Akhir tidak boleh kosong.");
                if (record.OUTSTART.Hour == 0 && record.OUTSTART.Minute == 0) throw new Exception("Batas Pulang Awal tidak boleh kosong.");
                if (record.OUTEND.Hour == 0 && record.OUTEND.Minute == 0) throw new Exception("Batas Pulang Akhir tidak boleh kosong.");
                if (record.INTIME.Hour == 0 && record.INTIME.Minute == 0) throw new Exception("Jadwal Datang tidak boleh kosong.");
                if (record.OUTTIME.Hour == 0 && record.OUTTIME.Minute == 0) throw new Exception("Jadwal Pulang tidak boleh kosong.");
            }

            var period = _context.MPERIOD.Where(d => d.UNITCODE == record.UNITID && ((d.FROM1 <= record.DATE && d.TO1 >= record.DATE && d.ACTIVE1 == true) || (d.FROM2 <= record.DATE && d.TO2 >= record.DATE && d.ACTIVE2 == true)));
            if (period.Count() == 0)
                throw new Exception("Tanggal harus dalam periode aktif (" + record.EMPLOYEE.UNITCODE + ").");
        }

        private void ValidateInsert(MSCHEDULEEMPLOYEE record, string userName)
        {
            Validate(record, userName);

            //if (record.EMPLOYEE == null)
            //{
            //    var emp = new MEMPLOYEE();
            //    emp.CopyFrom(_serviceEmpPos.GetSingle(record.EMPLOYEEID));
            //    record.EMPLOYEE = emp;
            //}
            
            var posDetail = _context.MPOSITIONDETAIL.Where(d => d.UNITID == record.UNITID && d.POSID == record.EMPLOYEE.POSITIONID).SingleOrDefault();
            if (posDetail == null) posDetail = new MPOSITIONDETAIL();
            if (!posDetail.ALLOWSCHEDULE)
                throw new Exception("Karyawan dengan jabatan ini tidak berhak membuat jadwal karyawan.");

            if (_context.MSCHEDULEEMPLOYEE.SingleOrDefault(d => d.EMPLOYEEID.Equals(record.EMPLOYEEID) && d.DATE.Equals(record.DATE) && d.STATUS.Equals(PMSConstants.TransactionStatusApproved)) != null)
                throw new Exception("Karyawan sudah terdaftar di tanggal tersebut");
        }

        protected override MSCHEDULEEMPLOYEE BeforeSave(MSCHEDULEEMPLOYEE record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                ValidateInsert(record, userName);

                int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.ScheduleEmployeeIdPrefix + record.EMPLOYEE.UNITCODE, _context);
                record.ID = PMSConstants.ScheduleEmployeeIdPrefix + "-" + record.UNITID + "-" + record.DATE.ToString("yyyyMMdd") + lastNumber.ToString("0000");
                record.STATUS = PMSConstants.TransactionStatusApproved;
            }
            Validate(record, userName);

            DateTime now = HelperService.GetServerDateTime(1, _context);

            record.INSTART = new DateTime(1900, 1, 1, record.INSTART.Hour, record.INSTART.Minute, 0);

            record.INEND = new DateTime(1900, 1, 1, record.INEND.Hour, record.INEND.Minute, 0);
            if (record.INEND < record.INSTART)
                record.INEND = record.INEND.AddDays(1);

            record.OUTSTART = new DateTime(1900, 1, 1, record.OUTSTART.Hour, record.OUTSTART.Minute, 0);
            if (record.OUTSTART < record.INSTART)
                record.OUTSTART = record.OUTSTART.AddDays(1);

            record.OUTEND = new DateTime(1900, 1, 1, record.OUTEND.Hour, record.OUTEND.Minute, 0);
            if (record.OUTEND < record.INSTART)
                record.OUTEND = record.OUTEND.AddDays(1);

            record.BREAKSTART = new DateTime(1900, 1, 1, record.BREAKSTART.Hour, record.BREAKSTART.Minute, 0);
            if (record.BREAKSTART < record.INSTART)
                record.BREAKSTART = record.BREAKSTART.AddDays(1);

            record.BREAKEND = new DateTime(1900, 1, 1, record.BREAKEND.Hour, record.BREAKEND.Minute, 0);
            if (record.BREAKEND < record.INSTART)
                record.BREAKEND = record.BREAKEND.AddDays(1);

            record.INTIME = new DateTime(1900, 1, 1, record.INTIME.Hour, record.INTIME.Minute, 0);
            if (record.INTIME < record.INSTART)
                record.INTIME = record.INTIME.AddDays(1);

            record.OUTTIME = new DateTime(1900, 1, 1, record.OUTTIME.Hour, record.OUTTIME.Minute, 0);
            if (record.OUTTIME < record.INSTART)
                record.OUTTIME = record.OUTTIME.AddDays(1);

            record.UPDATED = now;
            return record;
        }

        

        protected override MSCHEDULEEMPLOYEE AfterSave(MSCHEDULEEMPLOYEE record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.ScheduleEmployeeIdPrefix + record.EMPLOYEE.UNITCODE, _context);
            return record;
        }
        
        protected override bool DeleteFromDB(MSCHEDULEEMPLOYEE record, string userName)
        {
            Validate(record, userName);

            record = GetSingle(record.ID);
            if (record == null)
                throw new Exception("Record not found");

            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.STATUS = "D";
            record.UPDATED = now;
            _context.Entry<MSCHEDULEEMPLOYEE>(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }
    }
}
