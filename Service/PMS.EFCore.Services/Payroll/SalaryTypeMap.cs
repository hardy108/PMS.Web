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
    public class SalaryTypeMap : EntityFactory<TSALARYTYPEMAP,TSALARYTYPEMAP,FilterSalaryTypeMap, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public SalaryTypeMap(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "SalaryTypeMap";
            _authenticationService = authenticationService;
        }

        private string FieldsValidation(TSALARYTYPEMAP salaryTypeMap)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(salaryTypeMap.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(salaryTypeMap.UNITID)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(salaryTypeMap.TYPEID)) result += "Jenis premi tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(salaryTypeMap.FREQ)) result += "Frekuensi tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(salaryTypeMap.PositionId)) result += "Jabatan tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(salaryTypeMap.EmployeeId)) result += "Pegawai tidak boleh kosong." + Environment.NewLine;
            //if (salaryTypeMap.HkMin == 0) result += "Hari Kerja Minimum tidak boleh kosong." + Environment.NewLine;
            if (salaryTypeMap.AMOUNT == 0) result += "Jumlah tidak boleh kosong." + Environment.NewLine;
            if (salaryTypeMap.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        private void Validate(TSALARYTYPEMAP salaryTypeMap)
        {
            string result = this.FieldsValidation(salaryTypeMap);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void DeleteValidate(TSALARYTYPEMAP salaryTypeMap)
        {

            if (salaryTypeMap.STATUS == "D")
                throw new Exception("Transaksi sudah dihapus.");
      
        }

        private void UpdateValidate(TSALARYTYPEMAP salaryTypeMap)
        {

            this.Validate(salaryTypeMap);
            var currSal = GetSingle(salaryTypeMap.ID);
            if (currSal != null)
            {
                if (currSal.STATUS == "D")
                    throw new Exception("Data sudah di hapus/update..");
            }
        }

        protected override TSALARYTYPEMAP BeforeSave(TSALARYTYPEMAP record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                if (_context.TSALARYTYPEMAP.SingleOrDefault(d => d.ID.Equals(record.ID)) != null)
                    throw new Exception("Id sudah terdaftar.");

                record.STATUS = "A";
                record.ID = PMSConstants.SalaryTypeMapIdPrefix + "-" + record.UNITID + "-" + record.UPDATED.ToString("yyyyMMdd") + "-" + HelperService.GetCurrentDocumentNumber(PMSConstants.SalaryTypeMapIdPrefix + record.UNITID, _context).ToString("0000");
            }
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            return record;
        }

        protected override TSALARYTYPEMAP AfterSave(TSALARYTYPEMAP record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.SalaryTypeMapIdPrefix + record.UNITID, _context);
            return record;
        }

      

        protected override TSALARYTYPEMAP BeforeDelete(TSALARYTYPEMAP record, string userName)
        {
            DeleteValidate(record);
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            record.STATUS = "D";
            return record;
        }

        protected override bool DeleteFromDB(TSALARYTYPEMAP record, string userName)
        {
            _context.TSALARYTYPEMAP.Update(record);
            _context.SaveChanges();

            return true;
        }

        protected override TSALARYTYPEMAP GetSingleFromDB(params  object[] keyValues)
        {
            return _context.TSALARYTYPEMAP
            .SingleOrDefault(d => d.ID.Equals(keyValues[0]));
        }

        public override IEnumerable<TSALARYTYPEMAP> GetList(FilterSalaryTypeMap filter)
        {
            
            var criteria = PredicateBuilder.True<TSALARYTYPEMAP>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.unitId))
                    criteria = criteria.And(p => p.UNITID == filter.unitId);

                if (!string.IsNullOrWhiteSpace(filter.employeeCode))
                    criteria = criteria.And(p => p.EMPID == filter.employeeCode);

                if (!string.IsNullOrWhiteSpace(filter.premiId))
                    criteria = criteria.And(p => p.TYPEID == filter.premiId);

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    criteria = criteria.And(p => p.TYPEID.Contains(filter.Keyword) || p.POSID.Contains(filter.Keyword) || p.EMPID.Contains(filter.Keyword));
            }

            if (filter.PageSize <= 0)
                return _context.TSALARYTYPEMAP.Where(criteria).ToList();
            return _context.TSALARYTYPEMAP.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public  IEnumerable<TSALARYTYPEMAP> GetAll(object filterParameter)
        {
            try
            {
                return _context.TSALARYTYPEMAP.ToList();
            }
            catch { return new List<TSALARYTYPEMAP>(); }
        }
    }

}