using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Logistic;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using PMS.EFCore.Services.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class PaymentFP:EntityFactory<TPAYMENTDETAILTRX, TPAYMENTDETAILTRX, FilterPaymentFP, PMSContextBase>
    {
        private Period _periodService;
        private AuthenticationServiceBase _authenticationService;
        public PaymentFP(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PaymentFP";
            _authenticationService = authenticationService;
            _periodService = new Period(context,_authenticationService,auditContext);
        }

        private string FieldsValidation(TPAYMENTDETAILTRX PaymentFP)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(PaymentFP.EMPID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(PaymentFP.DOCNO)) result += "Nomor tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(PaymentFP.UNITCODE)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (PaymentFP.DOCDATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (PaymentFP.PERIOD == 0) result += "Period tidak boleh kosong." + Environment.NewLine;
            if (PaymentFP.STATUS == string.Empty || PaymentFP.STATUS =="") result += "Status tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        private void Validate(TPAYMENTDETAILTRX paymentfp)
        {

            string result = this.FieldsValidation(paymentfp);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
            _periodService.CheckValidPeriod(paymentfp.UNITCODE, paymentfp.DOCDATE.Date);
        }

        private void InsertValidate(TPAYMENTDETAILTRX paymentfp)
        {
            this.Validate(paymentfp);

            var employeeFPExist = GetList(new FilterPaymentFP {DocNo= paymentfp.DOCNO, EmpId= paymentfp.EMPID });
            if (employeeFPExist != null)
                throw new Exception("Id sudah terdaftar.");
        }

        private void DeleteValidate(TPAYMENTDETAILTRX paymentfp)
        {
            if (paymentfp.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (paymentfp.STATUS == "C")
                throw new Exception("Data sudah di cancel.");
            _periodService.CheckValidPeriod(paymentfp.UNITCODE, paymentfp.DOCDATE.Date);
        }

        public override IEnumerable<TPAYMENTDETAILTRX> GetList(FilterPaymentFP filter)
        {
            
            var criteria = PredicateBuilder.True<TPAYMENTDETAILTRX>();

            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(p => p.UNITCODE.Equals(filter.DivisionID));

            if (!string.IsNullOrWhiteSpace(filter.DocNo))
                criteria = criteria.And(p => p.DOCNO.Equals(filter.DocNo));

            if (filter.EmpId != null)
                criteria = criteria.And(p => p.EMPID.Equals(filter.EmpId));

            if (filter.TypeManual != null)
                criteria = criteria.And(p => p.TYPEMANUAL.Equals(filter.TypeManual));

            if (!filter.Date.Equals(DateTime.MinValue))
                criteria = criteria.And(p => p.DOCDATE.Date == filter.Date.Date);

            if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                criteria = criteria.And(p => p.DOCDATE.Date >= filter.StartDate.Date && p.DOCDATE.Date <= filter.EndDate.Date);

            var result = _context.TPAYMENTDETAILTRX.Where(criteria);
            if (filter.PageSize <= 0)
                return result;
            return result.GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        protected override TPAYMENTDETAILTRX BeforeSave(TPAYMENTDETAILTRX record, string userName, bool newRecord)
        {
            record.UPDATED = GetServerTime();
            if (newRecord)
            {
                this.InsertValidate(record);
                record.STATUS = "A";
            }
            else
                Validate(record);
            return record; 
        }

       

        protected override TPAYMENTDETAILTRX GetSingleFromDB(TPAYMENTDETAILTRX record)
        {
            return _context.TPAYMENTDETAILTRX.Where(d=> d.DOCNO.Equals(record.DOCNO) && d.EMPID.Equals(record.EMPID)).SingleOrDefault()  ;
        }

        protected override bool DeleteFromDB(TPAYMENTDETAILTRX record, string userName)
        {
            this.DeleteValidate(record);
            record = GetSingle(record);
            record.STATUS = "D";
            record.UPDATED = GetServerTime();
            _context.Entry<TPAYMENTDETAILTRX>(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        


    }
}
