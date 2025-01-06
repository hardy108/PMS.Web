using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Location
{
    public class Unit:EntityFactory<VUNIT,VUNIT,GeneralFilter, PMSContextBase>
    {
        AuthenticationServiceBase _authenticationService;
        public Unit(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Unit";
            _authenticationService = authenticationService;
        }

        private void Validate(VUNIT record, string userName)
        {
            string result = string.Empty;
            if (_context.VUNIT.SingleOrDefault(d => d.CODE.Equals(record.CODE)) != null) result += "Kode Unit sudah terdaftar.";
            if (result != string.Empty) throw new Exception(result);
        }

        protected override VUNIT GetSingleFromDB(params object[] keyValues)
        {
            string unitId = keyValues[0].ToString();
            return _context.VUNIT.SingleOrDefault(d => d.UNITCODE.Equals(unitId));
        }

        
        public override VUNIT CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            VUNIT record = base.CopyFromWebFormData(formData, newRecord);
            return record;
        }

        protected override VUNIT BeforeSave(VUNIT record, string userName, bool newRecord)
        {
            record.TYPE = "EST";
            record.ALIAS = StandardUtility.IsNull<string>(record.ALIAS, string.Empty);
            record.ADDR1 = StandardUtility.IsNull<string>(record.ADDR1, string.Empty);
            record.ADDR2 = StandardUtility.IsNull<string>(record.ADDR2, string.Empty);
            record.ADDR3 = StandardUtility.IsNull<string>(record.ADDR3, string.Empty);
            record.POSTALCODE = StandardUtility.IsNull<string>(record.POSTALCODE, string.Empty);
            record.UNITMGR = StandardUtility.IsNull<string>(record.UNITMGR, string.Empty);
            record.UNITKTU = StandardUtility.IsNull<string>(record.UNITKTU, string.Empty);

            this.Validate(record, userName);
            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                record.UNITCODE = record.CODE;
                record.ID = record.CODE;
                record.ACTIVE = true;
             
                record.CREATED = now;
                
                record.CREATEBY = userName;
            }
            record.UPDATEBY = userName;
            record.UPDATED = now;
            return record;
        }

       
        private MORGANIZATION ConvertToOrganization(VUNIT unit)
        {
            MORGANIZATION organization = new MORGANIZATION();
            organization.CopyFrom(unit);
            return organization;
        }

        private MUNIT ConvertToUnit(VUNIT vunit)
        {
            MUNIT unit = new MUNIT();
            unit.CopyFrom(vunit);
            return unit;
        }

        protected override VUNIT SaveInsertToDB(VUNIT record, string userName)
        {
            _context.MORGANIZATION.Add(ConvertToOrganization(record));
            _context.MUNIT.Add(ConvertToUnit(record));
            _context.SaveChanges();
            return GetSingle(record.ID);
        }

        protected override VUNIT SaveUpdateToDB(VUNIT record, string userName)
        {
            _context.MORGANIZATION.Update(ConvertToOrganization(record));
            _context.MUNIT.Update(ConvertToUnit(record));
            _context.SaveChanges();
            return GetSingle(record.ID);
        }

        //protected override bool DeleteFromDB(VUNIT record, string userName)
        //{
        //    record = GetSingle(record.CODE);
        //    if (record == null)
        //        throw new Exception("Record not found");

        //    DateTime now = HelperService.GetServerDateTime(1, _context);
        //    record.ACTIVE = false;
        //    record.UPDATED = now;
        //    record.UPDATEBY = userName;

        //    SaveUpdateToDB(record, userName);

        //    Security.Audit.Insert(userName, _serviceName, now, $"Delete Unit {record.ID}", _context);

        //    return true;
        //}

        #region

       
        public override IEnumerable<VUNIT> GetList(GeneralFilter filter)
        {
            

            


            var criteria = PredicateBuilder.True<VUNIT>();

            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                criteria = criteria.And(_authenticationService.GetFilterUnitByUserName(filter.UserName, filter.Id));
            }


            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p => 
                    p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.UNITCODE.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
            }

            
            if (filter.IsActive.HasValue)            
                criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.UNITCODE.Equals(filter.Id));
            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains(p.UNITCODE));


            //Get All Units
            if (filter.PageSize <= 0)
                return _context.VUNIT.Where(criteria);
            return _context.VUNIT.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;



        }

        #endregion
    }
}
