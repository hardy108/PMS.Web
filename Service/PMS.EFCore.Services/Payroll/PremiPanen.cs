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
    public class PremiPanen : EntityFactory<MPREMIPANEN,MPREMIPANEN,FilterPremiPanen, PMSContextBase>
    {
        private Block _serviceBlock;
        private List<MPREMIPANENBLOCK> _panenBlock = null;
        private AuthenticationServiceBase _authenticationService;
        public PremiPanen(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "PremiPanen";
            _authenticationService = authenticationService;
            _serviceBlock = new Block(context,_authenticationService, auditContext);
            _panenBlock = new List<MPREMIPANENBLOCK>();
        }

        private string FieldsValidation(MPREMIPANEN record)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.PREMIPANENID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.UNITCODE)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.ACTID)) result += "Kegiatan tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.BASISGROUP)) result += "Group tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DESCRIPTION)) result += "Keterangan tidak boleh kosong." + Environment.NewLine;
            //if (string.IsNullOrEmpty(record.EmployeeType)) result += "Tipe karyawan tidak boleh kosong." + Environment.NewLine;

            foreach (var block in record.MPREMIPANENBLOCK)
            {
                if (string.IsNullOrEmpty(block.PREMIPANENID)) result += "Id tidak boleh kosong." + Environment.NewLine;
                if (string.IsNullOrEmpty(block.BLOCKID)) result += "Blok tidak boleh kosong." + Environment.NewLine;
            }

            return result;
        }

        public override MPREMIPANEN CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            MPREMIPANEN record = base.CopyFromWebFormData(formData, newRecord);
            record.MPREMIPANENBLOCK.Clear();
            /*Custom Code - Start*/
            List<MPREMIPANENBLOCK> premiBlock = new List<MPREMIPANENBLOCK>();
            premiBlock.CopyFrom<MPREMIPANENBLOCK>(formData, "MPREMIPANENBLOCK");
            _panenBlock = premiBlock;

            _panenBlock.ForEach(d => { record.MPREMIPANENBLOCK.Add(d); });
            
            return record; // base.CopyFromWebFormData(formData, newRecord);
        }

        protected override MPREMIPANEN BeforeSave(MPREMIPANEN record, string userName, bool newRecord)
        {
            _panenBlock.Clear();

            foreach(var block in record.MPREMIPANENBLOCK)
            {
                _panenBlock.Add(block);
            }
            _saveDetails = _panenBlock.Any();
            record.MPREMIPANENBLOCK.Clear();

            if (record.EMPLOYEETYPE == null)
                record.EMPLOYEETYPE = "";

            DateTime currentDate = GetServerTime();
            if (newRecord)
            {
                var id = this.GenereteNewNumber(record.UNITCODE, currentDate);
                record.PREMIPANENID = id;
                record.STATUS = PMSConstants.TransactionStatusApproved;
                record.CREATEBY = userName;
                record.CREATED = currentDate;
            }
            record.UPDATEBY = userName;
            record.UPDATED = currentDate;

            foreach (var block in record.MPREMIPANENBLOCK)
            {
                block.PREMIPANENID = record.PREMIPANENID;
                block.STATUS = PMSConstants.TransactionStatusApproved;
            }
            this.Validate(record);


            return record;
        }

       
        private string GenereteNewNumber(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.PremiPanenIdPrefix + unitCode, _context);
            return PMSConstants.PremiPanenIdPrefix + unitCode + dateTime.ToString("yyyyMMdd") + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override MPREMIPANEN SaveInsertDetailsToDB(MPREMIPANEN record, string userName)
        {
            if (_panenBlock.Any())
                _context.MPREMIPANENBLOCK.AddRange(_panenBlock);
            return record; // return base.SaveInsertDetailsToDB(record, userName);
        }

        protected override MPREMIPANEN AfterSave(MPREMIPANEN record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.PremiPanenIdPrefix + record.UNITCODE, _context);
            return record; // base.AfterSaveInsert(record, userName);
        }

        protected override MPREMIPANEN SaveUpdateDetailsToDB(MPREMIPANEN record, string userName)
        {
            _context.MPREMIPANENBLOCK.RemoveRange(_context.MPREMIPANENBLOCK.Where(a => a.PREMIPANENID == record.PREMIPANENID));
            if (_panenBlock.Any())
                _context.MPREMIPANENBLOCK.AddRange(_panenBlock);
            return record; // base.SaveUpdateDetailsToDB(record, userName);
        }

        protected override MPREMIPANEN BeforeDelete(MPREMIPANEN record, string userName)
        {
            record.STATUS = PMSConstants.TransactionStatusDeleted;
            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();

            _panenBlock.Clear();
            foreach (var block in _panenBlock)
            {
                block.STATUS = PMSConstants.TransactionStatusDeleted;
                _panenBlock.Add(block);
            }
            record.MPREMIPANENBLOCK.Clear();

            return record; // base.BeforeDelete(record, userName);
        }

        protected override bool DeleteFromDB(MPREMIPANEN record, string userName)
        {
            if (_panenBlock.Any())
            {
                _context.MPREMIPANENBLOCK.UpdateRange(_panenBlock);
                //_context.SaveChanges();
            }

            _context.Entry<MPREMIPANEN>(record).State = EntityState.Modified;
            _context.SaveChanges();
            return true; // base.DeleteFromDB(record, userName);
        }

        private void Validate(MPREMIPANEN premiPanen)
        {
            string blockResult = string.Empty;
            foreach (var block in premiPanen.MPREMIPANENBLOCK)
            {
                //var existList = _context.MPREMIPANENBLOCK.Include(b => b.PREMIPANEN).
                //                    Where(a => a.PREMIPANENID != block.PREMIPANENID && a.PREMIPANEN.UNITCODE == premiPanen.UNITCODE
                //                    && a.BLOCKID == block.BLOCKID && a.STATUS == "A" && a.PREMIPANEN.STATUS == "A"
                //                    && a.PREMIPANEN.HARVESTTYPE == 2 && a.PREMIPANEN.EMPLOYEETYPE == "");

                //blockResult = existList.Aggregate(blockResult, (current, panen) => current + ("Blok " + block.BLOCKID + " sudah terdaftar di " + panen.PREMIPANEN.DESCRIPTION + "." + Environment.NewLine));

                MPREMIPANEN existList = (from H in _context.MPREMIPANEN
                                join D in _context.MPREMIPANENBLOCK
                                on H.PREMIPANENID equals D.PREMIPANENID
                                where ( H.PREMIPANENID.ToString() != premiPanen.PREMIPANENID.ToString()
                                && H.STATUS == "A"
                                && H.UNITCODE.Equals(premiPanen.UNITCODE)
                                && H.ACTID.Equals(premiPanen.ACTID)
                                //&& H.HARVESTTYPE.Equals(premiPanen.HARVESTTYPE)
                                && (H.EMPLOYEETYPE.Equals(premiPanen.EMPLOYEETYPE) || H.EMPLOYEETYPE == "")
                                && D.STATUS == "A" && D.BLOCKID.Equals(block.BLOCKID)
                                )select H).FirstOrDefault();

                if (existList !=null) blockResult+= "Blok " + block.BLOCKID + " sudah terdaftar di " + existList.BASISGROUP  + "." + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(blockResult))
                throw new Exception(blockResult);

            string result = this.FieldsValidation(premiPanen);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        protected override MPREMIPANEN GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.MPREMIPANEN.Include(b => b.MPREMIPANENBLOCK).Where(a => a.PREMIPANENID.Equals(Id)).SingleOrDefault();
        }

        public override IEnumerable<MPREMIPANEN> GetList(FilterPremiPanen filter)
        {
            
            var criteria = PredicateBuilder.True<MPREMIPANEN>();
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.PREMIPANENID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.BASISGROUP.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.DESCRIPTION.ToLower().Contains(filter.LowerCasedSearchTerm) 
                    );
                }

                if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS == PMSConstants.TransactionStatusApproved);
                else
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.PREMIPANENID.Equals(filter.Id));

                if (filter.Ids.Any())
                    criteria = criteria.And(p => filter.Ids.Contains(p.PREMIPANENID));


                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));
                if (filter.UnitIDs.Any())
                    criteria = criteria.And(p => filter.UnitIDs.Contains(p.UNITCODE));

                if (!string.IsNullOrWhiteSpace(filter.EmpoyeeType))
                    criteria = criteria.And(p => p.EMPLOYEETYPE.Equals(filter.EmpoyeeType));
                if (filter.EmployeeTypes.Any())
                    criteria = criteria.And(p => filter.EmployeeTypes.Contains(p.EMPLOYEETYPE));

                //if (!string.IsNullOrWhiteSpace(filter.Keyword))
                //    criteria = criteria.And(d => d.PREMIPANENID.Contains(filter.Keyword) || d.ACTID.Contains(filter.Keyword));

            }

            if (filter.PageSize <= 0)
                return _context.MPREMIPANEN.Where(criteria);
            return _context.MPREMIPANEN.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
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

        public MPREMIPANENBLOCK GetByBlock(string blockId, string employeeType)
        {
            return _context.MPREMIPANENBLOCK.Include(a => a.PREMIPANEN).Where(b => b.BLOCKID == blockId
                        && b.PREMIPANEN.EMPLOYEETYPE == employeeType || b.PREMIPANEN.EMPLOYEETYPE == "" ).SingleOrDefault();

        }

        public IEnumerable<MPREMIPANENBLOCK> GetListPremiPanenBlock(FilterPremiPanen filter)
        {
            
            var criteria = PredicateBuilder.True<MPREMIPANENBLOCK>();
             if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(p => p.PREMIPANEN.STATUS == PMSConstants.TransactionStatusApproved);
            else
                criteria = criteria.And(p => p.PREMIPANEN.STATUS.Equals(filter.RecordStatus));

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.PREMIPANENID.Equals(filter.Id));

            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains(p.PREMIPANENID));


            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(p => p.PREMIPANEN.UNITCODE.Equals(filter.UnitID));
            if (filter.UnitIDs.Any())
                criteria = criteria.And(p => filter.UnitIDs.Contains(p.PREMIPANEN.UNITCODE));

            if (!string.IsNullOrWhiteSpace(filter.EmpoyeeType))
                criteria = criteria.And(p => p.PREMIPANEN.EMPLOYEETYPE.Equals(filter.EmpoyeeType));
            if (filter.EmployeeTypes.Any())
                criteria = criteria.And(p => filter.EmployeeTypes.Contains(p.PREMIPANEN.EMPLOYEETYPE));


            if (!string.IsNullOrWhiteSpace(filter.BlockID) || filter.BlockIDs.Any())
            {

                FilterBlock filterBlock = new FilterBlock { BlockID = filter.BlockID };

                if (filter.BlockIDs.Any())
                    filterBlock.BlockIDs = filter.BlockIDs;

                List<string> unitIds = _serviceBlock.GetList(filterBlock).Select(d => d.DIVID.Substring(0, 4)).Distinct().ToList();



                if (string.IsNullOrWhiteSpace(filter.BlockID))
                    criteria = criteria.And(d => d.BLOCKID.Equals(filter.BlockID));
                if (filter.BlockIDs.Any())
                    criteria = criteria.And(d => filter.BlockIDs.Contains(d.BLOCKID));
                criteria = criteria.And(d => unitIds.Contains(d.PREMIPANEN.UNITCODE));

            }

            if (filter.PageSize<=0)
                return _context.MPREMIPANENBLOCK.Include(d=>d.PREMIPANEN).Where(criteria);
            return _context.MPREMIPANENBLOCK.Include(d => d.PREMIPANEN).Where(criteria).GetPaged(filter.PageNo,filter.PageSize).Results;   

        }

        public override MPREMIPANEN NewRecord(string userName)
        {
            MPREMIPANEN record = new MPREMIPANEN();
            record.STATUS = "";

            return record;
        }

    }
}
