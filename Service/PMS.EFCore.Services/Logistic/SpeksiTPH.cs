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
    public class SpeksiTPH : EntityFactory<TSPEKSITPH, TSPEKSITPH, GeneralFilter, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;

        private AuthenticationServiceBase _authenticationService;
        public SpeksiTPH(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "SpeksiTPH";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context, _authenticationService, auditContext);
            _serviceDivisiName = new Divisi(context, _authenticationService, auditContext);
        }

        public override TSPEKSITPH CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TSPEKSITPH record = base.CopyFromWebFormData(formData, newRecord);

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

        protected override TSPEKSITPH GetSingleFromDB(params object[] keyValues)
        {
            TSPEKSITPH record = _context.TSPEKSITPH.Include(b=>b.UNIT).Include(c=>c.DIV)
                .Where(a => a.ID.Equals(keyValues[0])).SingleOrDefault();

            return record;
            //return base.GetSingleFromDB(keyValues);
        }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.SpeksiTPHPrefix + unitCode, _context);
            return PMSConstants.SpeksiTPHPrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override TSPEKSITPH BeforeSave(TSPEKSITPH record, string userName,bool newRecord)
        {
            if (newRecord)
            {
                record.STATUS = PMSConstants.TransactionStatusProcess;
                this.InsertValidate(record);
                record.ID = GenereteNewCode(record.UNITID, record.TGLSPEKSI);
            }
            else
                UpdateValidate(record);
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;

            return record;
        }

        protected override TSPEKSITPH AfterSave(TSPEKSITPH record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.SpeksiTPHPrefix + record.UNITID, _context);
            return record;
        }

        private void InsertValidate(TSPEKSITPH record)
        {
            this.Validate(record);
            var speksiTph = _context.TSPEKSITPH.Where(a => a.TGLSPEKSI.Date == record.TGLSPEKSI.Date && a.DIVID == record.DIVID
                && a.STATUS != PMSConstants.TransactionStatusCanceled).ToList();
            if(speksiTph.Count() > 0)
                throw new Exception("Divisi pada tanggal Speksi sudah ada");
        }

        private void Validate(TSPEKSITPH record)
        {
            string result = this.FieldValidation(record);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(record.UNITID, record.TGLSPEKSI.Date);
        }

        private string FieldValidation(TSPEKSITPH record)
        {
            string result = string.Empty;
            //if (string.IsNullOrEmpty(record.ID)) result += "ID harus diisi." + Environment.NewLine;
            if (record.TGLSPEKSI == new DateTime()) result += "Tanggal Speksi harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.UNITID)) result += "Bisnis Area harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Divisi harus diisi." + Environment.NewLine;
            if (record.STATUS == string.Empty) result += "Status harus diisi." + Environment.NewLine;
            if (record.PRODMILLIN <= 0 && record.PRODMILLEX <= 0 && record.RESTAN <= 0 )
                result += "Speksi tidak boleh nol semua." + Environment.NewLine;

            return result;
        }

        

        private void UpdateValidate(TSPEKSITPH record)
        {
            this.Validate(record);
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
        }

        private void DeleteValidate(TSPEKSITPH record)
        {
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(record.UNITID, record.TGLSPEKSI.Date);
        }

        protected override TSPEKSITPH BeforeDelete(TSPEKSITPH record, string userName)
        {
            this.DeleteValidate(record);

            return record;
        }

        private void ApproveValidate(TSPEKSITPH record)
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

            record.STATUS = "A";
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
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

            record.STATUS = "C";
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        private void CancelValidate(TSPEKSITPH record)
        {
            if (record.STATUS != PMSConstants.TransactionStatusApproved)
                throw new Exception("Data belum di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(record.UNITID, record.TGLSPEKSI.Date);
        }

        public override IEnumerable<TSPEKSITPH> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TSPEKSITPH>();
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
                criteria = criteria.And(p => p.TGLSPEKSI.Date >= filter.StartDate.Date && p.TGLSPEKSI.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.ID.Contains(filter.Keyword)
                        || d.UNITID.Contains(filter.Keyword)
                        || d.DIVID.Contains(filter.Keyword)
                    );

            if (filter.PageSize <= 0)
                return _context.TSPEKSITPH.Include(b=>b.DIV).Include(c=>c.UNIT)
                    .Where(criteria);

            return _context.TSPEKSITPH.Include(b => b.DIV).Include(c => c.UNIT)
                    .Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public override TSPEKSITPH NewRecord(string userName)
        {
            var record = new TSPEKSITPH();
            record.TGLSPEKSI = GetServerTime();
            return record;
        }


    }
}
