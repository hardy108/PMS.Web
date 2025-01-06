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
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class PaymentDetail : EntityFactory<TPAYMENTDETAIL,TPAYMENTDETAIL,FilterSalary, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;
        private Employee _serviceEmp;
        private AuthenticationServiceBase _authenticationService;
        public PaymentDetail(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PaymentDetail";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context,_authenticationService,auditContext);
            _serviceDivisiName = new Divisi(context,_authenticationService,auditContext);
            _serviceEmp = new Employee(context,_authenticationService,auditContext);
        }

        protected override TPAYMENTDETAIL BeforeSave(TPAYMENTDETAIL record, string userName, bool newRecord)
        {
            if (string.IsNullOrEmpty(record.POSITIONID))
                record.POSITIONID = null;

            if (string.IsNullOrEmpty(record.STATUSID))
                record.STATUSID = null;

            if (string.IsNullOrEmpty(record.SUPERVISORID))
                record.SUPERVISORID = null;

            return record;// base.BeforeSaveInsert(record, userName);
        }

        protected override TPAYMENTDETAIL GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.TPAYMENTDETAIL.Include(b => b.DOCNONavigation).Include(a => a.EMP).SingleOrDefault(d => d.DOCNO.Equals(id));

            //return base.GetSingle(keyValues);
        }

        public override IEnumerable<TPAYMENTDETAIL> GetList(FilterSalary filter)
        {
            
            var criteria = PredicateBuilder.True<TPAYMENTDETAIL>();
            try
            {
                //if (filter.AUTO.HasValue)
                //    criteria = criteria.And(d => d.AUTO == filter.AUTO.Value);

                if (!string.IsNullOrEmpty(filter.Id))
                    criteria = criteria.And(d => d.DOCNO == filter.Id);

                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.DOCNO));

                if (!string.IsNullOrEmpty(filter.DOCNO))
                    criteria = criteria.And(d => d.DOCNO == filter.DOCNO);

                if (!string.IsNullOrEmpty(filter.EMPID))
                    criteria = criteria.And(d => d.EMPID == filter.EMPID);

                if (!string.IsNullOrEmpty(filter.UnitID))
                    criteria = criteria.And(d => d.DOCNONavigation.UNITCODE == filter.UnitID);

                //if (!string.IsNullOrEmpty(filter.STATUS))
                //    criteria = criteria.And(d => d.STATUS == filter.STATUS);

                DateTime dateNull = new DateTime();
                if (filter.Date.Date != dateNull)
                    criteria = criteria.And(d => d.JOINTDATE == filter.Date);

                if (filter.StartDate.Date != dateNull || filter.EndDate.Date != dateNull)
                    criteria = criteria.And(d => d.DOCNONavigation.DOCDATE.Date >= filter.StartDate.Date
                                && d.DOCNONavigation.DOCDATE.Date <= filter.EndDate.Date);

                if (filter.PageSize <= 0)
                    return _context.TPAYMENTDETAIL.Where(criteria).ToList();
                return _context.TPAYMENTDETAIL.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }

        public List<sp_PaymentDetail_GetTaxYTD_Result> GetTaxYTD(string unitId, DateTime Date)
        {
            return _context.sp_PaymentDetail_GetTaxYTD_Result(unitId, Date).ToList();

            //repository.GetTaxYTD(unitId, date);
        }

        public List<MEMPLOYEE> GetFromEmployee(string unitCode, DateTime resignedDate)
        {
            return _context.MEMPLOYEE.Include(b => b.MEMPLOYEEBANK).Include(c => c.STATUSNavigation).Include(d => d.EMPTYPENavigation)
                    .Where(a => a.UNITCODE == unitCode && a.RESIGNEDDATE == resignedDate
                    && (a.STATUS == PMSConstants.TransactionStatusApproved || a.STATUS == PMSConstants.TransactionStatusDeleted 
                    || a.STATUS == PMSConstants.TransactionStatusCanceled)).ToList();
            //repository.GetFromEmployee(unitCode, startDate, endDate);
        }
    }
}
