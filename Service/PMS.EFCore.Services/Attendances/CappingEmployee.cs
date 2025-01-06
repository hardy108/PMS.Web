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
    public class CappingEmployee : EntityFactory<TCAPPINGEMP, TCAPPINGEMP, GeneralFilter, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;
        private Attendance _serviceAttName;

        private AuthenticationServiceBase _authenticationService;
        public CappingEmployee(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "CappingEmployee";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context, _authenticationService, auditContext);
            _serviceDivisiName = new Divisi(context, _authenticationService, auditContext);
            _serviceAttName = new Attendance(context, _authenticationService, auditContext);
        }

        public override TCAPPINGEMP CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TCAPPINGEMP record = base.CopyFromWebFormData(formData, newRecord);

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

        protected override TCAPPINGEMP GetSingleFromDB(params object[] keyValues)
        {
            TCAPPINGEMP record = _context.TCAPPINGEMP.Include(b=>b.UNIT).Include(c => c.DIV).Include(d=>d.EMPLOYEE)
                .Where(a => a.ID.Equals(keyValues[0])).SingleOrDefault();

            return record;
            //return base.GetSingleFromDB(keyValues);
        }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.TravelPrefix + unitCode, _context);
            return PMSConstants.TravelPrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override TCAPPINGEMP BeforeSave(TCAPPINGEMP record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                record.STATUS = PMSConstants.TransactionStatusProcess;
                this.InsertValidate(record);
                record.ID = GenereteNewCode(record.UNITID, record.DATE);
            }
            else
                this.UpdateValidate(record);
            
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;
            return record;
        }

        protected override TCAPPINGEMP AfterSave(TCAPPINGEMP record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.TravelPrefix + record.UNITID, _context);
            return record;
        }

        private void InsertValidate(TCAPPINGEMP record)
        {
            this.Validate(record);
            var TCAPPINGEMP = _context.TCAPPINGEMP.Where(a => a.DATE.Date <= record.DATE.Date && a.EMPLOYEEID == record.EMPLOYEEID 
                && a.STATUS != PMSConstants.TransactionStatusCanceled).ToList();
            if(TCAPPINGEMP.Count() > 0)
                throw new Exception("Karyawan pada tanggal tersebut sudah ada");
            
        }

        private void Validate(TCAPPINGEMP record)
        {
            string result = this.FieldValidation(record);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(record.UNITID, record.DATE.Date);
        }

        private string FieldValidation(TCAPPINGEMP record)
        {
            
            string result = string.Empty;
            //if (string.IsNullOrEmpty(record.ID)) result += "ID harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.UNITID)) result += "Bisnis Area harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Divisi harus diisi." + Environment.NewLine;
            if (record.DATE == new DateTime()) result += "Tanggal harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.EMPLOYEEID)) result += "Karyawan harus diisi." + Environment.NewLine;
            if (record.STATUS == string.Empty) result += "Status harus diisi." + Environment.NewLine;
            
            return result;
        }

      

        private void UpdateValidate(TCAPPINGEMP record)
        {
            this.Validate(record);
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
        }

        private void DeleteValidate(TCAPPINGEMP record)
        {
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(record.UNITID, record.DATE.Date);
        }

        protected override TCAPPINGEMP BeforeDelete(TCAPPINGEMP record, string userName)
        {
            this.DeleteValidate(record);

            return record;
        }

        private void ApproveValidate(TCAPPINGEMP record)
        {
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            this.Validate(record);

            //var chekcAttendance = HelperService.GetConfigValue(PMSConstants.CFG_UpkeepAttendanceCheck + record.UNITID, _context) == PMSConstants.CFG_UpkeepAttendanceCheckTrue;

            //if (chekcAttendance)
            //{
            //    foreach (DateTime day in EachDay(record.DATE, record.ENDDATE))
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
            string id = formDataCollection["ID"];
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

            //var divisi = new MDIVISI();
            //divisi.CopyFrom(_serviceDivisiName.GetSingle(record.DIVID));
            
            //foreach (DateTime day in EachDay(record.DATE, record.ENDDATE))
            //{
            //    var attendance = new TATTENDANCE
            //    {
            //        DIVID = record.DIVID,
            //        EMPLOYEEID = record.EMPLOYEEID,
            //        EMPLOYEE = record.EMPLOYEE,
            //        DIV = divisi,
            //        DATE = day,
            //        PRESENT = true,
            //        REMARK = string.Empty,
            //        HK = 1 ,
            //        //AbsentId = "K",//-*Constant atau Enum
            //        STATUS = record.STATUS,
            //        REF = record.ID,
            //        AUTO = true,
            //        CREATEBY = record.UPDATEBY,
            //        CREATEDDATE = record.UPDATED,
            //        UPDATEBY = record.UPDATEBY,
            //        UPDATEDDATE = record.UPDATED,
            //    };
            //    _serviceAttName.SaveInsert(attendance, userName);
            //}
            
            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            SaveAuditTrail(record, userName, "Approve Record");
            return true;
        }

        public bool CancelApprove(IFormCollection formDataCollection, string userName)
        {
            string id = formDataCollection["ID"];
            //string canceledcomment = formDataCollection["CANCELEDCOMMENT"];
            var record = GetSingle(id);
            this.CancelValidate(record);

            record.STATUS = PMSConstants.TransactionStatusCanceled;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();

            //_serviceAttName.DeleteByReferences(record.ID);

            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        private void CancelValidate(TCAPPINGEMP record)
        {
            if (record.STATUS != PMSConstants.TransactionStatusApproved)
                throw new Exception("Data belum di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(record.UNITID, record.DATE.Date);
        }

        public override IEnumerable<TCAPPINGEMP> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TCAPPINGEMP>();
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

            if (!filter.StartDate.Equals(DateTime.MinValue))
                criteria = criteria.And(p => p.DATE.Date >= filter.StartDate.Date );

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.ID.Contains(filter.Keyword)
                        || d.UNITID.Contains(filter.Keyword)
                        || d.DIVID.Contains(filter.Keyword)
                        || d.EMPLOYEEID.Contains(filter.Keyword)
                        //|| d.EMPLOYEE.EMPNAME.Contains(filter.Keyword)
                        //|| d.MANDOR.EMPNAME.Contains(filter.Keyword)
                    );

            if (filter.PageSize <= 0)
                return _context.TCAPPINGEMP.Include(d => d.EMPLOYEE).Include(b=>b.DIV).Include(c=>c.UNIT)
                    .Where(criteria);

            return _context.TCAPPINGEMP.Include(d => d.EMPLOYEE).Include(b => b.DIV).Include(c => c.UNIT)
                    .Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public override TCAPPINGEMP NewRecord(string userName)
        {
            var record = new TCAPPINGEMP();
            record.DATE = GetServerTime();
            return record;
        }


    }
}
