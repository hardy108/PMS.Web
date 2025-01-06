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
using Remotion.Linq.Clauses;

namespace PMS.EFCore.Services.Logistic
{
    public class DendaMandor : EntityFactory<TMANDORFINE,TMANDORFINE, FilterSalary, PMSContextBase>
    {
        private Period _servicePeriod;
        private Divisi _serviceDivisiName;
        private Employee _serviceEmp;
        private AuthenticationServiceBase _authenticationService;
        public DendaMandor(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "DendaMandor";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context,_authenticationService,auditContext);
            _serviceDivisiName = new Divisi(context,_authenticationService,auditContext);
            _serviceEmp = new Employee(context,_authenticationService,auditContext);
        }

        private void DeleteValidate(TMANDORFINE mandorFine)
        {
            if (mandorFine.STATUS == PMSConstants.TransactionStatusDeleted) throw new Exception("Transaksi sudah dihapus.");
            _servicePeriod.CheckValidPeriod(mandorFine.UNITID, mandorFine.DATE);
        }

        

        protected override TMANDORFINE AfterSave(TMANDORFINE record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.MandorFineIdPrefix + record.UNITID, _context);

            return record;
        }

        
       


        
        private void Validate(TMANDORFINE mandorFine)
        {
            
            string result = string.Empty;
            if (string.IsNullOrEmpty(mandorFine.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(mandorFine.UNITID)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(mandorFine.EMPID)) result += "Pegawai tidak boleh kosong." + Environment.NewLine;
            if (mandorFine.STATUS == PMSConstants.TransactionStatusNone) result += "Status tidak boleh kosong." + Environment.NewLine;

            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(mandorFine.UNITID, mandorFine.DATE);
            _servicePeriod.CheckMaxPeriod(mandorFine.UNITID, mandorFine.DATE);
        }

        

        protected override TMANDORFINE BeforeSave(TMANDORFINE mandorFine, string userName, bool newRecord)
        {
            DateTime now = GetServerTime();
            Validate(mandorFine);
            if (newRecord)
            {
                int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.MandorFineIdPrefix + mandorFine.UNITID, _context);
                mandorFine.ID = PMSConstants.MandorFineIdPrefix + mandorFine.UNITID + mandorFine.UPDATED.ToString("yyyyMMdd") + lastNumber.ToString().PadLeft(6, '0');

                var premiExist = _context.TMANDORFINE.FirstOrDefault(d => d.ID.Equals(mandorFine.ID));
                if (premiExist != null) throw new Exception("Id " + mandorFine.ID + " sudah terdaftar.");

            }
            else
            {
                if (mandorFine.STATUS == PMSConstants.TransactionStatusDeleted)
                    throw new Exception("Data sudah di hapus.");
            }
            mandorFine.UPDATED = now;
            
            return mandorFine;
        }


        protected override TMANDORFINE BeforeDelete(TMANDORFINE record, string userName)
        {
            this.DeleteValidate(record);

            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPDATED = this.GetServerTime();

            return record;
        }

        protected override bool DeleteFromDB(TMANDORFINE record, string userName)
        {
            try
            {
                _context.Update(record);
                _context.SaveChanges();
                return true;
            }
            catch
            { throw; }
        }

        protected override TMANDORFINE GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.TMANDORFINE.SingleOrDefault(d => d.ID.Equals(id));
        }

        public override IEnumerable<TMANDORFINE> GetList(FilterSalary filter)
        {
            
            var criteria = PredicateBuilder.True<TMANDORFINE>();
            
            if (!string.IsNullOrEmpty(filter.Id))
                criteria = criteria.And(d => d.ID == filter.Id);
            if (filter.Ids.Any())
                criteria = criteria.And(d => filter.Ids.Contains(d.ID));

            if (!string.IsNullOrEmpty(filter.UnitID))
                criteria = criteria.And(d => d.UNITID == filter.UnitID);
            if (filter.UnitIDs.Any())
                criteria = criteria.And(d => filter.UnitIDs.Contains(d.UNITID));

            if (!string.IsNullOrEmpty(filter.EMPID))
                criteria = criteria.And(d => d.EMPID == filter.EMPID);

            if (!string.IsNullOrEmpty(filter.STATUS))
                criteria = criteria.And(d => d.STATUS == filter.STATUS);

            DateTime dateNull = new DateTime();
            if (filter.StartDate.Date != dateNull || filter.EndDate.Date != dateNull)
                criteria = criteria.And(d => d.DATE.Date >= filter.StartDate.Date && d.DATE.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.ID.Contains(filter.Keyword) || d.EMPID.Contains(filter.Keyword));

            var result =
                from a in _context.TMANDORFINE.Where(criteria)
                join b in _context.MEMPLOYEE on a.EMPID equals b.EMPID
                select new TMANDORFINE(a) { EMPNAME = b.EMPNAME };

            if (filter.PageSize <= 0)
                return result;
            return result.GetPaged(filter.PageNo, filter.PageSize).Results;
            
        }

        public override TMANDORFINE NewRecord(string userName)
        {
            TMANDORFINE record = new TMANDORFINE
            {
                DATE = DateTime.Today,
                STATUS = "A"
            };
            return record;
            
        }
    }
}
