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
using PMS.Shared.Utilities;
using PMS.EFCore.Helper;


using FileStorage.EFCore;
using System.Data;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{
    public class Material : EntityFactory<MMATERIAL,MMATERIAL,FilterMaterial, PMSContextBase>
    {
        AuthenticationServiceBase _authenticationService;
        public Material(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Material";
            _authenticationService = authenticationService;
        }

        private string FieldsValidation(MMATERIAL material)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(material.MATERIALID)) result += "Id harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(material.MATERIALNAME)) result += "Nama harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(material.UOM)) result += "Satuan harus diisi." + Environment.NewLine;
            return result;
        }

        private void Validate(MMATERIAL material)
        {
            string result = this.FieldsValidation(material);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void InsertValidate(MMATERIAL material)
        {
            this.Validate(material);

            var materialExist = this.Get(material.MATERIALID);
            if (materialExist != null)
                throw new Exception("Material sudah pernah diinput.");
        }

        public override MMATERIAL CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MMATERIAL record = base.CopyFromWebFormData(formData, newRecord);
            return record;
        }

        protected override MMATERIAL BeforeSave(MMATERIAL record, string userName, bool newRecord)
        {
            DateTime now = GetServerTime();
            if (newRecord)
            {
                this.InsertValidate(record);
                record.CREATEDATE = GetServerTime();
                record.CREATEBY = userName;
                record.ACTIVE = true;
            }
            else
                Validate(record);
            record.UPDATED = now;
            record.UPDATEBY = userName;
            return record;
        }

        

        protected override bool DeleteFromDB(MMATERIAL record, string userName)
        {
            record = GetSingle(record.MATERIALID);
            record.ACTIVE = false;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            _context.Entry(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true;

        }

        public override IEnumerable<MMATERIAL> GetList(FilterMaterial filter)
        {
            
            var criteria = PredicateBuilder.True<MMATERIAL>();
            try
            {
                //Added By Junaidi 2020-03-29 - Start
                if (filter.Ids.Any())
                    criteria = criteria.And(p => filter.Ids.Contains(p.MATERIALID));
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => filter.Id.Equals(p.MATERIALID));
                //Added By Junaidi 2020-03-29 - End

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.MATERIALID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.MATERIALNAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.UOM))
                    criteria = criteria.And(p => p.UOM.Equals(filter.UOM));

                if (!string.IsNullOrWhiteSpace(filter.AccountCode))
                    criteria = criteria.And(p => p.ACCOUNTCODE.Equals(filter.AccountCode));

                if (filter.PageSize<=0)
                    return _context.MMATERIAL
                        .Include(d=> d.TACTIVITYMATERIALMAP)
                        .Where(criteria);
                return _context.MMATERIAL
                    .Include(d => d.TACTIVITYMATERIALMAP)
                    .Where(criteria)
                    .GetPaged(filter.PageNo,filter.PageSize).Results;


            }
            catch { return new List<MMATERIAL>(); }
        }

        public List<MMATERIAL> Find(string id, string name, bool active, string activityId)
        {
            List<MMATERIAL> result = null;

            if (activityId == null || activityId == string.Empty)
            {
                result =
                    (
                    from a in _context.MMATERIAL where a.ACCOUNTCODE.Equals(activityId) && 
                    a.MATERIALNAME.Equals(name) && a.ACTIVE == active select a
                    ).ToList();

            }
            else if(activityId != null || activityId != string.Empty)
            {
                result =
                    (
                    from a in _context.MMATERIAL join b in _context.TACTIVITYMATERIALMAP on a.MATERIALID equals b.MATERIALID
                    into temp from t in temp.DefaultIfEmpty()
                    where a.MATERIALID.Equals(activityId) && a.MATERIALNAME.Equals(name) && a.ACTIVE == active 
                    && t.ACTIVITYID.Equals(activityId) && t.STATUS =="A"
                    select a ).Distinct().ToList();               
            }
            
            return result;

        }

        public MMATERIAL Get(string id)
        {
            return _context.MMATERIAL.SingleOrDefault(d => d.MATERIALID == id);
        }

        //public MMATERIAL GetSingle(string userName, string Id, bool withActivity)
        //{
        //    if (withActivity)
        //        return _context.MMATERIAL
        //            .Include(d => d.TACTIVITYMATERIALMAP)
        //            .SingleOrDefault(d => d.MATERIALID.Equals(Id));
        //    return GetSingle(Id);
        //}

        protected override MMATERIAL GetSingleFromDB(params object[] keyValues)
        {
            string id = keyValues[0].ToString();
            return _context.MMATERIAL.SingleOrDefault(d => d.MATERIALID.Equals(id));
        }

        public List<MMATERIAL> GetByActivity(string code)
        {
            List<MMATERIAL> result = null;

            result =
                (
                from a in _context.MMATERIAL join b in _context.TACTIVITYMATERIALMAP on a.MATERIALID equals b.MATERIALID
                into temp from t in temp.DefaultIfEmpty()
                where t.MATERIALID == code && t.STATUS == "A"
                select a ).ToList();
            return result;
        }
        
    }
}
