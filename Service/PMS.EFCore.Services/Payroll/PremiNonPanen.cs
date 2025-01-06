using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services.Payroll;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Organization;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class PremiNonPanen : EntityFactory<TPREMINONPANEN,TPREMINONPANEN,FilterSalary, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;
        private Employee _serviceEmp;
        private AuthenticationServiceBase _authenticationService;
        private VDIVISI _divisi;
        public PremiNonPanen(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PremiNonPanen";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context,_authenticationService,auditContext);
            _serviceDivisiName = new Divisi(context,_authenticationService,auditContext);
            _serviceEmp = new Employee(context,_authenticationService,_auditContext);
        }

        private void ApproveValidate(TPREMINONPANEN premiNonPanen)
        {
            if (premiNonPanen.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            //WFDocument wfDocument = PMSServices.Document.WFGet(PMSServices.Document.GetByPMSCode(PMSServices.DocumentPremiNonPanen.GetByPremiNo(premiNonPanen.No).Code).DocTransNo + "", PMSConstants.PremiNonPanenCodePrefix, premiNonPanen.Division.UnitCode, premiNonPanen.DivisionId, premiNonPanen.Date, premiNonPanen.Date, "*");
            //if (wfDocument.DocStatus != "9999")
            //    if (string.IsNullOrEmpty(wfDocument.NextActivityName))
            //        throw new Exception("Dokumen sudah dalam keadaan " + wfDocument.DocStatusText);
            //    else
            //        throw new Exception("Dokumen masih dalam keadaan " + wfDocument.NextActivityName);
            this.Validate(premiNonPanen);
        }

        private void CancelValidate(TPREMINONPANEN premiNonPanen)
        {
            if (premiNonPanen.STATUS != PMSConstants.TransactionStatusApproved)
                throw new Exception("Data belum di approve.");
            _servicePeriod.CheckValidPeriod(premiNonPanen.DIV.UNITCODE, premiNonPanen.PREMIDATE);
        }

        private void DeleteValidate(TPREMINONPANEN premiNonPanen)
        {
            if (premiNonPanen.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (premiNonPanen.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
            _servicePeriod.CheckValidPeriod(premiNonPanen.DIV.UNITCODE, premiNonPanen.PREMIDATE);
        }

        private string FieldsValidation(TPREMINONPANEN premiNonPanen)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(premiNonPanen.DOCNO)) result += "No tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(premiNonPanen.DIVID)) result += "Divisi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(premiNonPanen.EMPLOYEEID)) result += "Karyawan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(premiNonPanen.ACTIVITYID)) result += "Kegiatan tidak boleh kosong." + Environment.NewLine;
            if (premiNonPanen.PREMIDATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(premiNonPanen.REMARK)) result += "Keterangan tidak boleh kosong." + Environment.NewLine;
            if (premiNonPanen.STATUS == PMSConstants.TransactionStatusNone) result += "Status tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        protected override TPREMINONPANEN GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.TPREMINONPANEN.Include(a => a.DIV).SingleOrDefault(d => d.DOCNO.Equals(id));
        }

        private string GenereteNewNumber(string unitCode, DateTime datetime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.PremiNonPanenCodePrefix + unitCode, _context);
            return PMSConstants.PremiNonPanenCodePrefix + "-" + datetime.ToString("yyyyMMdd") + "-"
                + unitCode + "-" + lastNumber.ToString().PadLeft(4, '0');
        }

        private void InsertValidate(TPREMINONPANEN premiNonPanen)
        {
            this.Validate(premiNonPanen);
            var pnpExist = GetSingle(premiNonPanen.DOCNO);
            if (pnpExist != null) throw new Exception("Premi dengan no " + premiNonPanen.DOCNO + " sudah terdaftar.");
        }

        protected override TPREMINONPANEN BeforeSave(TPREMINONPANEN record, string userName, bool newRecord)
        {
            _divisi = _serviceDivisiName.GetSingle(record.DIVID);
            if (newRecord)
            {
                record.DOCNO = this.GenereteNewNumber(_divisi.UNITCODE, record.PREMIDATE);
                record.CREATEBY = userName;
                record.CREATED = GetServerTime();
                record.STATUS = PMSConstants.TransactionStatusProcess;
                this.InsertValidate(record);
            }
            else
                this.UpdateValidate(record);
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            return record;
        }

        protected override TPREMINONPANEN AfterSave(TPREMINONPANEN premiNonPanen, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.PremiNonPanenCodePrefix + _divisi.UNITCODE, _context);

            return premiNonPanen;
        }

        private void Validate(TPREMINONPANEN premiNonPanen)
        {
            string result = this.FieldsValidation(premiNonPanen);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            if (premiNonPanen.PREMIHK == 0 && premiNonPanen.PREMIAMOUNT == 0)
                throw new Exception("Bonus atau Premi tidak boleh nol.");

            if (premiNonPanen.PREMIHK != 0 && premiNonPanen.PREMIAMOUNT != 0)
                throw new Exception("Isi bonus atau premi saja.");

            _servicePeriod.CheckValidPeriod(_divisi.UNITCODE, premiNonPanen.PREMIDATE);
            _servicePeriod.CheckMaxPeriod(_divisi.UNITCODE, premiNonPanen.PREMIDATE);

            var fil = new FilterSalary();
            fil.EMPID = premiNonPanen.EMPLOYEEID;
            fil.Date = premiNonPanen.PREMIDATE;
            var getData = this.GetList(fil);
            var pnp = getData.Where(a => a.DOCNO != premiNonPanen.DOCNO).ToList();
            //pnp.RemoveAll(p => p.No == premiNonPanen.DOCNO);
            if (pnp.Count > 0)
                throw new Exception("Tanggal untuk karyawan tersebut sudah ada.");
        }

        private void UpdateValidate(TPREMINONPANEN premiNonPanen)
        {
            if (premiNonPanen.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (premiNonPanen.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            this.Validate(premiNonPanen);
        }

        

        protected override TPREMINONPANEN BeforeDelete(TPREMINONPANEN record, string userName)
        {
            if (record.DIV == null)
            {
                var divisi = new MDIVISI();
                divisi.CopyFrom(_serviceDivisiName.GetSingle(record.DIVID));
                record.DIV = divisi;
            }
            this.DeleteValidate(record);
            record.DIV = null;
            record.EMPLOYEE = null;
            return record;
        }

        public void Approve(string no, string by)
        {
            try
            {
                var premi = GetSingle(no);
                if (premi.DIV == null)
                {
                    var divisi = new MDIVISI();
                    divisi.CopyFrom(_serviceDivisiName.GetSingle(premi.DIVID));
                    premi.DIV = divisi;
                    premi.EMPLOYEE = _serviceEmp.GetSingle(premi.EMPLOYEEID);
                }
                this.ApproveValidate(premi);

                premi.UPDATEBY = by;
                premi.UPDATED = GetServerTime();
                premi.STATUS = PMSConstants.TransactionStatusApproved;
                this.SaveUpdate(premi, by);

                //Document wfDocument = PMSServices.Document.WFGet(PMSServices.Document.GetByPMSCode(PMSServices.DocumentPremiNonPanen.GetByPremiNo(premi.No).Code).DocTransNo + "", PMSConstants.PremiNonPanenCodePrefix, premi.Division.UnitCode, premi.DivisionId, premi.Date, premi.Date, "*");
                //DocumentPremiNonPanen wfDocumentPremiNonPanen = new DocumentPremiNonPanen(premi.No, premi.DivisionId, premi.EmployeeId, premi.Employee.Name, premi.Date, premi.Amount, premi.Remark, "Approved");
                //wfDocument.DocumentPremiNonPanen = wfDocumentPremiNonPanen;
                //PMSServices.Document.WFUpdate(wfDocument, true);
            }
            catch
            {
                throw;
            }
        }

        public void Cancel(string no, string by)
        {
            try
            {
                var premi = GetSingle(no);
                if (premi.DIV == null)
                {
                    var divisi = new MDIVISI();
                    divisi.CopyFrom(_serviceDivisiName.GetSingle(premi.DIVID));
                    premi.DIV = divisi;
                    premi.EMPLOYEE = _serviceEmp.GetSingle(premi.EMPLOYEEID);
                }
                this.CancelValidate(premi);

                premi.UPDATEBY = by;
                premi.UPDATED = GetServerTime();
                premi.STATUS = PMSConstants.TransactionStatusCanceled;
                this.SaveUpdate(premi, by);

                //Document wfDocument = PMSServices.Document.WFGet(PMSServices.Document.GetByPMSCode(PMSServices.DocumentPremiNonPanen.GetByPremiNo(premi.No).Code).DocTransNo + "", PMSConstants.PremiNonPanenCodePrefix, premi.Division.UnitCode, premi.DivisionId, premi.Date, premi.Date, "*");
                //DocumentPremiNonPanen wfDocumentPremiNonPanen = new DocumentPremiNonPanen(premi.No, premi.DivisionId, premi.EmployeeId, premi.Employee.Name, premi.Date, premi.Amount, premi.Remark, "Cancelled");
                //wfDocument.DocumentPremiNonPanen = wfDocumentPremiNonPanen;
                //PMSServices.Document.WFUpdate(wfDocument, true);
            }
            catch
            {
                throw;
            }
        }

        public override IEnumerable<TPREMINONPANEN> GetList(FilterSalary filter)
        {
            
            var criteria = PredicateBuilder.True<TPREMINONPANEN>();
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
           
            if (!string.IsNullOrEmpty(filter.DOCNO))
                criteria = criteria.And(d => d.PAYMENTNO == filter.DOCNO);

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.PAYMENTNO.Equals(filter.Id));

            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains(p.PAYMENTNO));

            if (!string.IsNullOrEmpty(filter.EMPID))
                criteria = criteria.And(d => d.EMPLOYEEID == filter.EMPID);

            if (!string.IsNullOrEmpty(filter.STATUS))
                criteria = criteria.And(d => d.STATUS == filter.STATUS);

            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(p => p.DIVID.Equals(filter.DivisionID));
            if (filter.DivisionIDs.Any())
                criteria = criteria.And(d => filter.DivisionIDs.Contains(d.DIVID));

            if (filter.StartDate.Date != dateNull || filter.EndDate.Date != dateNull)
                criteria = criteria.And(d => d.PREMIDATE.Date >= filter.StartDate.Date && d.PREMIDATE.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.PAYMENTNO.Contains(filter.Keyword) || d.EMPLOYEEID.Contains(filter.Keyword));

            var result =
                from a in _context.TPREMINONPANEN.Where(criteria)
                join b in _context.MEMPLOYEE on a.EMPLOYEEID equals b.EMPID
                select new TPREMINONPANEN(a) { EMPNAME = b.EMPNAME };

            if (filter.PageSize <= 0)
                return result;
            return result.GetPaged(filter.PageNo, filter.PageSize).Results;

           

            
        }

        public override TPREMINONPANEN NewRecord(string userName)
        {
            return new TPREMINONPANEN
            {
                STATUS = PMSConstants.TransactionStatusApproved,
                PREMIDATE = DateTime.Today,
                PREMIAMOUNT = 0,
                PREMIHK = 0
            };
        }

    }
}
