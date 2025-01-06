using AM.EFCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.GL;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PMS.EFCore.Services.Attendances
{
    public class Travel : EntityFactory<TTRAVEL, TTRAVEL, GeneralFilter, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;
        private Attendance _serviceAttName;

        private AuthenticationServiceBase _authenticationService;
        public Travel(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Travel";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context, _authenticationService, auditContext);
            _serviceDivisiName = new Divisi(context, _authenticationService, auditContext);
            _serviceAttName = new Attendance(context, _authenticationService, auditContext);
        }

        public override TTRAVEL CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TTRAVEL record = base.CopyFromWebFormData(formData, newRecord);

            if (record.DIVID == null)
            {
                this.Validate(record);
            }

            if (record.UNITID == null)
            {
                var divisi = new MDIVISI();
                divisi.CopyFrom(_serviceDivisiName.GetSingle(record.DIVID));

                record.UNITID = divisi.UNITCODE;
            }

            return record;
        }

        protected override TTRAVEL GetSingleFromDB(params object[] keyValues)
        {
            TTRAVEL record = _context.TTRAVEL.Include(b=>b.UNIT).Include(c => c.DIV).Include(d=>d.EMPLOYEE)
                .Where(a => a.IDTRAVEL.Equals(keyValues[0])).SingleOrDefault();

            return record;
            //return base.GetSingleFromDB(keyValues);
        }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.TravelPrefix + unitCode, _context);
            return PMSConstants.TravelPrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override TTRAVEL BeforeSave(TTRAVEL record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                record.STATUS = PMSConstants.TransactionStatusProcess;

                this.InsertValidate(record);
                record.IDTRAVEL = GenereteNewCode(record.UNITID, record.STARTDATE);
            }
            else
                UpdateValidate(record);
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;

            return record;
        }

        protected override TTRAVEL AfterSave(TTRAVEL record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.TravelPrefix + record.UNITID, _context);
            return record;
        }

        private void InsertValidate(TTRAVEL record)
        {
            this.Validate(record);
            var TTRAVEL = _context.TTRAVEL.Where(a => a.STARTDATE.Date <= record.STARTDATE.Date && a.ENDDATE.Date >= record.STARTDATE.Date && a.EMPLOYEEID == record.EMPLOYEEID 
                && a.STATUS != PMSConstants.TransactionStatusCanceled).ToList();
            if(TTRAVEL.Count() > 0)
                throw new Exception("Karyawan pada tanggal Perjalanan Dinas sudah ada");
            TTRAVEL = _context.TTRAVEL.Where(a => a.STARTDATE.Date <= record.ENDDATE.Date && a.ENDDATE.Date >= record.ENDDATE.Date && a.EMPLOYEEID == record.EMPLOYEEID
                && a.STATUS != PMSConstants.TransactionStatusCanceled).ToList();
            if (TTRAVEL.Count() > 0)
                throw new Exception("Karyawan pada tanggal Perjalanan Dinas sudah ada");
        }

        private void Validate(TTRAVEL record)
        {
            string result = this.FieldValidation(record);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(record.UNITID, record.STARTDATE.Date);
            _servicePeriod.CheckValidPeriod(record.UNITID, record.ENDDATE.Date);
        }

        private string FieldValidation(TTRAVEL record)
        {
            
            string result = string.Empty;
            //if (string.IsNullOrEmpty(record.IDTRAVEL)) result += "ID harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.UNITID)) result += "Bisnis Area harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Divisi harus diisi." + Environment.NewLine;
            if (record.STARTDATE == new DateTime()) result += "Tanggal Mulai Perjalanan Dinas harus diisi." + Environment.NewLine;
            if (record.ENDDATE == new DateTime()) result += "Tanggal Akhir Perjalanan Dinas harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.EMPLOYEEID)) result += "Karyawan harus diisi." + Environment.NewLine;
            if (record.STATUS == string.Empty) result += "Status harus diisi." + Environment.NewLine;
            if(DateTime.Compare(record.STARTDATE, record.ENDDATE) > 0) result += "Tanggal Akhir Perjalanan Dinas lebih dahulu dari tanggal awal." + Environment.NewLine;

            return result;
        }

        

        private void UpdateValidate(TTRAVEL record)
        {
            this.Validate(record);
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
        }

        private void DeleteValidate(TTRAVEL record)
        {
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(record.UNITID, record.STARTDATE.Date);
        }

        protected override TTRAVEL BeforeDelete(TTRAVEL record, string userName)
        {
            this.DeleteValidate(record);

            return record;
        }

        private void ApproveValidate(TTRAVEL record)
        {
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");

            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
            this.Validate(record);

            //var chekcAttendance = HelperService.GetConfigValue(PMSConstants.CFG_UpkeepAttendanceCheck + record.UNITID, _context) == PMSConstants.CFG_UpkeepAttendanceCheckTrue;

            //if (chekcAttendance)
            //{
            //    foreach (DateTime day in EachDay(record.STARTDATE, record.ENDDATE))
            //    {
            //        var rfidType = string.Empty;  // ??????

            //        var isExist = _serviceAttName.CheckAttendance(record.UNITID, record.EMPLOYEEID, day, "K", rfidType);
            //        if (isExist == 0)
            //            throw new Exception("Absensi karyawan " + record.EMPLOYEEID + " tanggal " + day.ToString("dd/MM/yyyy") + " tidak valid.");
            //    }
            //}
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string id = formDataCollection["IDTRAVEL"];
            return Approve(id, userName);
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
                yield return day;
        }

        public bool Approve(string id, string userName)
        {
            var record = GetSingle(id);
            this.ApproveValidate(record);

            record.STATUS = PMSConstants.TransactionStatusApproved;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();

            var divisi = new MDIVISI();
            divisi.CopyFrom(_serviceDivisiName.GetSingle(record.DIVID));
            
            foreach (DateTime day in EachDay(record.STARTDATE, record.ENDDATE))
            {
                var attendance = new TATTENDANCE
                {
                    DIVID = record.DIVID,
                    EMPLOYEEID = record.EMPLOYEEID,
                    EMPLOYEE = record.EMPLOYEE,
                    DIV = divisi,
                    DATE = day,
                    PRESENT = true,
                    REMARK = string.Empty,
                    HK = 1 ,
                    //AbsentId = "K",//-*Constant atau Enum
                    STATUS = record.STATUS,
                    REF = record.IDTRAVEL,
                    AUTO = true,
                    CREATEBY = record.UPDATEBY,
                    CREATEDDATE = record.UPDATED,
                    UPDATEBY = record.UPDATEBY,
                    UPDATEDDATE = record.UPDATED,
                };
                _serviceAttName.SaveInsert(attendance, userName);
            }


            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            SaveAuditTrail(record, userName, "Approve Record");
            return true;
        }

        public bool CancelApprove(IFormCollection formDataCollection, string userName)
        {
            string id = formDataCollection["IDTRAVEL"];
            //string canceledcomment = formDataCollection["CANCELEDCOMMENT"];
            var record = GetSingle(id);
            this.CancelValidate(record);

            record.STATUS = PMSConstants.TransactionStatusCanceled;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();

            _serviceAttName.DeleteByReferences(record.IDTRAVEL);

            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            SaveAuditTrail(record, userName, "Cancel Record");
            return true;
        }

        private void CancelValidate(TTRAVEL record)
        {
            if (record.STATUS != PMSConstants.TransactionStatusApproved)
                throw new Exception("Data belum di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(record.UNITID, record.STARTDATE.Date);
            _servicePeriod.CheckValidPeriod(record.UNITID, record.ENDDATE.Date);
        }

        public override IEnumerable<TTRAVEL> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TTRAVEL>();
            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                bool allUnit = true;
                List<string> authorizedUnitIds = new List<string>();

                //Authorization check
                //Get Authorization By User Name

                authorizedUnitIds = _authenticationService.GetAuthorizedLocationIdByUserName(filter.UserName, out allUnit);
                if (!allUnit)
                    criteria = criteria.And(p => authorizedUnitIds.Contains(p.UNITID));

            }
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(d => d.UNITID.Equals(filter.UnitID));

            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(d => d.DIVID.Equals(filter.DivisionID));

            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));

            if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                criteria = criteria.And(p => p.STARTDATE.Date >= filter.StartDate.Date && p.STARTDATE.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.IDTRAVEL.Contains(filter.Keyword)
                        || d.UNITID.Contains(filter.Keyword)
                        || d.DIVID.Contains(filter.Keyword)
                        || d.EMPLOYEEID.Contains(filter.Keyword)
                        //|| d.EMPLOYEE.EMPNAME.Contains(filter.Keyword)
                        //|| d.MANDOR.EMPNAME.Contains(filter.Keyword)
                    );

            if (filter.PageSize <= 0)
                return _context.TTRAVEL.Include(d => d.EMPLOYEE).Include(b=>b.DIV).Include(c=>c.UNIT)
                    .Where(criteria);

            return _context.TTRAVEL.Include(d => d.EMPLOYEE).Include(b => b.DIV).Include(c => c.UNIT)
                    .Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public override TTRAVEL NewRecord(string userName)
        {
            var record = new TTRAVEL();
            record.STARTDATE = GetServerTime();
            record.ENDDATE = GetServerTime();
            return record;
        }


    }
}
