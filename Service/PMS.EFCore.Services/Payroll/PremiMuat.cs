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
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class PremiMuat : EntityFactory<MPREMIMUAT,MPREMIMUAT,GeneralFilter, PMSContextBase>
    {
        private Block _serviceBlock;
        private List<MPREMIMUATBLOCK> _premiMuatBlock = null;
        private AuthenticationServiceBase _authenticationService;
        public PremiMuat(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PremiMuat";
            _authenticationService = authenticationService;
            _serviceBlock = new Block(context,_authenticationService, auditContext);
            _premiMuatBlock = new List<MPREMIMUATBLOCK>();
        }

        private string FieldsValidation(MPREMIMUAT record)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.PREMIMUATID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.UNITCODE)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.ACTID)) result += "Kegiatan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.BASISGROUP)) result += "Group tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DESCRIPTION)) result += "Keterangan tidak boleh kosong." + Environment.NewLine;
            foreach(var block in record.MPREMIMUATBLOCK)
            {
                if (string.IsNullOrEmpty(block.PREMIMUATID)) result += "Id tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(block.BLOCKID)) result += "Blok tidak boleh kosong." + Environment.NewLine;
            }

            return result;
        }

        private void Validate(MPREMIMUAT PremiMuat)
        {
            string blockResult = string.Empty;
            if (_premiMuatBlock.Any())
            {
                foreach (var block in _premiMuatBlock)
                {
                    var existList = _context.MPREMIMUATBLOCK.Include(b => b.PREMIMUAT).
                                        Where(a => a.PREMIMUATID != block.PREMIMUATID && a.PREMIMUAT.UNITCODE == PremiMuat.UNITCODE
                                        && a.BLOCKID == block.BLOCKID && a.STATUS == PMSConstants.TransactionStatusApproved
                                        && a.PREMIMUAT.STATUS == PMSConstants.TransactionStatusApproved
                                        && a.PREMIMUAT.HARVESTTYPE == 2 && a.PREMIMUAT.EMPLOYEETYPE == "");

                    blockResult = existList.Aggregate(blockResult, (current, panen) => current + ("Blok " + block.BLOCKID + " sudah terdaftar di " + panen.PREMIMUAT.DESCRIPTION + "." + Environment.NewLine));
                }
            }

            if (!string.IsNullOrEmpty(blockResult))
                throw new Exception(blockResult);

            string result = this.FieldsValidation(PremiMuat);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        protected override MPREMIMUAT GetSingleFromDB(params object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.MPREMIMUAT.Include(b => b.MPREMIMUATBLOCK).Where(a => a.PREMIMUATID.Equals(Id)).SingleOrDefault();
        }

        public override MPREMIMUAT CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MPREMIMUAT record = base.CopyFromWebFormData(formData, newRecord);
            
            if (string.IsNullOrWhiteSpace(formData["EMPLOYEETYPE"]))
                record.EMPLOYEETYPE = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["VEHICLETYPEID"]))
                record.VEHICLETYPEID = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["DESCRIPTION"]))
                record.DESCRIPTION = string.Empty;

            /*Custom Code - Start*/
            List<MPREMIMUATBLOCK> premiBlock = new List<MPREMIMUATBLOCK>();
            premiBlock.CopyFrom<MPREMIMUATBLOCK>(formData, "MPREMIMUATBLOCK");
            _premiMuatBlock = premiBlock;

            _saveDetails = _premiMuatBlock.Any();

            return record; // base.CopyFromWebFormData(formData, newRecord, userName);
        }

        protected override MPREMIMUAT BeforeSave(MPREMIMUAT record, string userName,bool newRecord)
        {
            DateTime currentDate = GetServerTime();
            if (newRecord)
            {
                var id = this.GenereteNewNumber(record.UNITCODE, currentDate);
                record.PREMIMUATID = id;
                record.STATUS = PMSConstants.TransactionStatusApproved;
                record.CREATEBY = userName;
                record.CREATED = currentDate;
            }
            record.UPDATEBY = userName;
            record.UPDATED = currentDate;

            this.Validate(record);

            if (_premiMuatBlock.Any())
            {
                foreach (var block in _premiMuatBlock)
                {
                    block.PREMIMUATID = record.PREMIMUATID;
                    block.STATUS = PMSConstants.TransactionStatusApproved;
                }
            }

            return record;
        }

       

        private string GenereteNewNumber(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.PremiMuatIdPrefix + unitCode, _context);
            return PMSConstants.PremiMuatIdPrefix + unitCode + dateTime.ToString("yyyyMMdd") + lastNumber.ToString().PadLeft(4, '0');
        }

        //protected override MPREMIMUAT SaveInsertToDB(MPREMIMUAT record, string userName)
        //{
        //    return base.SaveInsertToDB(record, userName);
        //}

        protected override MPREMIMUAT SaveInsertDetailsToDB(MPREMIMUAT record, string userName)
        {
            if (_premiMuatBlock.Any())
                _context.MPREMIMUATBLOCK.AddRange(_premiMuatBlock);
            return record; // base.SaveInsertDetailsToDB(record, userName);
        }

        protected override MPREMIMUAT AfterSave(MPREMIMUAT record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.PremiMuatIdPrefix + record.UNITCODE, _context);
            return record; // base.AfterSaveInsert(record, userName);
        }

        protected override MPREMIMUAT SaveUpdateDetailsToDB(MPREMIMUAT record, string userName)
        {
            _context.MPREMIMUATBLOCK.RemoveRange(_context.MPREMIMUATBLOCK.Where(a => a.PREMIMUATID == record.PREMIMUATID));
            if (_premiMuatBlock.Any())
                _context.MPREMIMUATBLOCK.AddRange(_premiMuatBlock);
            return record; // base.SaveUpdateDetailsToDB(record, userName);
        }

        protected override MPREMIMUAT BeforeDelete(MPREMIMUAT record, string userName)
        {
            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();

            foreach (var block in _premiMuatBlock)
            {
                block.STATUS = PMSConstants.TransactionStatusDeleted;
            }

            return record; // base.BeforeDelete(record, userName);
        }

        protected override bool DeleteFromDB(MPREMIMUAT record, string userName)
        {
            _context.MPREMIMUAT.Update(record);
            if (_premiMuatBlock.Any())
                _context.MPREMIMUATBLOCK.UpdateRange(_premiMuatBlock);
            _context.SaveChanges();
            return true;// base.DeleteFromDB(record, userName);
        }

        public override IEnumerable<MPREMIMUAT> GetList(GeneralFilter filter)
        {            
            var criteria = PredicateBuilder.True<MPREMIMUAT>();
            if (filter != null)
            {
                if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS == PMSConstants.TransactionStatusApproved);
                else
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.PREMIMUATID.Equals(filter.Id));
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.PREMIMUATID));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));
                if (filter.UnitIDs.Any())
                    criteria = criteria.And(d => filter.UnitIDs.Contains(d.UNITCODE));

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    criteria = criteria.And(d =>
                        d.PREMIMUATID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        d.ACTID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        d.BASISGROUP.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        d.DESCRIPTION.ToLower().Contains(filter.LowerCasedSearchTerm));
            }

            if (filter.PageSize <= 0)
                return _context.MPREMIMUAT.Where(criteria);
            return _context.MPREMIMUAT.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public List<BLOCK> GetBlockCandidate(string unitCode, string code)
        {
            FilterBlock filter = new FilterBlock();
            filter.UnitID = unitCode;
            var list = _serviceBlock.GetList(filter);
            list = list.Where(e => e.ACTIVE == true && e.PHASE != 9 && e.PHASE != 0);//-*Constant

            if (code != string.Empty)
                list = list.Where(e => e.CODE == code);

            List<BLOCK> newList = new List<BLOCK>();
            
            foreach (var vblock in list)
            {
                BLOCK block = new BLOCK();
                block.CopyFrom(vblock);
                newList.Add(block);
            }

            return newList;
        }

        public MPREMIMUATBLOCK GetByBlock(string blockId, string employeeType)
        {
            return _context.MPREMIMUATBLOCK.Include(a => a.PREMIMUAT).Where(b => b.BLOCKID == blockId
                        && ( b.PREMIMUAT.EMPLOYEETYPE == employeeType || b.PREMIMUAT.EMPLOYEETYPE == "" )).SingleOrDefault();
        }

       
    }
}
