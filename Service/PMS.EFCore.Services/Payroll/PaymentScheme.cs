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
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class PaymentScheme : EntityFactory<MPAYMENTSCHEME,MPAYMENTSCHEME,GeneralFilter, PMSContextBase>
    {
        AuthenticationServiceBase _authenticationService;
        public PaymentScheme(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PaymentScheme";
            _authenticationService = authenticationService;
        }

        private string FieldsValidation(MPAYMENTSCHEME paymentScheme)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(paymentScheme.UNITCODE)) result += "Estate Code harus diisi dahulu." + Environment.NewLine;
            if (string.IsNullOrEmpty(paymentScheme.RICEPAIDASMONEY)) result += "Beras dibayar berupa tidak boleh kosong. " + Environment.NewLine;
            if (paymentScheme.PREMIBASEDCALCULATION == string.Empty) result += "Dasar perhitungan premi tidak boleh kosong. " + Environment.NewLine;
            return result;
        }

        private void Validate(MPAYMENTSCHEME paymentScheme)
        {
            string result = this.FieldsValidation(paymentScheme);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        protected override MPAYMENTSCHEME BeforeSave(MPAYMENTSCHEME record, string userName, bool newRecord)
        {
            DateTime now = GetServerTime();
            record.UPDATED = now;
            record.UPDATEBY = userName;
            if (newRecord)
            {
                if (_context.MPAYMENTSCHEME.SingleOrDefault(d => d.UNITCODE.Equals(record.UNITCODE)) != null)
                    throw new Exception("Id sudah terdaftar.");

                record.STATUS = "A";                
                record.CREATED = now;                
                record.CREATEBY = userName;
            }
            return record;
        }

       

        protected override MPAYMENTSCHEME BeforeDelete(MPAYMENTSCHEME record, string userName)
        {
            this.Validate(record);
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            record.UPDATEBY = userName;
            record.STATUS = "D";
            return record;
        }

        protected override MPAYMENTSCHEME GetSingleFromDB(params  object[] keyValues)
        {
            return _context.MPAYMENTSCHEME
            .SingleOrDefault(d => d.UNITCODE.Equals(keyValues[0]));
        }

        public override IEnumerable<MPAYMENTSCHEME> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<MPAYMENTSCHEME>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITCODE == filter.UnitID);
            }

            if (filter.PageSize <= 0)
                return _context.MPAYMENTSCHEME.Where(criteria).ToList();
            return _context.MPAYMENTSCHEME.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public  IEnumerable<MPAYMENTSCHEME> GetAll(object filterParameter)
        {
            try
            {
                return _context.MPAYMENTSCHEME.ToList();
            }
            catch { return new List<MPAYMENTSCHEME>(); }
        }
    }

}