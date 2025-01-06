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
    public class Speksi : EntityFactory<TSPEKSI, TSPEKSI, GeneralFilter, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;

        private AuthenticationServiceBase _authenticationService;
        public Speksi(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Speksi";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context, _authenticationService, auditContext);
            _serviceDivisiName = new Divisi(context, _authenticationService, auditContext);
        }

        public override TSPEKSI CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TSPEKSI record = base.CopyFromWebFormData(formData, newRecord);

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

        protected override TSPEKSI GetSingleFromDB(params object[] keyValues)
        {
            TSPEKSI record = _context.TSPEKSI.Include(b=>b.UNIT).Include(c=>c.DIV).Include(c=>c.BLOCK).Include(d=>d.EMPLOYEE).Include(e=>e.MANDOR)
                .Where(a => a.ID.Equals(keyValues[0])).FirstOrDefault();

            return record;
            //return base.GetSingleFromDB(keyValues);
        }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.SpeksiPrefix + unitCode, _context);
            return PMSConstants.SpeksiPrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override TSPEKSI BeforeSave(TSPEKSI record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                record.STATUS = PMSConstants.TransactionStatusProcess;
                this.InsertValidate(record);
                record.ID = GenereteNewCode(record.UNITID, record.TGLSPEKSI);
            }
            else
                this.UpdateValidate(record);
            record.UPDATED = GetServerTime();
            record.UPDATEBY = userName;

            return record;
        }

        protected override TSPEKSI AfterSave(TSPEKSI record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.SpeksiPrefix + record.UNITID, _context);
            return record;
        }

        private void InsertValidate(TSPEKSI record)
        {
            this.Validate(record);
            var tspeksi = _context.TSPEKSI.Where(a => a.TGLSPEKSI.Date == record.TGLSPEKSI.Date && a.EMPLOYEEID == record.EMPLOYEEID 
                && a.STATUS != PMSConstants.TransactionStatusCanceled).ToList();
            if(tspeksi.Count() > 0)
                throw new Exception("Karyawan pada tanggal Speksi sudah ada");
        }

        private void Validate(TSPEKSI record)
        {
            string result = this.FieldValidation(record);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(record.UNITID, record.TGLSPEKSI.Date);
        }

        private string FieldValidation(TSPEKSI record)
        {
            
            string result = string.Empty;
            //if (string.IsNullOrEmpty(record.ID)) result += "ID harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.UNITID)) result += "Bisnis Area harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Divisi harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.BLOCKID)) result += "Block harus diisi." + Environment.NewLine;
            if (record.TGLSPEKSI == new DateTime()) result += "Tanggal Speksi harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.EMPLOYEEID)) result += "Karyawan harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.MANDORID)) result += "Mandor harus diisi." + Environment.NewLine;
            if (record.TGLPANEN == new DateTime()) result += "Tanggal Panen harus diisi." + Environment.NewLine;
            if (record.STATUS == string.Empty) result += "Status harus diisi." + Environment.NewLine;
            if (record.LUASSPEKSI <= 0) result += "Luas speksi tidak boleh nol." + Environment.NewLine;
            if (record.TBSTDKPANEN <= 0 && record.TBSTDKBAWA <= 0 && record.BRDTDKKUTIP <= 0 && record.BRDTDKBERSIH <= 0
                && record.BUNGAMTH <= 0 && record.PELEPAHTDKMEPET <= 0 && record.PELEPAHTDKSESUAI <= 0 && record.PELEPAHTDKSUSUN <= 0)
                result += "Speksi buah tidak boleh nol semua." + Environment.NewLine;

            return result;
        }

        

        private void UpdateValidate(TSPEKSI record)
        {
            this.Validate(record);
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
        }

        private void DeleteValidate(TSPEKSI record)
        {
            if (record.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(record.UNITID, record.TGLSPEKSI.Date);
        }

        protected override TSPEKSI BeforeDelete(TSPEKSI record, string userName)
        {
            this.DeleteValidate(record);

            return record;
        }

        private void ApproveValidate(TSPEKSI record)
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

        private void CancelValidate(TSPEKSI record)
        {
            if (record.STATUS != PMSConstants.TransactionStatusApproved)
                throw new Exception("Data belum di approve.");
            if (record.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(record.UNITID, record.TGLSPEKSI.Date);
        }

        public override IEnumerable<TSPEKSI> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TSPEKSI>();
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
                        || d.BLOCKID.Contains(filter.Keyword)
                        || d.EMPLOYEEID.Contains(filter.Keyword)
                        //|| d.EMPLOYEE.EMPNAME.Contains(filter.Keyword)
                        || d.MANDORID.Contains(filter.Keyword)
                        //|| d.MANDOR.EMPNAME.Contains(filter.Keyword)
                    );

            if (filter.PageSize <= 0)
                return _context.TSPEKSI.Include(d => d.EMPLOYEE).Include(a=>a.MANDOR).Include(b=>b.DIV).Include(c=>c.UNIT).Include(d=>d.BLOCK)
                    .Where(criteria);

            return _context.TSPEKSI.Include(d => d.EMPLOYEE).Include(a => a.MANDOR).Include(b => b.DIV).Include(c => c.UNIT).Include(d => d.BLOCK)
                    .Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public override TSPEKSI NewRecord(string userName)
        {
            var record = new TSPEKSI();
            record.TGLSPEKSI = GetServerTime();
            record.TGLPANEN = GetServerTime();
            return record;
        }


    }
}
