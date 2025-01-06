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
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;
using Newtonsoft.Json;
using PMS.EFCore.Services.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class HarvestingBlockResult : EntityFactory<THARVESTBLOCKRESULT, THARVESTBLOCKRESULT, FilterHarvestBlockResult,PMSContextBase>
    {
        private Period _servicePeriod;
        private Block _serviceBlock;
        private SPB _serviceSPB;
        AuthenticationServiceBase _authenticationService;
        public HarvestingBlockResult(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "HarvestingBlockResult";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(_context, _authenticationService, auditContext);
            _serviceBlock = new Block(_context, _authenticationService, auditContext);
            _serviceSPB = new SPB(_context, _authenticationService, auditContext);
        }

        public List<String> GetServerMill()
        {
            List<string> ServerList = new List<string>();

            try
            {

                var sourceList =  HelperService.GetConfigValue("HARVESTTONASESOURCE",_context).Split(new[] { '#' });
                foreach (var source in sourceList) ServerList.Add(source);
                return ServerList;
            }

            catch { return ServerList; }
        }

        private void DeleteValidate(string unitId, THARVESTBLOCKRESULT blockResult)
        {
            if (blockResult.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (blockResult.STATUS == "C")
                throw new Exception("Data sudah di cancel.");

            _servicePeriod.CheckValidPeriod(unitId, blockResult.DATE.Date);
        }

        private string FieldsValidation(THARVESTBLOCKRESULT blockResult)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(blockResult.ID)) result += "Id tidak boleh kosong." + Environment.NewLine;
            if (blockResult.DATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(blockResult.BLOCKID)) result += "Blok tidak boleh kosong." + Environment.NewLine;
            if (blockResult.STATUS == string.Empty) result += "Status tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        private void Validate(THARVESTBLOCKRESULT blockResult)
        {
            string result = this.FieldsValidation(blockResult);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);
        }

        private void InsertValidate(THARVESTBLOCKRESULT blockResult)
        {
            this.Validate(blockResult);
            var resultExist = Get(blockResult.SOURCE, blockResult.NOSPB, blockResult.DATE, blockResult.BLOCKID);
            if (resultExist != null) throw new Exception("Hasil dengan id " + blockResult.ID + " sudah terdaftar.");

            resultExist = Get(blockResult.SOURCE, blockResult.NOSPB, blockResult.DATE, blockResult.BLOCKID);
            if (resultExist != null) throw new Exception("Hasil untuk blok " + blockResult.BLOCKID + " pada tanggal " + blockResult.DATE.ToString("dd/MM/yyyy") + " dari " + blockResult.SOURCE + " sudah terdaftar.");
        }

        private void UpdateValidate(THARVESTBLOCKRESULT blockResult)
        {
            var curResult = Get(blockResult.SOURCE,blockResult.NOSPB,blockResult.DATE,blockResult.BLOCKID);
            if (curResult.STATUS == "D")
                throw new Exception("Data sudah di hapus.");
            if (curResult.STATUS == "C")
                throw new Exception("Data sudah di cancel.");

            this.Validate(blockResult);
        }

        protected override THARVESTBLOCKRESULT BeforeSave(THARVESTBLOCKRESULT record, string userName, bool newRecord)
        {
            record.UPDATED = GetServerTime();

            var block = _context.MBLOCK.Where(d=> d.BLOCKID.Equals(record.BLOCKID)).SingleOrDefault();
            var divisi = _context.MDIVISI.Where(d => d.DIVID.Equals(block.DIVID)).SingleOrDefault();
            _servicePeriod.CheckValidPeriod(divisi.UNITCODE, record.DATE.Date);

            return record;
        }

        public List<THARVESTBLOCKRESULT> Get(DateTime date, string blockId)
        {
            return _context.THARVESTBLOCKRESULT.Where
                (
                d => d.DATE.Year == date.Year && d.DATE.Month == date.Month && d.DATE.Day == date.Day &&
                d.BLOCKID.Equals(blockId)
                ).ToList();
        }

        public THARVESTBLOCKRESULT Get(string source, string nospb, DateTime date, string blockId)
        {
            return
                _context.THARVESTBLOCKRESULT.SingleOrDefault
                (
                d => d.SOURCE.Equals(source) && d.NOSPB.Equals(nospb) &&
                d.DATE.Year == date.Year && d.DATE.Month == date.Month && d.DATE.Day == date.Day &&
                d.BLOCKID.Equals(blockId)
                );
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            DateTime date = Convert.ToDateTime(formDataCollection["DATE"]);
            string blockId = formDataCollection["BLOCKID"];

            var list = Get(date, blockId);
            foreach (var item in list)
            {
                item.STATUS = "A";
                item.UPDATED = GetServerTime();

                _context.Entry(list).State = EntityState.Modified;
                _context.SaveChanges();

                Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Calculate {item.ID}", _context);
            }

            return true;
        }

        public override IEnumerable<THARVESTBLOCKRESULT> GetList(FilterHarvestBlockResult filter)
        {
            
            var criteria = PredicateBuilder.True<THARVESTBLOCKRESULT>();

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.ID.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.SOURCE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                        p.NOSPB.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.ID.Equals(filter.Id));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.BLOCKID.StartsWith(filter.UnitID));

                if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                    criteria = criteria.And(p => p.BLOCKID.StartsWith(filter.DivisionID));

                if (!string.IsNullOrWhiteSpace(filter.Source))
                    criteria = criteria.And(p => p.SOURCE.Equals(filter.Source));

                if (!string.IsNullOrWhiteSpace(filter.NoSPB))
                    criteria = criteria.And(p => p.NOSPB.Equals(filter.NoSPB));

                if (!string.IsNullOrWhiteSpace(filter.VehId))
                    criteria = criteria.And(p => p.VEHID.Equals(filter.VehId));

                if (!string.IsNullOrWhiteSpace(filter.Driver))
                    criteria = criteria.And(p => p.DRIVER.Equals(filter.Driver));

                if (!string.IsNullOrWhiteSpace(filter.BlockId))
                    criteria = criteria.And(p => p.BLOCKID.Equals(filter.BlockId));

                if (!filter.Date.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.DATE.Date == filter.Date.Date);

                if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.DATE.Date >= filter.StartDate.Date && p.DATE.Date <= filter.EndDate.Date);

                if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                IEnumerable<THARVESTBLOCKRESULT> record = _context.THARVESTBLOCKRESULT
                    .Where(criteria)
                    .ToList();

                foreach(var data in record)
                {
                    data.BLOCKDETAIL = _context.MBLOCK.Where(d => d.BLOCKID.Equals(data.BLOCKID)).SingleOrDefault();
                }

                return record;

                //return _context.THARVESTBLOCKRESULT
                //    .Where(criteria)
                //    .ToList();
            }
            catch { return new List<THARVESTBLOCKRESULT>(); }

        }

        public IEnumerable<SPBKG>GetListSPB(FilterHarvestBlockResult filter)
        {
            int sDay = 0;
            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                
                List<string> authorizedUnitIds = new List<string>();

                authorizedUnitIds = _authenticationService.GetAuthorizedUnitByUserName(filter.UserName,string.Empty).Select(d => d.UNITCODE).ToList();
                sDay = Convert.ToInt16(HelperService.GetConfigValue(PMSConstants.CfgTransactionMaxSPBList + authorizedUnitIds[0], _context));
            }

            filter.StartDate = GetServerTime().Date.AddDays(sDay * -1);
            filter.EndDate = GetServerTime().Date;

            var criteria = PredicateBuilder.True<SPBKG>();
            try
            {
                if (filter.Ids.Any())
                    criteria = criteria.And(p => filter.Ids.Contains(p.SPBID));

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.SPBID.ToLower().Contains(filter.LowerCasedSearchTerm) 
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.BlockId))
                    criteria = criteria.And(p => p.SPBID.Equals(filter.NoSPB));

                if (!filter.Date.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.DATEIN.Date == filter.Date.Date);

                if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.DATEIN.Date >= filter.StartDate.Date && p.DATEIN.Date <= filter.EndDate.Date);

                if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (filter.PageSize <= 0)
                    return _context.SPBKG
                        .Where(criteria);

                return _context.SPBKG
                    .Where(criteria)
                    .GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return new List<SPBKG>(); }

        }

        public bool DownloadDataSPB(string source, string unitId, DateTime from, DateTime to, string Username)
        {
            _servicePeriod.CheckValidPeriod(unitId, from.Date);
            _servicePeriod.CheckValidPeriod(unitId, to.Date);

            THARVESTBLOCKRESULT result;
            List<THARVESTBLOCKRESULT> resultlist = new List<THARVESTBLOCKRESULT>();

            if (source == "SPB" || source == string.Empty || source == null)
            {
                var dataresult = _serviceSPB.GetTonaseBlock(unitId, from.Date, to.Date);
                foreach (var item in dataresult)
                {
                    result = new THARVESTBLOCKRESULT();
                    result.CopyFrom(item);
                    result.SOURCE = "SPB";
                    result.ID = result.NOSPB;
                    result.BLOCKID = item.BLOCK;
                    result.AUTO = true;
                    result.STATUS = "P";
                    result.UPDATED = GetServerTime();
                    resultlist.Add(result);
                }
            }
            else
            {
                MUNITDBSERVER unitDBServer = _context.MUNITDBSERVER.Where(d=> d.ALIAS.Equals(source)).SingleOrDefault();
                if (unitDBServer == null)
                throw new Exception("Invalid DB Server");
                {
                    PMSContextBase contextWB = new PMSContextBase(DBContextOption<PMSContextBase>.GetOptions(unitDBServer.SERVERNAME, unitDBServer.DBNAME, unitDBServer.DBUSER, unitDBServer.DBPASSWORD, PMSConstants.ConnectionStringEncryptionKey));
                    List<sp_WbTransaction_GetBlockQty_Result> dataresult = contextWB.sp_WbTransaction_GetBlockQty(unitId, from.Date, to.Date).ToList();
                    foreach (var item in dataresult)
                    {
                        result = new THARVESTBLOCKRESULT();
                        result.CopyFrom(item);
                        result.SOURCE = source;
                        result.BLOCKID = item.BLOCK;
                        result.AUTO = true;
                        result.STATUS = "P";
                        result.UPDATED = GetServerTime();
                        resultlist.Add(result);
                    }
                }
            }

            string SPB = string.Empty;
            foreach (var item in resultlist)
                {
                int SPBCheckCount = SPBCheck(item.ID, item.NOSPB, item.BLOCKID);
                if (SPBCheckCount == 0)
                    {
                    BeforeSave(item,Username,true);
                    //Insert Data
                    _context.Entry(item).State = EntityState.Added;
                    _context.SaveChanges();
                    }
                else
                    {
                    var SPBNew = this.Get(item.SOURCE, item.NOSPB, item.DATE.Date, item.BLOCKID);
                    if (SPBNew != null && SPBNew.STATUS == "P")
                        {
                        //Update Data
                        _context.Entry(item).State = EntityState.Modified;
                        _context.SaveChanges();
                        }
                    }

                    var local = _context.Set<THARVESTBLOCKRESULT>()
                    .Local
                    .FirstOrDefault
                    (entry => entry.ID.Equals(item.ID) && entry.SOURCE.Equals(item.SOURCE) && entry.BLOCKID.Equals(item.BLOCKID)                    );
                    if (local != null)
                    {
                    _context.Entry(local).State = EntityState.Detached;
                    }
                }

            return true;
        }

        public int SPBCheck(string Id, String NoSPB, string BlockId)
        {
            return _context.THARVESTBLOCKRESULT.Where(d=> d.ID.Equals(Id) && d.NOSPB.Equals(NoSPB) && d.BLOCKID.Equals(BlockId)).Count();
        }

    }
       
}
