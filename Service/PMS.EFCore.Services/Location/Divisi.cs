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
using FileStorage.EFCore;

namespace PMS.EFCore.Services.Location
{
    public class Divisi:EntityFactory<VDIVISI,VDIVISI,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Divisi(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Divisi";
            _authenticationService = authenticationService;
        }

        private void Validate(VDIVISI record, string userName)
        {
            string result = string.Empty;
            if (_context.VDIVISI.SingleOrDefault(d => d.DIVID.Equals(record.DIVID)) != null) result += "Kode Divisi sudah terdaftar.";
            if (result != string.Empty) throw new Exception(result);
        }

        protected override VDIVISI GetSingleFromDB(params  object[] keyValues)
        {
            string divisiId = keyValues[0].ToString();
            return _context.VDIVISI.SingleOrDefault(d => d.DIVID.Equals(divisiId));
        }

        public override VDIVISI CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            VDIVISI record = base.CopyFromWebFormData(formData, newRecord);
            return record;
        }

        protected override VDIVISI BeforeSave(VDIVISI record, string userName, bool newRecord)
        {
            record.TYPE = "DIV";
            record.DIVASISTEN = StandardUtility.IsNull<string>(record.DIVASISTEN, string.Empty);
            record.DIVASKEP = StandardUtility.IsNull<string>(record.DIVASKEP, string.Empty);
            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                var newId = record.UNITCODE + "-" + record.CODE;
                record.DIVID = newId;
                record.ID = newId;
                record.ACTIVE = true;
                record.CREATED = now;
                record.CREATEBY = userName;
            }
            record.UPDATED = now;
            record.UPDATEBY = userName;
            this.Validate(record, userName);
            return record;
        }

       

        private MORGANIZATION ConvertToOrganization(VDIVISI divisi)
        {
            MORGANIZATION organization = new MORGANIZATION();
            organization.CopyFrom(divisi);
            return organization;
        }

        private MDIVISI ConvertToDivisi(VDIVISI vdivisi)
        {
            MDIVISI divisi = new MDIVISI();
            divisi.CopyFrom(vdivisi);
            return divisi;
        }

        protected override VDIVISI SaveInsertToDB(VDIVISI record, string userName)
        {
            _context.MORGANIZATION.Add(ConvertToOrganization(record));
            _context.MDIVISI.Add(ConvertToDivisi(record));
            _context.SaveChanges();
            return GetSingle(record.ID);
        }

        protected override VDIVISI SaveUpdateToDB(VDIVISI record, string userName)
        {
            _context.MORGANIZATION.Update(ConvertToOrganization(record));
            _context.MDIVISI.Update(ConvertToDivisi(record));
            _context.SaveChanges();
            return GetSingle(record.ID);
        }

        //protected override bool DeleteFromDB(VDIVISI record, string userName)
        //{
        //    record = GetSingle(record.CODE);
        //    if (record == null)
        //        throw new Exception("Record not found");

        //    DateTime now = HelperService.GetServerDateTime(1, _context);
        //    record.ACTIVE = false;
        //    record.UPDATED = now;
        //    record.UPDATEBY = userName;

        //    SaveUpdateToDB(record, userName);

        //    Security.Audit.Insert(userName, _serviceName, now, $"Delete Divisi {record.ID}", _context);

        //    return true;
        //}

        #region VDIVISI

        
        public override IEnumerable<VDIVISI> GetList(GeneralFilter filter)
        {
            

            var criteria = PredicateBuilder.True<VDIVISI>();

            if (!string.IsNullOrWhiteSpace(filter.UserName))            

                criteria = criteria.And(_authenticationService.GetFilterDivisiByUserName(filter.UserName, filter.UnitID, filter.Id));
                    
                
            

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.UNITCODE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.DIVID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.FULLNAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }

            if (filter.IsActive.HasValue)
                criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);
            
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.DIVID.Equals(filter.Id));

            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains(p.DIVID));

            //Get All Units
            if (filter.PageSize<=0)
                return _context.VDIVISI.Where(criteria);
            return _context.VDIVISI.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        #endregion
    }
}
