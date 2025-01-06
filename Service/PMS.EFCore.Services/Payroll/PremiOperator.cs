using System;
using System.Collections;
using System.Collections.Generic;

using System.Text;
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
    public class PremiOperator : EntityFactory<MPREMIOPERATOR, MPREMIOPERATOR, GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public PremiOperator(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "premioperator";
            _authenticationService = authenticationService;
        }

        private string FieldsValidation(MPREMIOPERATOR PremiOperator)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(PremiOperator.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(PremiOperator.UNITCODE)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(PremiOperator.VEHICLETYPEID)) result += "Tipe Kendaraan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(PremiOperator.ACTIVITYID)) result += "Kegiatan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(PremiOperator.BASISGROUP)) result += "Group tidak boleh kosong." + Environment.NewLine;
            if (PremiOperator.PRODUCTID == 0) result += "Produk tidak boleh kosong." + Environment.NewLine;
            //if (PremiOperator.UpperRange == 0) result += "Nilai maksimal range tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        private void Validate(MPREMIOPERATOR PremiOperator)
        {
            string result = this.FieldsValidation(PremiOperator);

            //if (_context.MPREMIOPERATOR.SingleOrDefault(d => d.ID.Equals(PremiOperator.ID)) != null
            //    && _context.MPREMIOPERATOR.SingleOrDefault(d => d.UNITCODE.Equals(PremiOperator.UNITCODE)) != null
            //    && _context.MPREMIOPERATOR.SingleOrDefault(d => d.ACTIVITYID.Equals(PremiOperator.ACTIVITYID)) != null
            //    && _context.MPREMIOPERATOR.SingleOrDefault(d => d.VEHICLETYPEID.Equals(PremiOperator.VEHICLETYPEID)) != null
            //    && _context.MPREMIOPERATOR.SingleOrDefault(d => d.PRODUCTID.Equals(PremiOperator.PRODUCTID)) != null
            //    )

            var existList =  (from A in _context.MPREMIOPERATOR
                                        join B in _context.MACTIVITY on A.ACTIVITYID equals B.ACTIVITYID
                                        join C in _context.MVEHICLETYPE on A.VEHICLETYPEID equals C.ID
                                        join D in _context.MPRODUCT on A.PRODUCTID equals D.ID
                                        where A.ID.ToString() != PremiOperator.ID.ToString()
                                        && A.STATUS == "A"
                                        && A.UNITCODE.Equals(PremiOperator.UNITCODE)
                                        && B.ACTIVITYID.Equals(PremiOperator.ACTIVITYID)
                                        && C.ID.Equals(PremiOperator.VEHICLETYPEID)
                                        && D.ID.Equals(PremiOperator.PRODUCTID)
                                        select new { A.BASISGROUP, ACT = B.ACTIVITYID +" - "+ B.ACTIVITYNAME, VEHICLETYPENAME = C.NAME }).FirstOrDefault();

            if (existList != null) 
            result += "Kegiatan " + existList.ACT + " Dan Tipe Kendaraan " + existList.VEHICLETYPENAME + " sudah Terdaftar di " + existList.BASISGROUP + Environment.NewLine;

            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void DeleteValidate(MPREMIOPERATOR PremiOperator)
        {
            string result = string.Empty;
            if (PremiOperator.RANGE < this.GetLastRange(PremiOperator.UNITCODE, PremiOperator.VEHICLETYPEID, PremiOperator.ACTIVITYID, PremiOperator.PRODUCTID))
                result += "Hanya range terakhir yang dapat dihapus." + Environment.NewLine;
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        public int GetLastRange(string unitCode, string vehicleTypeId, string activityTypeId, int productId)
        {
           return _context.Database.ExecuteSqlCommand($"Exec sp_PremiMandor_DeleteByPaymentNo {unitCode},{vehicleTypeId},{activityTypeId},{productId}");
        }

        private string GenereteNewNumber(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.PremiOperatorIdPrefix + unitCode, _context);
            return PMSConstants.PremiOperatorIdPrefix + unitCode + dateTime.ToString("yyyyMMdd") + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override MPREMIOPERATOR BeforeSave(MPREMIOPERATOR record, string userName, bool newRecord)
        {
            if (string.IsNullOrEmpty(record.ID))
                record.ID = GenereteNewNumber(record.UNITCODE, record.CREATED);

            DateTime now = GetServerTime();
            if (newRecord)
            {   
                if (_context.MPREMIOPERATOR.SingleOrDefault(d => d.ID.Equals(record.ID)) != null)
                    throw new Exception("ID Premi dengan nomor tersebut sudah ada.");

                record.STATUS = "A";
                record.CREATED = now;
                record.CREATEBY = userName;

            }
            record.UPDATED = now;
            record.UPDATEBY = userName;
            Validate(record);
            return record;

        }

        

        protected override MPREMIOPERATOR AfterSave(MPREMIOPERATOR record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.PremiOperatorIdPrefix + record.UNITCODE, _context);
            return record;
        }


        protected override bool DeleteFromDB(MPREMIOPERATOR record, string userName)
        {
            record = GetSingle(record.ID);
            record.STATUS = "D";
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            _context.Entry<MPREMIOPERATOR>(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;

        }

        protected override MPREMIOPERATOR GetSingleFromDB(params  object[] keyValues)
        {
            return _context.MPREMIOPERATOR
            .SingleOrDefault(d => d.ID.Equals(keyValues[0]));
        }

        public override IEnumerable<MPREMIOPERATOR> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<MPREMIOPERATOR>();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.ID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.BASISGROUP.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.DESCRIPTION.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.ID == filter.Id);

            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(p => p.UNITCODE == filter.UnitID);

            if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(p => p.STATUS == PMSConstants.TransactionStatusApproved);
            else
                criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

            if (filter.PageSize <= 0)
                return _context.MPREMIOPERATOR.Where(criteria).ToList();
            return _context.MPREMIOPERATOR.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public IEnumerable<MPREMIOPERATOR> GetAll(object filterParameter)
        {
            try
            {
                return _context.MPREMIOPERATOR.ToList();
            }
            catch { return new List<MPREMIOPERATOR>(); }
        }
    }

}