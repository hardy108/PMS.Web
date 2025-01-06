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
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.GL
{
    public class Account : EntityFactory<MACCOUNT,MACCOUNT,FilterAccount, PMSContextBase>
    {
        private Period _servicePeriod;

        private string parentcode_old = string.Empty;

        public Account(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Account";
            _servicePeriod = new Period(_context,authenticationService,auditContext);
        }

        private string FieldsValidation(MACCOUNT account)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(account.CODE)) result += "Kode harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(account.NAME)) result += "Nama harus diisi." + Environment.NewLine;
            if (account.TYPEID == 0) result += "Tipe harus diisi.";
            return result;
        }

        private void Validate( MACCOUNT account)
        {
            string result = this.FieldsValidation(account);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            if (!string.IsNullOrEmpty(account.PARENTCODE))
            {
                MACCOUNT parent = GetSingle(account.PARENTCODE);
                if (parent != null)
                {
                    if (account.TYPEID != parent.TYPEID)
                        throw new Exception("Tipe tidak sama dengan parent.");
                }
            }
        }

        private void DeleteValidate(MACCOUNT account)
        {
            if (account.PARENT == true)
                throw new Exception("Unable to delete Account " + account.CODE + "."
                    + Environment.NewLine + "This account has child.");
        }

        private void InsertValidate( MACCOUNT account)
        {
            this.Validate(account);

            MACCOUNT existAccount = GetSingle(account.CODE);
            if (existAccount != null)
                throw new Exception(account.CODE + " sudah terdaftar."); return;
            
        }

        public override IEnumerable<MACCOUNT> GetList(FilterAccount filter)
        {
            

            var criteria = PredicateBuilder.True<MACCOUNT>();

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.CODE.ToLower().Contains(filter.LowerCasedSearchTerm)||
                        p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

                if (filter.TypeId != 0)
                    criteria = criteria.And(p => p.TYPEID == Convert.ToSByte(filter.TypeId));

                if (filter.Parent.HasValue)
                    criteria = criteria.And(p => p.PARENT == filter.Parent.Value);

                if (!string.IsNullOrWhiteSpace(filter.AccountCode))
                    criteria = criteria.And(p => p.CODE == filter.AccountCode);

                if (!string.IsNullOrWhiteSpace(filter.ParentCode))
                    criteria = criteria.And(p => p.PARENTCODE == filter.ParentCode);

                var result = _context.MACCOUNT.Where(criteria);
                if (filter.PageSize <= 0)
                    return result;
                return result.GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return new List<MACCOUNT>(); }

        }

        public void SetParent(string code)
        {
            MACCOUNT acc = new MACCOUNT();
            int countparentcode = _context.MACCOUNT.Where(d => d.PARENTCODE == code).Count();
            if (countparentcode > 0)
            {
                acc = (from p in _context.MACCOUNT where p.CODE == code select p).SingleOrDefault();
                acc.PARENT = true;
            }
            else
            {
                acc = (from p in _context.MACCOUNT where p.CODE == code select p).SingleOrDefault();
                acc.PARENT = false;
            }

            _context.Update(acc);
            _context.SaveChanges();

            //_context.Entry(acc).State = EntityState.Modified;
            //_context.SaveChanges();

        }

        protected override MACCOUNT BeforeSave(MACCOUNT record, string userName, bool newRecord)
        {
            
            if (record.PARENTCODE != string.Empty && record.PARENTCODE != null)
            {
                var parent = GetSingle(record.PARENTCODE);
                record.LEVEL = Convert.ToSByte(Convert.ToInt16(parent.LEVEL) + 1);
            }
            else
                record.LEVEL = 0;
            DateTime now = GetServerTime();
            if (newRecord)
            {
                InsertValidate(record);
                record.CREATEBY = userName;
                record.CREATED = now;
                record.ACTIVE = true;
            }
            else
            {
                parentcode_old = StandardUtility.IsNull(GetSingle(record.CODE).PARENTCODE, null);
                Validate(record);
            }
            record.UPDATED = now;
            record.UPDATEBY = userName;
            return record;
        }

        

        public override MACCOUNT CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MACCOUNT parent = new MACCOUNT();

            MACCOUNT record = base.CopyFromWebFormData(formData, newRecord);//If Required Replace with your custom code

            if (!string.IsNullOrEmpty(record.PARENTCODE))
            {
                parent = GetSingle(record.PARENTCODE);
                if (parent != null)
                    record.PARENTCODE = parent.CODE;
                else record.PARENTCODE = null;

            }
            return record;
        }

      
      
        protected override MACCOUNT BeforeDelete(MACCOUNT record, string userName)
        {
            DeleteValidate(record);
            return record;
        }

        protected override MACCOUNT AfterSave(MACCOUNT record, string userName, bool newRecord)
        {

            if (record.PARENTCODE != null && record.PARENTCODE != string.Empty)
                SetParent(record.PARENTCODE);

            if (!newRecord)
            {
                if (parentcode_old != null && parentcode_old != string.Empty)
                    SetParent(parentcode_old);

                if (record.PARENT == true)
                    UpdateChildLevel(record.CODE, record.LEVEL, userName);

                //Back To Record Existing
                _context.Entry(record).State = EntityState.Modified;
            }
            return record;
        }
        

        

        protected override bool DeleteFromDB(MACCOUNT record, string userName)
        {
            record = GetSingle(record.CODE);
            record.ACTIVE = false ;
            record.UPDATEBY = userName;
            record.UPDATED = Utilities.HelperService.GetServerDateTime(1, _context);
            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;

        }

        public void UpdateChildLevel(string parentCode, int parentLevel, string userName)
        {
            var childAccounts = _context.MACCOUNT.Where(d => d.CODE == parentCode).ToList();

            foreach (MACCOUNT account in childAccounts)
            {
                account.LEVEL = Convert.ToSByte(Convert.ToInt16(parentLevel) + 1);
                _context.Update(account);
                _context.SaveChanges();
                UpdateChildLevel(account.CODE, account.LEVEL, userName);
            }
        }

        protected override MACCOUNT GetSingleFromDB(params  object[] keyValues)
        {
            return _context.MACCOUNT.SingleOrDefault(d => d.CODE.Equals(keyValues[0]));
        }

        public bool CloseYear(string unitCode, int year, string accountCode, string userName)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(unitCode)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(accountCode)) result += "Account tidak boleh kosong." + Environment.NewLine;
            if (year == 0) result += "Tahun tidak boleh kosong." + Environment.NewLine;

            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckCloseYearValidPeriod(unitCode, year);

            string id = _context.sp_Account_CloseYear(unitCode, year, accountCode, GetServerTime());

            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Close Account Year {id}", _context);

            return true;
        }

        public void SetBalance(string unitId, string accountId, string objectId, int month, int year, decimal amount)
        {
            _context.Database.ExecuteSqlCommand($"Exec sp_AccountBalance_SetAmount1 {unitId},{accountId},{objectId},{month},{year},{amount}");
        }

    }
}
