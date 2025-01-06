using System;
using System.Collections.Generic;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Location
{
    public class Company : EntityFactory<VCOMPANY,VCOMPANY,FilterCompany, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Company(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Company";
            _authenticationService = authenticationService;
        }

        private void Validate(VCOMPANY record, string userName)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.CODE)) result += "Kode Perusahaan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.NAME)) result += "Nama tidak boleh kosong." + Environment.NewLine;
            if (result != string.Empty) throw new Exception(result);
        }

        private void ValidateInsert(VCOMPANY record, string userName)
        {
            Validate(record, userName);

            string result = string.Empty;
            if (_context.VCOMPANY.SingleOrDefault(d => d.CODE.Equals(record.CODE)) != null) result += "Kode Perusahaan sudah terdaftar.";
            if (result != string.Empty) throw new Exception(result);
        }

        protected override VCOMPANY GetSingleFromDB(params object[] keyValues)
        {
            string legalId = keyValues[0].ToString();
            return _context.VCOMPANY.SingleOrDefault(d => d.LEGALID.Equals(legalId));
        }

        //protected override VCOMPANY GetSingleFromDB(params  object[] keyValues)
        //{
        //    return base.GetSingle(keyValues);
        //}

        public VCOMPANY GetSingle(string legalId)
        {
            //if (withAccount)
                return _context.VCOMPANY
                    .SingleOrDefault(d => d.LEGALID.Equals(legalId));
            //return GetSingle(legalId);
        }

        public override VCOMPANY CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            VCOMPANY record = base.CopyFromWebFormData(formData, newRecord);//If Required Replace with your custom code

            if (string.IsNullOrWhiteSpace(formData["NPWP"]))
                record.NPWP = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["ADDR1"]))
                record.ADDR1 = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["ADDR2"]))
                record.ADDR2 = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["ADDR3"]))
                record.ADDR3 = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["POSTALCODE"]))
                record.POSTALCODE = string.Empty;

            return record;
        }

        protected override VCOMPANY BeforeSave(VCOMPANY record, string userName, bool newRecord)
        {
            DateTime currentDate = HelperService.GetServerDateTime(1, _context);
            record.TYPE = "CMP";
            record.NPWP = StandardUtility.IsNull<string>(record.NPWP, string.Empty);
            record.ADDR1 = StandardUtility.IsNull<string>(record.ADDR1, string.Empty);
            record.ADDR2 = StandardUtility.IsNull<string>(record.ADDR2, string.Empty);
            record.ADDR3 = StandardUtility.IsNull<string>(record.ADDR3, string.Empty);
            record.POSTALCODE = StandardUtility.IsNull<string>(record.POSTALCODE, string.Empty);
            if (newRecord)
            {
                this.ValidateInsert(record, userName);
                record.LEGALID = record.CODE;
                record.ID = record.CODE;
                record.LOGO = null;
                record.ACTIVE = true;
                record.CREATED = currentDate;
                record.CREATEBY = userName;
            }
            else
                this.Validate(record, userName);
            record.UPDATED = currentDate;
            record.UPDATEBY = userName;
            return record;
        }

       


        private MORGANIZATION ConvertToOrganization(VCOMPANY company)
        {
            MORGANIZATION organization = new MORGANIZATION();
            organization.CopyFrom(company);
            return organization;
        }

        private MCOMPANY ConvertToCompany(VCOMPANY vcompany)
        {
            MCOMPANY company = new MCOMPANY();
            company.CopyFrom(vcompany);            
            return company;
        }

        protected override VCOMPANY SaveInsertToDB(VCOMPANY record, string userName)
        {
            _context.MORGANIZATION.Add(ConvertToOrganization(record));
            _context.MCOMPANY.Add(ConvertToCompany(record));
            _context.SaveChanges();
            return GetSingle(record.ID);
        }

        protected override VCOMPANY SaveUpdateToDB(VCOMPANY record, string userName)
        {
            _context.MORGANIZATION.Update(ConvertToOrganization(record));
            _context.MCOMPANY.Update(ConvertToCompany(record));            
            _context.SaveChanges();
            return GetSingle(record.ID);
        }

        //protected override bool DeleteFromDB(VCOMPANY record, string userName)
        //{
        //    record = GetSingle(record.CODE);
        //    if (record == null)
        //        throw new Exception("Record not found");

        //    DateTime now = HelperService.GetServerDateTime(1, _context);
        //    record.ACTIVE = false;
        //    record.UPDATED = now;
        //    record.UPDATEBY = userName;

        //    SaveUpdateToDB(record, userName);

        //    Security.Audit.Insert(userName, _serviceName, now, $"Delete {record.ID}", _context);

        //    return true;
        //}

        #region VCOMPANY       

        public override IEnumerable<VCOMPANY> GetList(FilterCompany filter)
        {            
            var criteria = PredicateBuilder.True<VCOMPANY>();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.LEGALID.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }

            if (filter.IsActive.HasValue)
                criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.LegalID))
                criteria = criteria.And(p => p.LEGALID.Equals(filter.LegalID));

            if (filter.PageSize <= 0)
                return _context.VCOMPANY.Where(criteria);
            return _context.VCOMPANY.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }
        #endregion
    }
}