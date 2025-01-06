using System;
using System.Collections;
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
using AM.EFCore.Services;
using System.ComponentModel.DataAnnotations;

namespace PMS.EFCore.Services.General
{
    
    public class Activity : EntityFactory<MACTIVITY,MACTIVITY,FilterActivity, PMSContextBase>
    {
        private MACTIVITYACCOUNT _newactacc = new MACTIVITYACCOUNT();
        private AuthenticationServiceBase _authenticationService;
        public Activity(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Activity";
            _authenticationService = authenticationService;
        }

        public override MACTIVITY CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MACTIVITY record = base.CopyFromWebFormData(formData, newRecord);
            List<MMATERIAL> materials = new List<MMATERIAL>();
            materials.CopyFrom<MMATERIAL>(formData, "MATERIAL");
            record.MATERIAL = materials;

            MACTIVITYACCOUNT _actacc = new MACTIVITYACCOUNT();
            _actacc.CopyFrom(formData);
            _actacc.ACTIVITY = record;
            
            //record.MACTIVITYACCOUNT = _actacc;
            _newactacc = _actacc;

            return record;
        }

        private void SaveActivityMaterials(string activityId, List<MMATERIAL> materials, string userName)
        {

            List<TACTIVITYMATERIALMAP> deleted = new List<TACTIVITYMATERIALMAP>(),
                updated = new List<TACTIVITYMATERIALMAP>(),
                inserted = new List<TACTIVITYMATERIALMAP>();

            if (materials == null)
                materials = new List<MMATERIAL>();

            //Delete Material
            List<string> newIds = materials.Select(d => d.MATERIALID).ToList();
            List<string> oldIds = _context.TACTIVITYMATERIALMAP.Where(d => d.ACTIVITYID.Equals(activityId)).Select(d => d.MATERIALID).ToList();


            DateTime now = GetServerTime();

            deleted = (from a in _context.TACTIVITYMATERIALMAP.Where(d => d.ACTIVITYID.Equals(activityId))
                           join b in StandardUtility.GetDeletedItems(oldIds, newIds) on a.MATERIALID equals b
                           select a).ToList();

            deleted.ForEach(d => {
                d.STATUS = PMSConstants.TransactionStatusDeleted;
                d.UPDATEBY = userName;
                d.UPDATED = now;
            });

            updated = (from a in _context.TACTIVITYMATERIALMAP.Where(d => d.ACTIVITYID.Equals(activityId))
                           join b in StandardUtility.GetUpdatedItems(oldIds, newIds) on a.MATERIALID equals b
                           select a).ToList();

            updated.ForEach(d => {
                d.STATUS = PMSConstants.TransactionStatusApproved;
                d.UPDATEBY = userName;
                d.UPDATED = now;
            });


            inserted = StandardUtility.GetNewItems(oldIds, newIds)
                .Select(d=> new TACTIVITYMATERIALMAP
                {
                    ACTIVITYID = activityId,
                    MATERIALID = d,
                    CREATED = now,
                    CREATEBY = userName,
                    UPDATEBY = userName,
                    UPDATED = now,
                    STATUS = PMSConstants.TransactionStatusApproved
                }).ToList();
                           

            if (deleted.Any())
                _context.UpdateRange(deleted);

            if (updated.Any())
                _context.UpdateRange(updated);

            if (inserted.Any())
                _context.AddRange(inserted);

        }

        public MACTIVITY GetSingle(string Id, bool withAccount,bool withMaterial)
        {
            MACTIVITY record = null;
            if (withAccount)
                record = _context.MACTIVITY.Include(d => d.MACTIVITYACCOUNT).SingleOrDefault(d => d.ACTIVITYID.Equals(Id));
            else
                record = GetSingle(Id);

            if (withMaterial)
            {
                
                record.MATERIAL =
                    (from a in _context.TACTIVITYMATERIALMAP
                     join b in _context.MMATERIAL on a.MATERIALID equals b.MATERIALID
                     where a.ACTIVITYID.Equals(Id) && a.STATUS == PMSConstants.TransactionStatusApproved
                     select b).ToList();                
                if (record.MATERIAL == null)
                    record.MATERIAL = new List<MMATERIAL>();
            }

            return record;
        }

        protected override MACTIVITY BeforeSave(MACTIVITY record, string userName,bool newRecord)
        {
            DateTime currentDate = Utilities.HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {   
                record.CREATED = currentDate;                
                record.CREATEBY = userName;
            }
            record.UPDATED = currentDate;
            record.UPDATEBY = userName;
            return record;
        }

        

        protected override MACTIVITY SaveInsertToDB(MACTIVITY record, string userName)
        {
            _context.MACTIVITY.Add(record);
            if (_newactacc != null)
                _context.MACTIVITYACCOUNT.Add(_newactacc);
            SaveActivityMaterials(record.ACTIVITYID, record.MATERIAL.ToList(), userName);
            _context.SaveChanges();

            return record;
        }

        protected override MACTIVITY SaveUpdateToDB(MACTIVITY record, string userName)
        {
            _context.MACTIVITY.Update(record);
            _context.MACTIVITYACCOUNT.Update(_newactacc);
            SaveActivityMaterials(record.ACTIVITYID, record.MATERIAL.ToList(), userName);
            _context.SaveChanges();

            return record;
        }

