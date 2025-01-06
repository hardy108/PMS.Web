using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using PMS.EFCore.Services.General;
using AM.EFCore.Services;
using PMS.Shared.Models;

namespace PMS.EFCore.Services.Logistic
{
    public class Contract : EntityFactory<TCONTRACT,TCONTRACT,GeneralFilter, PMSContextBase>
    {

        
        private List<TCONTRACTITEM> _newContractItems = new List<TCONTRACTITEM>();
        private List<TCONTRACTITEM> _deletedContractItems = new List<TCONTRACTITEM>();
        private List<TCONTRACTITEM> _editedContractItems = new List<TCONTRACTITEM>();
        private Activity _activityService;
        private AuthenticationServiceBase _authenticationService;
        public Contract(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Contract";
            _authenticationService = authenticationService;
            _activityService = new Activity(_context,_authenticationService,auditContext);
            
            
        }

        public override IEnumerable<TCONTRACT> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TCONTRACT>();
            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                bool allUnit = true;
                List<string> authorizedUnitIds = new List<string>();

                //Authorization check
                //Get Authorization By User Name
                
                authorizedUnitIds = _authenticationService.GetAuthorizedLocationIdByUserName(filter.UserName, out allUnit);
                if (!allUnit)
                    criteria = criteria.And(p => authorizedUnitIds.Contains(p.UNITID));

            }
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(d => d.UNITID.Equals(filter.UnitID));
            if (filter.UnitIDs.Any())
                criteria = criteria.And(d => filter.UnitIDs.Contains(d.UNITID));
            if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => !d.STATUS.Equals(PMSConstants.TransactionStatusDeleted));
            else
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));
          

            criteria = criteria.And(d => ((d.STARTDATE.Date >= filter.StartDate.Date && d.STARTDATE.Date <= filter.EndDate.Date)
                    || (d.ENDDATE.Date >= filter.StartDate.Date && d.ENDDATE.Date <= filter.EndDate.Date)
                    || (d.STARTDATE.Date < filter.StartDate.Date && d.ENDDATE.Date > filter.EndDate.Date)
                   ));

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.CARDID.Contains(filter.Keyword)                        
                        || d.UNITID.Contains(filter.Keyword)
                        || d.NOTE.Contains(filter.Keyword)
                        || d.ID.Contains(filter.Keyword)
                    );

            if (filter.PageSize <= 0)
                return _context.TCONTRACT.Where(criteria);
            return _context.TCONTRACT.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }



        protected override TCONTRACT GetSingleFromDB(params  object[] keyValues)
        {
            
            string contractId = keyValues[0].ToString();
            
            var record = _context.TCONTRACT.Include(d => d.TCONTRACTITEM).SingleOrDefault(d => d.ID.Equals(contractId));
            
            
            if (record != null)
            {
                if (!string.IsNullOrWhiteSpace(CurrentUserName))
                {
                    if (!_authenticationService.IsAuthorizedUnit(CurrentUserName, record.UNITID))
                        throw new ExceptionNoUnitAccess(record.UNITID);
                }
                if (record.TCONTRACTITEM != null && record.TCONTRACTITEM.Any())
                {
                    var activityIds = record.TCONTRACTITEM.Select(d => d.ACTID).Distinct().ToList();
                    record.TCONTRACTITEM.Join(
                        _activityService.GetList(new FilterActivity { Ids = activityIds }),
                        a => a.ACTID,
                        b => b.ACTIVITYID,
                        (a, b) => new { a, ACTNAME = b.ACTIVITYNAME })
                        .ToList().ForEach(d =>
                        {
                            d.a.ACTNAME = $"{d.a.ACTID} - {d.ACTNAME}";
                        });
                }
                _context.Entry(record).State = EntityState.Detached;
            }

            return record;
        }

        //#add priyo 22042020# untuk UI upkeep borongan get item contract
        public IEnumerable<TCONTRACTITEM> GetContractItem(FilterContractItem filter)
        {
            
            try
            {
                var criteria = PredicateBuilder.True<TCONTRACTITEM>();
                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(d => d.ACTID.Equals(filter.Id));
                if (!string.IsNullOrWhiteSpace(filter.CONTID))
                    criteria = criteria.And(d => d.CONTID.Equals(filter.CONTID));
                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    criteria = criteria.And(d => d.ACTID.Contains(filter.Keyword));

                var record = _context.TCONTRACTITEM.Include(d => d.CONT).Where(criteria).ToList();
                if (record != null)
                {
                    var activityIds = record.Select(a => a.ACTID.ToString()).Distinct().ToList();
                    var _actIds = _activityService.GetList(new FilterActivity { Ids = activityIds }).ToList();
                    foreach (var item in record)
                    {
                        item.ACTNAME = _actIds.Where(a => a.ACTIVITYID.Equals(item.ACTID)).Select(b => b.ACTIVITYNAME).SingleOrDefault();
                    }
                }

                return record;
            }
            catch { throw; }
            
        }

        public override TCONTRACT CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TCONTRACT record = base.CopyFromWebFormData(formData,newRecord);//If Required Replace with your custom code
            /*Custom Code - Start*/
            List<TCONTRACTITEM> _potentialItems = new List<TCONTRACTITEM>();
            _potentialItems.CopyFrom<TCONTRACTITEM>(formData, "TCONTRACTITEM");
            _potentialItems.ForEach(d => d.CONTID = record.ID);
            List<string> _potentialIds = _potentialItems.Select(d => d.ACTID).ToList();

            _newContractItems = new List<TCONTRACTITEM>();
            _editedContractItems = new List<TCONTRACTITEM>();
            _deletedContractItems = new List<TCONTRACTITEM>();
            if (newRecord)
                _newContractItems = _potentialItems;
            else if (!_potentialIds.Any())
                _deletedContractItems = _context.TCONTRACTITEM.Where(x => x.CONTID.Equals(record.ID)).ToList();
            else
            {
                var existingItems = _context.TCONTRACTITEM.Where(x => x.CONTID.Equals(record.ID));
                var existingItemsId = existingItems.Select(x => x.ACTID).ToList();

                _newContractItems = _potentialItems.Where(o => !existingItemsId.Contains(o.ACTID)).ToList();
                _deletedContractItems = existingItems.Where(o => !_potentialIds.Contains(o.ACTID)).ToList();
                _editedContractItems = _potentialItems.Where(o => existingItemsId.Contains(o.ACTID)).ToList();
            }

            _saveDetails = _newContractItems.Any() || _deletedContractItems.Any() || _editedContractItems.Any();

            /*Custom Code - Here*/
            return record;
        }

       

        protected override TCONTRACT SaveInsertDetailsToDB(TCONTRACT record, string userName)
        {

            if (_newContractItems.Any())
            {
                _newContractItems.ForEach(d=>d.CONTID=record.ID);
                _context.TCONTRACTITEM.AddRange(_newContractItems);
            }
            return record;
        }

        protected override TCONTRACT BeforeSave(TCONTRACT record, string userName, bool newRecord)
        {
            if (!_authenticationService.IsAuthorizedUnit(userName, record.UNITID))
                throw new ExceptionNoUnitAccess(record.UNITID);
            if (newRecord)
            {
                _autoNumberPrefix = _serviceName + record.UNITID;
                int lastNumber = GetCurrentDocumentNumber();
                record.ID = record.UNITID + "-" + record.DATE.ToString("yyyyMM") + "-" + lastNumber.ToString("0000");
            }
            record.UPDATED = GetServerTime();
            record.NOTE = StandardUtility.IsNull<string>(record.NOTE, string.Empty);
            return record;
        }

        protected override TCONTRACT BeforeDelete(TCONTRACT record, string userName)
        {
            if (!_authenticationService.IsAuthorizedUnit(userName, record.UNITID))
                throw new ExceptionNoUnitAccess(record.UNITID);
            return base.BeforeDelete(record, userName);
        }

        protected override TCONTRACT AfterSave(TCONTRACT record, string userName, bool newRecord)
        {
            if (newRecord)
            {
                IncreaseRunningNumber();
                _autoNumberPrefix = string.Empty;
            }

            return GetSingle(record.ID);
        }

        

        
        
        protected override TCONTRACT SaveUpdateDetailsToDB(TCONTRACT record, string userName)
        {


            if (_deletedContractItems.Any())
            {
                _deletedContractItems.ForEach(d => d.CONTID = record.ID);
                _context.TCONTRACTITEM.RemoveRange(_deletedContractItems);
            }
            if (_newContractItems.Any())
            {
                _newContractItems.ForEach(d => d.CONTID = record.ID);
                _context.TCONTRACTITEM.AddRange(_newContractItems);
            }
            if (_editedContractItems.Any())
            {
                _editedContractItems.ForEach(d => d.CONTID = record.ID);
                _context.TCONTRACTITEM.UpdateRange(_editedContractItems);
            }
            return record;
        }

     

        protected override bool DeleteFromDB(TCONTRACT record, string userName)
        {

            record = GetSingle(record.ID);
            if (record == null)
                throw new Exception("Record not found");

            DateTime now = HelperService.GetServerDateTime(1, _context);
            record.STATUS = "D";
            record.UPDATED = now;
            _context.Entry<TCONTRACT>(record).State = EntityState.Modified;           
            _context.SaveChanges();
            return true;
        }

       
                
        

        

        public override TCONTRACT NewRecord(string userName)
        {
            TCONTRACT record = new TCONTRACT
            {
                ID = "AUTONUMBER",
                DATE = DateTime.Today,
                NOTE = string.Empty,
                STARTDATE = DateTime.Today,
                ENDDATE = DateTime.Today,
                TCONTRACTITEM = new List<TCONTRACTITEM>(),
                CLOSE = false,
                STATUS = "A"
            };
            return record;
        }

       
    }
}
