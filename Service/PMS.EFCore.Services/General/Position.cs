using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using PMS.EFCore.Helper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{
    public class Position:EntityFactory<MPOSITION,MPOSITION,FilterPosition, PMSContextBase>
    {
        private List<MPOSITIONDETAIL> _newDetails = new List<MPOSITIONDETAIL>();
        private List<MPOSITIONDETAIL> _deletedDetails = new List<MPOSITIONDETAIL>();
        private List<MPOSITIONDETAIL> _editedDetails = new List<MPOSITIONDETAIL>();
        private AuthenticationServiceBase _authenticationService;
        public Position(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Position";
            _authenticationService = authenticationService;
        }

        protected override MPOSITION GetSingleFromDB(params  object[] keyValues)
        {
            string posId = keyValues[0].ToString();
            return _context.MPOSITION
                .Include(d => d.GROUP)
                .Include(d=>d.MPOSITIONDETAIL)
                .SingleOrDefault(d => d.POSITIONID.Equals(posId));
        }

        public override IEnumerable<MPOSITION> GetList(FilterPosition filter)
        {
            var criteria = PredicateBuilder.True<MPOSITION>();
            
                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);                    

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.POSITIONNAME.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.POSITIONID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        (p.GROUP != null && p.GROUP.NAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                        )
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.POSITIONID.Equals(filter.Id));
                if (filter.Ids.Any())
                    criteria = criteria.And(p => filter.Ids.Contains(p.POSITIONID));

                if (filter.PositionGroupId !=0 )
                    criteria = criteria.And(p => p.GROUPID == filter.PositionGroupId);
                if (!string.IsNullOrWhiteSpace(filter.PositionGroupName))
                    criteria = criteria.And(p => p.GROUP != null && p.GROUP.NAME.Equals(filter.PositionGroupName));

                if (!filter.AllPostitionFlag)                
                    criteria = criteria.And(p => filter.PositionFlags.Contains(p.POSFLAG));

            if (filter.PageSize <= 0)
                return _context.MPOSITION.Include(d => d.GROUP).Where(criteria);
            return _context.MPOSITION.Include(d => d.GROUP).Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public override MPOSITION CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MPOSITION record = base.CopyFromWebFormData(formData, newRecord);
            List<MPOSITIONDETAIL> _potentialDetails = new List<MPOSITIONDETAIL>();
            _potentialDetails.CopyFrom<MPOSITIONDETAIL>(formData, "MPOSITIONDETAIL");
            _potentialDetails.ForEach(d => d.POSID = record.POSITIONID);
            List<string> _potentialDetailUnitIds = _potentialDetails.Select(d => d.UNITID).ToList();
            _newDetails = new List<MPOSITIONDETAIL>();
            _editedDetails = new List<MPOSITIONDETAIL>();
            _editedDetails = new List<MPOSITIONDETAIL>();

            if (newRecord)
            {
                _newDetails = _potentialDetails;
            }
            else
            {
                if (!_potentialDetailUnitIds.Any())
                    _deletedDetails = _context.MPOSITIONDETAIL.Where(x => x.POSID.Equals(record.POSITIONID)).ToList();
                else
                {
                    var existingItems = _context.MPOSITIONDETAIL.Where(x => x.POSID.Equals(record.POSITIONID));
                    var existingItemsId = existingItems.Select(x => x.UNITID).ToList();

                    _newDetails = _potentialDetails.Where(o => !existingItemsId.Contains(o.UNITID)).ToList();
                    _deletedDetails = existingItems.Where(o => !_potentialDetailUnitIds.Contains(o.UNITID)).ToList();
                    _editedDetails = _potentialDetails.Where(o => existingItemsId.Contains(o.UNITID)).ToList();
                }                
            }

            _saveDetails = _editedDetails.Any() || _deletedDetails.Any() || _newDetails.Any();

            return record;
        }

        protected override MPOSITION BeforeSave(MPOSITION record, string userName, bool newRecord)
        {
            string errrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(record.POSITIONID))
                errrorMessage += "Kode tidak boleh kosong.\r\n";
            if (string.IsNullOrWhiteSpace(record.POSITIONNAME))
                errrorMessage += "Nama tidak boleh kosong.\r\n";
            MPOSITIONGROUP group = _context.MPOSITIONGROUP.Find(record.GROUPID);
            if (group == null)
                errrorMessage += "Kelompok tidak valid.";
            if (!string.IsNullOrWhiteSpace(errrorMessage))
                throw new Exception(errrorMessage);

            if (_newDetails.Any())
                _newDetails.ForEach(d => d.POSID = record.POSITIONID);
            
            if (_editedDetails.Any())
                _editedDetails.ForEach(d => d.POSID = record.POSITIONID);

            if (_deletedDetails.Any())
                _deletedDetails.ForEach(d => d.POSID = record.POSITIONID);
            DateTime serverTime = GetServerTime();
            if (newRecord)
            {
                record.CREATEBY = userName;
                record.CREATED = serverTime;
            }
            record.UPDATEBY = userName;
            record.UPDATED = serverTime;
            
            return record;
        }

       

       
        protected override MPOSITION SaveInsertDetailsToDB(MPOSITION record, string userName)
        {
            if (_newDetails.Any())
                _context.MPOSITIONDETAIL.AddRange(_newDetails);
            return record;
        }

        protected override MPOSITION SaveUpdateDetailsToDB(MPOSITION record, string userName)
        {
            if (_deletedDetails.Any())
                _context.MPOSITIONDETAIL.RemoveRange(_deletedDetails);
            if (_newDetails.Any())
                _context.MPOSITIONDETAIL.AddRange(_newDetails);
            if (_editedDetails.Any())
                _context.MPOSITIONDETAIL.UpdateRange(_editedDetails);
            return record;
        }

        protected override bool DeleteDetailsFromDB(MPOSITION record, string userName)
        {
            if (_deletedDetails.Any())
                _context.MPOSITIONDETAIL.RemoveRange(_deletedDetails);
            return true; ;
        }
    }
}