        public override IEnumerable<MACTIVITY> GetList(FilterActivity filter)
        {
            
            var criteria = PredicateBuilder.True<MACTIVITY>();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p => 
                        p.ACTIVITYID.ToLower().Contains(filter.LowerCasedSearchTerm) || 
                        p.ACTIVITYNAME.ToLower().Contains(filter.LowerCasedSearchTerm));

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.ACTIVITYID.Equals(filter.Id));

                //Added By Junaidi 2020-03-29 - Start
                if (filter.Ids.Any())
                    criteria = criteria.And(p => filter.Ids.Contains(p.ACTIVITYID));
                //Added By Junaidi 2020-03-29 - End

                var criteriaActivityType = PredicateBuilder.True<MACTIVITY>();
                bool allType = true;

                if (filter.RA)
                {
                    criteriaActivityType = criteriaActivityType.Or(p => p.RA == true);
                    allType = false;
                }

                if (filter.CE)
                {
                    criteriaActivityType = criteriaActivityType.Or(p => p.CE == true);
                    allType = false;
                }

                if (filter.GA)
                {
                    criteriaActivityType = criteriaActivityType.Or(p => p.GA == true);
                    allType = false;
                }




                if (filter.Nursery)
                {
                    criteriaActivityType = criteriaActivityType.Or(p => p.NURSERY == true);
                    allType = false;
                }

                if (filter.LC)
                {
                    criteriaActivityType = criteriaActivityType.Or(p => p.LC == true);
                    allType = false;
                }

                if (filter.TBM)
                {
                    criteriaActivityType = criteriaActivityType.Or(p => p.TBM == true);
                    allType = false;
                }

                if (filter.TM)
                {
                    criteriaActivityType = criteriaActivityType.Or(p => p.TM == true);
                    allType = false;
                }

                if (filter.HV)
                {
                    criteriaActivityType = criteriaActivityType.Or(p => p.HV == true);
                    allType = false;
                }

                if (!allType)
                    criteria = criteria.And(criteriaActivityType);

                if (filter.HVYTPE.HasValue)
                    criteria = criteria.And(p => p.HVTYPE == filter.HVYTPE.Value);

                if (!string.IsNullOrWhiteSpace(filter.RFID))
                    criteria = criteria.And(p => p.RFID == filter.RFID);

                if (filter.AUTO.HasValue)
                    criteria = criteria.And(p => p.AUTO == filter.AUTO.Value);
                

            }

            if (filter.PageSize <= 0)
                return _context.MACTIVITY.Where(criteria);
            return _context.MACTIVITY.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        private void MappingValidate(MACTIVITY record)
        {
            if (string.IsNullOrEmpty(record.ACTIVITYID))
                throw new Exception("Id Kegiatan harus diisi.");

            var q = _context.MACTIVITY.Where(itm => string.IsNullOrEmpty(itm.ACTIVITYID));
            if (q.Count() > 0)
                throw new Exception("Id Bahan harus diisi.");
        }

        public List<TACTIVITYMATERIALMAP> UpdateMaterial(IFormCollection formData, string userName)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            MACTIVITY _mAct = new MACTIVITY();
            _mAct.CopyFrom<MACTIVITY>(formData);
            this.MappingValidate(_mAct);
            var actMat = _context.TACTIVITYMATERIALMAP.Where(am => am.ACTIVITYID.Equals(_mAct.ACTIVITYID));
            if (actMat.Count() > 0)
                foreach (TACTIVITYMATERIALMAP _tActMap in actMat)
                {
                    _tActMap.STATUS = PMSConstants.TransactionStatusDeleted;
                    _tActMap.UPDATEBY = userName;
                    _tActMap.UPDATED = now;
                    _context.TACTIVITYMATERIALMAP.Update(_tActMap);
                    _context.SaveChanges();
                }
            Security.Audit.Insert(userName, "TACTIVITYMATERIALMAP", HelperService.GetServerDateTime(1, _context), $"Edit {_mAct.ACTIVITYID}", _context);
            List<TACTIVITYMATERIALMAP> _newMat = new List<TACTIVITYMATERIALMAP>();
            _newMat.CopyFrom<List<TACTIVITYMATERIALMAP>>(formData);
            foreach (TACTIVITYMATERIALMAP _newActMap in _newMat)
            {
                var _checkMat = actMat.Where(am => am.MATERIALID.Equals(_newActMap.MATERIALID));
                if (_checkMat.Count() > 0)
                {
                    _newActMap.STATUS = PMSConstants.TransactionStatusApproved;
                    _newActMap.UPDATEBY = userName;
                    _newActMap.UPDATED = now;
                    _context.TACTIVITYMATERIALMAP.Update(_newActMap);
                }
                else
                {
                    _newActMap.STATUS = PMSConstants.TransactionStatusApproved;
                    _newActMap.CREATEBY = userName;
                    _newActMap.CREATED = now;
                    _newActMap.UPDATEBY = userName;
                    _newActMap.UPDATED = now;
                    _context.TACTIVITYMATERIALMAP.Add(_newActMap);
                }
                _context.SaveChanges();
            }
            Security.Audit.Insert(userName, "TACTIVITYMATERIALMAP", HelperService.GetServerDateTime(1, _context), $"Insert {_mAct.ACTIVITYID}", _context);

            return _newMat;
        }

        public List<MMATERIAL> GetListMat()
        {
            return _context.MMATERIAL.ToList();
        }

        public override MACTIVITY NewRecord(string userName)
        {
            return new MACTIVITY
            {
                ACTIVE = true
            };
        }
    }
}
