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
    public class Block:EntityFactory<VBLOCK,VBLOCK, GeneralFilter, PMSContextBase>
    {
        Divisi _divisiService;
        private AuthenticationServiceBase _authenticationService;
        public Block(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Block";
            _authenticationService = authenticationService;
            _divisiService = new Divisi(_context,_authenticationService, auditContext);
            
        }

        private void InsertValidate(VBLOCK record, string userName)
        {
            Validate(record, userName);

            string result = string.Empty;
            if (_context.VBLOCK.SingleOrDefault(d => d.BLOCKID.Equals(record.BLOCKID)) != null) result += "Kode Blok sudah terdaftar.";
            if (result != string.Empty)
                throw new Exception(result);
        }

        private void Validate(VBLOCK record, string userName)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.CODE)) result += "Kode harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(record.DIVID)) result += "Divisi harus diisi." + Environment.NewLine;
            if (record.PHASE == -1) result += "Phase harus diisi." + Environment.NewLine;

            var blockExist = _context.VBLOCK.SingleOrDefault(d => d.BLOCKID.Equals(record.BLOCKID));
            if (blockExist != null)
                if (blockExist.EFFDATE >= record.EFFDATE)
                    throw new Exception("Tanggal efektif harus lebih besar dari tanggal sebelumnya.");
            if (result != string.Empty)
                throw new Exception(result);
        }

        public override VBLOCK NewRecord(string userName)
        {
            short month = Convert.ToInt16(DateTime.Now.Month);
            short year = Convert.ToInt16(DateTime.Now.Year);

            VBLOCK record = new VBLOCK
            {
                BLNTANAM = month,
                THNTANAM = year,
                EFFDATE = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day-1)
            };
            return record;
        }

        protected override VBLOCK GetSingleFromDB(params  object[] keyValues)
        {
            string blockId = keyValues[0].ToString();
            return _context.VBLOCK.SingleOrDefault(d => d.BLOCKID.Equals(blockId));
        }

        public override VBLOCK CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            VBLOCK record = base.CopyFromWebFormData(formData, newRecord);

            if (string.IsNullOrWhiteSpace(formData["JENISBIBIT"]))
                record.JENISBIBIT = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["TOPOGRAPI"]))
                record.TOPOGRAPI = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["KELASTANAH"]))
                record.KELASTANAH = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["COSTCTR"]))
                record.COSTCTR = string.Empty;

            if (string.IsNullOrWhiteSpace(formData["WBS"]))
                record.WBS = string.Empty;

            record.LUASBLOCK = Convert.ToDecimal(formData["LUASBLOCK"]);
            record.CURRENTPLANTED = Convert.ToDecimal(formData["CURRENTPLANTED"]);
            record.BJR = Convert.ToDecimal(formData["BJR"]);
            record.SPH = Convert.ToInt16(formData["SPH"]);

            return record;
        }

        protected override VBLOCK BeforeSave(VBLOCK record, string userName, bool newRecord)
        {
            record.TYPE = "BLK";
            record.JENISBIBIT = StandardUtility.IsNull<string>(record.JENISBIBIT, string.Empty);
            record.TOPOGRAPI = StandardUtility.IsNull<string>(record.TOPOGRAPI, string.Empty);
            record.KELASTANAH = StandardUtility.IsNull<string>(record.KELASTANAH, string.Empty);
            record.COSTCTR = StandardUtility.IsNull<string>(record.COSTCTR, string.Empty);
            record.WBS = StandardUtility.IsNull<string>(record.WBS, string.Empty);

            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                var newId = record.DIVID + "-" + record.NAME;
                record.BLOCKID = newId;
                record.ID = newId;
                record.CODE = record.NAME;
                record.ACTIVE = true;
                record.CREATED = now;
                record.CREATEBY = userName;
                this.InsertValidate(record, userName);
            }
            else
            {
                this.Validate(record, userName);
            }
            record.UPDATED = now;
            record.UPDATEBY = userName;
            return record;
        }

        
        protected override VBLOCK AfterSave(VBLOCK record, string userName, bool newRecord)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            HelperService.DHSUpdateMaster(userName, now, _serviceName, _context);
            return record;
        }
        

        

        

        private MORGANIZATION ConvertToOrganization(VBLOCK block)
        {
            MORGANIZATION organization = new MORGANIZATION();
            organization.CopyFrom(block);
            return organization;
        }

        private MBLOCK ConvertToBlock(VBLOCK vblock)
        {
            MBLOCK block = new MBLOCK();
            block.CopyFrom(vblock);
            return block;
        }

        protected override VBLOCK SaveInsertToDB(VBLOCK record, string userName)
        {
            _context.MORGANIZATION.Add(ConvertToOrganization(record));
            _context.MBLOCK.Add(ConvertToBlock(record));

            MBLOCKHIST blockHistory = new MBLOCKHIST();
            blockHistory.CopyFrom(record);
            blockHistory.STARTDATE = record.EFFDATE;
            blockHistory.ENDDATE = null;
            _context.MBLOCKHIST.Add(blockHistory);

            _context.SaveChanges();
            return GetSingle(record.BLOCKID);
        }

        protected override VBLOCK SaveUpdateToDB(VBLOCK record, string userName)
        {
            _context.MORGANIZATION.Update(ConvertToOrganization(record));
            _context.MBLOCK.Update(ConvertToBlock(record));

            MBLOCKHIST blockHistory = (from p in _context.MBLOCKHIST
                                    where p.ENDDATE == null && p.BLOCKID == record.BLOCKID 
                                    select p).SingleOrDefault();

            blockHistory.ENDDATE = record.EFFDATE.AddDays(-1);
            _context.MBLOCKHIST.Update(blockHistory);
            _context.SaveChanges();

            blockHistory.BLNTANAM = record.BLNTANAM;
            blockHistory.THNTANAM = record.THNTANAM;
            blockHistory.JENISBIBIT = record.JENISBIBIT;
            blockHistory.LUASBLOCK = record.LUASBLOCK;
            blockHistory.CURRENTPLANTED = record.CURRENTPLANTED;
            blockHistory.BJR = record.BJR;
            blockHistory.TOPOGRAPI = record.TOPOGRAPI;
            blockHistory.KELASTANAH = record.KELASTANAH;
            blockHistory.COSTCTR = record.COSTCTR;
            blockHistory.WBS = record.WBS;
            blockHistory.SPH = record.SPH;
            blockHistory.PHASE = record.PHASE;
            blockHistory.STARTDATE = record.EFFDATE;
            blockHistory.ENDDATE = null;
            _context.MBLOCKHIST.Add(blockHistory);
            _context.SaveChanges();

            return GetSingle(record.BLOCKID);
        }

        //protected override bool DeleteFromDB(VBLOCK record, string userName)
        //{
        //    record = GetSingle(record.BLOCKID);
        //    if (record == null)
        //        throw new Exception("Record not found");

        //    DateTime now = HelperService.GetServerDateTime(1, _context);
        //    //record.ACTIVE = false;
        //    //record.UPDATED = now;
        //    //record.UPDATEBY = userName;

        //    SaveUpdateToDB(record, userName);
        //    _context.Entry<VBLOCK>(record).State = EntityState.Deleted;

        //    Security.Audit.Insert(userName, _serviceName, now, $"Delete Block {record.ID}", _context);
        //    _context.SaveChanges();
        //    return true;
        //}

        #region       
        public override IEnumerable<VBLOCK> GetList(GeneralFilter filter)
        {
            var criteria = PredicateBuilder.True<VBLOCK>();

            if (!string.IsNullOrWhiteSpace(filter.UserName))            
                criteria = criteria.And(_authenticationService.GetFilterBlockByUserName(filter.UserName,filter.UnitID,filter.DivisionID,filter.Id));
                
            

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p => p.NAME.ToLower().Contains(filter.LowerCasedSearchTerm)
                    || p.DIVID.ToLower().Contains(filter.LowerCasedSearchTerm)
                    || p.BLOCKID.ToLower().Contains(filter.LowerCasedSearchTerm));
            }
            
            if (filter.IsActive.HasValue)            
                criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

            //Change By Hardi 2020-04-30 filter by UnitCode
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(p => p.DIVID.StartsWith(filter.UnitID));

            if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                criteria = criteria.And(p => p.DIVID.Equals(filter.DivisionID));

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.BLOCKID.Equals(filter.Id));
            //Added By Junaidi 2020-03-29 - Start
            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains(p.BLOCKID));
            //Added By Junaidi 2020-03-29 - End

            //Get All Units
            if (filter.PageSize<=0)
                return _context.VBLOCK.Where(criteria);
            return _context.VBLOCK.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        public VDIVISI GetParent( string blockId)
        {
            VBLOCK block = GetSingle(blockId);
            if (block == null)
                return null;
            return _context.VDIVISI.SingleOrDefault(d => d.DIVID.Equals(block.DIVID));
        }

        public List<VBLOCK> GetListNurseryBatch(string unitID)
        {

            var criteriaDivisi = PredicateBuilder.True<VDIVISI>();
            var criteriaBlock = PredicateBuilder.True<VBLOCK>();
            if (!string.IsNullOrWhiteSpace(unitID)) 
            {
                criteriaDivisi = criteriaDivisi.And(d => d.UNITCODE.Equals(unitID));
                criteriaBlock = criteriaBlock.And(d => d.UNITCODE.Equals(unitID));
            }
            criteriaDivisi = criteriaDivisi.And(d => d.CODE.Equals("Z"));
            criteriaBlock = criteriaBlock.And(d => d.PHASE == 0);

            return
            (
                from a in _context.VBLOCK.Where(criteriaBlock)
                join b in _context.VDIVISI.Where(criteriaDivisi) on a.DIVID equals b.DIVID
                select a
            ).ToList();
        }
        #endregion
    }
}
