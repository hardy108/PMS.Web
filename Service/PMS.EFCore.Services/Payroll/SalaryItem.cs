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
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class SalaryItem : EntityFactory<TSALARYITEM,TSALARYITEM,FilterSalary, PMSContextBase>
    {
        private Period _servicePeriod;
        //private Payment _servicePayment;
        private AuthenticationServiceBase _authenticationService;
        public SalaryItem(PMSContextBase context, AuthenticationServiceBase authenticationService,AuditContext auditContext) : base(context,auditContext)
        {
            _serviceName = "SalaryItem";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context,_authenticationService,auditContext);
            //_servicePayment = new Payment(context);
        }

        private void DeleteValidate(TSALARYITEM salaryItem)
        {
            if (salaryItem.AUTO) throw new Exception("Transaksi tidak boleh dihapus manual.");
            if (salaryItem.STATUS == PMSConstants.TransactionStatusDeleted) throw new Exception("Transaksi sudah dihapus.");
            _servicePeriod.CheckValidPeriod(salaryItem.UNITID, salaryItem.DATE);
        }

        private string FieldsValidation(TSALARYITEM salaryItem)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(salaryItem.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(salaryItem.UNITID)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(salaryItem.TYPEID)) result += "Jenis premi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(salaryItem.EMPID)) result += "Pegawai tidak boleh kosong." + Environment.NewLine;
            if (salaryItem.AMOUNT == 0) result += "Jumlah tidak boleh kosong." + Environment.NewLine;
            if (salaryItem.STATUS == PMSConstants.TransactionStatusNone) result += "Status tidak boleh kosong." + Environment.NewLine;
            
            return result;
        }

        protected override TSALARYITEM GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.TSALARYITEM.SingleOrDefault(d => d.ID.Equals(id));
        }

        private void InsertValidate(TSALARYITEM salaryItem)
        {
            this.Validate(salaryItem);
            var premiExist = this.GetSingle(salaryItem.ID);
            if (premiExist != null) throw new Exception("Id " + salaryItem.ID + " sudah terdaftar.");
        }

        protected override TSALARYITEM BeforeSave(TSALARYITEM salaryItem, string userName, bool newRecord)
        {
            DateTime now = this.GetServerTime();
            salaryItem.UPDATED = now;
            if (newRecord)
            {
                int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.SalaryItemIdPrefix + salaryItem.UNITID, _context);
                salaryItem.ID = PMSConstants.SalaryItemIdPrefix + salaryItem.UNITID + salaryItem.UPDATED.ToString("yyyyMMdd") + lastNumber.ToString().PadLeft(6, '0');

                this.InsertValidate(salaryItem);
            }
            else
                UpdateValidate(salaryItem);

            return salaryItem;
        }

        protected override TSALARYITEM AfterSave(TSALARYITEM salaryItem, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.SalaryItemIdPrefix + salaryItem.UNITID, _context);

            return salaryItem;
        }

        private void Validate(TSALARYITEM salaryItem)
        {
            string result = this.FieldsValidation(salaryItem);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(salaryItem.UNITID, salaryItem.DATE);
            if (!salaryItem.AUTO)
                _servicePeriod.CheckMaxPeriod(salaryItem.UNITID, salaryItem.DATE);
        }

        private void UpdateValidate(TSALARYITEM salaryItem)
        {
            if (salaryItem.STATUS == PMSConstants.TransactionStatusDeleted)
                throw new Exception("Data sudah di hapus.");

            this.Validate(salaryItem);

            if (!string.IsNullOrEmpty(salaryItem.PAYMENTNO))
            {
                //var payment = _servicePayment.GetSingle(salaryItem.PAYMENTNO);
                var payment = _context.TPAYMENT.Where(a => a.DOCNO == salaryItem.PAYMENTNO).SingleOrDefault();
                if (payment.STATUS == PMSConstants.TransactionStatusApproved) throw new Exception("Payroll sudah di approve.");
            }
        }

       
        protected override TSALARYITEM BeforeDelete(TSALARYITEM record, string userName)
        {
            this.DeleteValidate(record);

            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPDATED = this.GetServerTime();

            return record;
        }

        protected override bool DeleteFromDB(TSALARYITEM record, string userName)
        {
            try
            {
                _context.TSALARYITEM.Update(record);
                _context.SaveChanges();
                return true;
            }
            catch
            { return false; }
            
        }

        public void DeleteByPaymentNo(string paymentNo, DateTime date)
        {
            _context.TSALARYITEM.RemoveRange(_context.TSALARYITEM.Where(d => d.PAYMENTNO.Equals(paymentNo) && d.AUTO==true));
            _context.TSALARYITEM.Where(d => d.PAYMENTNO.Equals(paymentNo) && d.AUTO==false).ToList()
               .ForEach(d => d.PAYMENTNO = null);
            _context.SaveChanges();
        }

        public void UpdatePaymentNo(string unitCode, DateTime startDate, DateTime endDate, string paymentNo, string userName)
        {
            var fil = new FilterSalary();
            fil.UnitID = unitCode;
            fil.AUTO = false;
            fil.STATUS = PMSConstants.TransactionStatusApproved;
            fil.StartDate = startDate;
            fil.EndDate = endDate;
            var salaryItem = this.GetList(fil);
            foreach(var item in salaryItem)
            {
                item.PAYMENTNO = paymentNo;
                this.SaveUpdate(item, userName);
            }
        }

        public override IEnumerable<TSALARYITEM> GetList(FilterSalary filter)
        {
            
            var criteria = PredicateBuilder.True<TSALARYITEM>();
            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date == dateNull || filter.EndDate.Date == dateNull)
                throw new Exception("Tanggal harus diisi");
            if (filter.AUTO.HasValue)
                criteria = criteria.And(d => d.AUTO == filter.AUTO.Value);

            if (!string.IsNullOrEmpty(filter.Id))
                criteria = criteria.And(d => d.ID == filter.Id);
            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains(p.ID));

            if (!string.IsNullOrEmpty(filter.DOCNO))
                criteria = criteria.And(d => d.PAYMENTNO == filter.DOCNO);
            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains(p.PAYMENTNO));

            if (!string.IsNullOrEmpty(filter.UnitID))
                criteria = criteria.And(d => d.UNITID == filter.UnitID);
            if (filter.UnitIDs.Any())
                criteria = criteria.And(p => filter.UnitIDs.Contains(p.UNITID));

            if (!string.IsNullOrEmpty(filter.EMPID))
                criteria = criteria.And(d => d.EMPID == filter.EMPID);

            if (!string.IsNullOrEmpty(filter.TYPE))
                criteria = criteria.And(d => d.TYPEID == filter.TYPE);

            if (!string.IsNullOrEmpty(filter.STATUS))
                criteria = criteria.And(d => d.STATUS == filter.STATUS);
                
            if (filter.StartDate.Date != dateNull || filter.EndDate.Date != dateNull)
                criteria = criteria.And(d => d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.PAYMENTNO.Contains(filter.Keyword) || d.ID.Contains(filter.Keyword));

            var result =
                from a in _context.TSALARYITEM.Where(criteria)
                join b in _context.MEMPLOYEE on a.EMPID equals b.EMPID
                select new TSALARYITEM(a) { EMPNAME = b.EMPNAME  };

            if (filter.PageSize <= 0)
                return result;
            return result.GetPaged(filter.PageNo, filter.PageSize).Results;

            
           
        }

        public override TSALARYITEM NewRecord(string userName)
        {
            TSALARYITEM record = new TSALARYITEM { DATE = DateTime.Today, STATUS = "A" };
            return record;
        }

    }
}
