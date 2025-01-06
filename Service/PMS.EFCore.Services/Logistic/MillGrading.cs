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

namespace PMS.EFCore.Services.Logistic
{
    public class MillGrading : EntityFactory<TMILLGRADING, TMILLGRADING, GeneralFilter, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;

        private AuthenticationServiceBase _authenticationService;
        public MillGrading(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "MillGrading";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context, _authenticationService, auditContext);
            _serviceDivisiName = new Divisi(context, _authenticationService, auditContext);
        }

        public override TMILLGRADING CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TMILLGRADING record = base.CopyFromWebFormData(formData, newRecord);
            
            if(record.UNIT == null)
                record.UNIT = _context.MUNIT.Find(record.UNITID);

            if (record.DIV == null)
                record.DIV = _context.MDIVISI.Find(record.DIVID);

            return record;
        }

        protected override TMILLGRADING GetSingleFromDB(params object[] keyValues)
        {
            TMILLGRADING record = _context.TMILLGRADING.Include(b => b.UNIT).Include(c => c.DIV)
                .Where(a => a.ID.Equals(keyValues[0])).FirstOrDefault();

            return record;
            //return base.GetSingleFromDB(keyValues);
        }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.MillGradingPrefix + unitCode, _context);
            return PMSConstants.MillGradingPrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override TMILLGRADING BeforeSave(TMILLGRADING record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                record.ID = GenereteNewCode(record.UNITID, record.GRADINGDATE);
                record.STATUS = PMSConstants.TransactionStatusProcess;

                this.InsertValidate(record);
            }
            else
                this.UpdateValidate(record);
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;

            return record;
        }


        

        private void UpdateValidate(TMILLGRADING record)
        {
            this.Validate(record);
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
        }



        protected override TMILLGRADING AfterSave(TMILLGRADING record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.MillGradingPrefix + record.UNITID, _context);
            return record;
        }

        private void InsertValidate(TMILLGRADING record)
        {
            this.Validate(record);
            var tmillGrading = _context.TMILLGRADING.Where(a => a.GRADINGDATE.Date == record.GRADINGDATE.Date
                && a.UNITID == record.UNITID && a.DIVID == record.DIVID && a.STATUS != PMSConstants.TransactionStatusCanceled).ToList();
            if(tmillGrading.Count() > 0)
                throw new Exception("Mill Grading pada tanggal " + record.GRADINGDATE.ToString("dd-MM-yyyy") + " sudah ada");
        }

        private void Validate(TMILLGRADING record)
        {
            string result = this.FieldValidation(record);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(record.UNITID, record.GRADINGDATE.Date);
        }

        private string FieldValidation(TMILLGRADING record)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.ID)) throw new Exception("ID harus diisi.");
            if (string.IsNullOrEmpty(record.UNITID)) throw new Exception("Estate harus diisi.");
            if (string.IsNullOrEmpty(record.DIVID)) throw new Exception("Divisi harus diisi.");
            if (record.GRADINGDATE == new DateTime()) throw new Exception("Tanggal Grading harus diisi.");
            if (record.QTYPCT <= 0 || record.QTYPCT > 100) throw new Exception("Brondolan (%) harus di antara 0 dan 100");
            if (record.STATUS == string.Empty) throw new Exception("Status harus diisi.");
            
            return result;
        }

        

        protected override bool DeleteFromDB(TMILLGRADING record, string userName)
        {
            _servicePeriod.CheckValidPeriod(record.UNITID, record.GRADINGDATE.Date);

            record = GetSingle(record.ID);
            if (record == null)
                throw new Exception("Record not found");

            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPDATED = now;
            _context.Entry<TMILLGRADING>(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        private void ApproveValidate(TMILLGRADING record)
        {
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            this.Validate(record);
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string id = formDataCollection["ID"];
            return Approve(id, userName);
        }

        public bool Approve(string id, string userName)
        {
            var record = GetSingle(id);
            this.ApproveValidate(record);

            record.STATUS = PMSConstants.TransactionStatusApproved;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        public override IEnumerable<TMILLGRADING> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TMILLGRADING>();
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

            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));

            if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                criteria = criteria.And(p => p.GRADINGDATE.Date >= filter.StartDate.Date && p.GRADINGDATE.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.UNITID.Contains(filter.Keyword)
                        || d.ID.Contains(filter.Keyword)
                    );

            if (filter.PageSize <= 0)
                return _context.TMILLGRADING.Include(a => a.UNIT).Include(b => b.DIV)
                    .Where(criteria);

            return _context.TMILLGRADING.Include(a => a.UNIT).Include(b => b.DIV)
                    .Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public override TMILLGRADING NewRecord(string userName)
        {
            var record = new TMILLGRADING();
            record.GRADINGDATE = GetServerTime();
            return record;
        }
    }
}
