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
using PMS.EFCore.Services;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using PMS.EFCore.Model.Extentions;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.GL;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Inventory
{
    public class GoodIssue : EntityFactory<TGI,TGI, FilterGoodIssue, PMSContextBase>
    {
        private Period _servicePeriod;
        private JournalType _serviceJournalType;
        private Journal _serviceJournal;
        private Material _serviceMaterial;
        private Stock _serviceStock;
        AuthenticationServiceBase _authenticationService;

        private List<TGIITEM> _newTGIItems = new List<TGIITEM>();
        private bool _reverse;

        public GoodIssue(PMSContextBase context, AuthenticationServiceBase authenticationService,AuditContext auditContext) : base(context,auditContext)
        {
            _serviceName = "GoodIssue";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(_context,authenticationService,auditContext);
            _serviceJournalType = new JournalType(_context,_authenticationService,auditContext);
            _serviceJournal = new Journal(_context,authenticationService,auditContext);
            _serviceStock = new Stock(_context,_authenticationService,auditContext);
            _serviceMaterial = new Material(_context,_authenticationService,auditContext);
        }
        
        private void Validate(TGI issue)
        {
            string result = this.FieldsValidation(issue);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            if (issue.TGIITEM.Count == 0)
                throw new Exception("Detail belum diisi. ");

            _servicePeriod.CheckValidPeriod(issue.UNITCODE, issue.DATE.Date);

            var q = (from r in issue.TGIITEM
                     group r by new { r.MATERIALID, r.STOCK, }
                         into grp
                     select new { grp.Key.MATERIALID, grp.Key.STOCK, Quantity = grp.Select(x => x.QTY).Sum() }).ToList();

            foreach (var item in q)
                if (item.Quantity > item.STOCK)
                    throw new Exception("Jumlah item " + item.MATERIALID + "(" + item.Quantity + ") melebihi stok(" + item.STOCK + ").");
        }

        private string FieldsValidation(TGI issue)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(issue.NO)) result += "No harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(issue.VOUCHERNO)) result += "Voucher No harus diisi." + Environment.NewLine;
            if (issue.DATE == new DateTime()) result += "Tanggal harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(issue.UNITCODE)) result += "Bisnis Area harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(issue.LOCCODE)) result += "Lokasi harus diisi." + Environment.NewLine;
            if (issue.STATUS == string.Empty) result += "Status harus diisi." + Environment.NewLine;

            //TGR Item
            foreach (var item in issue.TGIITEM)
            {
                if (string.IsNullOrEmpty(item.CODE)) result += "Kode harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.NO)) result += "No harus diisi." + Environment.NewLine;
                if (item.SEQ == 0) result += "Sequence harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.MATERIALID)) result += "Material harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.ACCOUNTCODE)) result += "Account pengguna harus diisi." + Environment.NewLine;
                if (item.STOCK < 0) result += "Stok harus lebih besar atau sama dengan nol." + Environment.NewLine;
                if (item.QTY == 0) result += "Quantity tidak boleh nol." + Environment.NewLine;
                if (item.PRICE <= 0) result += "Harga harus lebih besar dari nol." + Environment.NewLine;
            }

            return result;
        }

        private void ApproveValidate(TGI issue)
        {
            if (string.IsNullOrEmpty(issue.REF))
                if (CheckNo(issue.NO, issue.VOUCHERNO, issue.DATE.Date, issue.UNITCODE) != 0)
                    throw new Exception("No Voucher untuk periode ini sudah ada.");
            if (issue.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (!string.IsNullOrEmpty(issue.REVCODE))
                throw new Exception("Data sudah di Reverse.");
            this.Validate(issue);
        }

        private void InsertValidate(TGI issue)
        { this.Validate(issue); }

        private void UpdateValidate(TGI issue)
        {
            this.Validate(issue);

            if (issue.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (!string.IsNullOrEmpty(issue.REVCODE))
                throw new Exception("Data sudah di Reverse.");

            if (!string.IsNullOrEmpty(issue.REF))
                throw new Exception("Document tidak dapat diupdate" + issue.NO + "."
                    + Environment.NewLine + "Silahkan update dari " + issue.REF + ".");
        }

        private void DeleteValidate(TGI issue)
        {
            if (issue.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (!string.IsNullOrEmpty(issue.REVCODE))
                throw new Exception("Data sudah di Reverse.");
            if (!string.IsNullOrEmpty(issue.REF))
                throw new Exception("Tidak dapat dihapus " + issue.NO + "."
                    + Environment.NewLine + "Silahkan hapus melalui " + issue.REF + ".");

            _servicePeriod.CheckValidPeriod(issue.UNITCODE, issue.DATE.Date);
        }

        protected override TGI GetSingleFromDB(params  object[] keyValues)
        {
            return _context.TGI
                .Include(d => d.TGIITEM).ThenInclude(d => d.MATERIAL).ThenInclude(e => e.ACCOUNTCODENavigation)
                .SingleOrDefault(d => d.NO.Equals(keyValues[0]));
        }

        private int CheckNo(string id, string no, DateTime date, string unitCode)
        {
            return _context.TGI.Where
                (
                d => d.DATE.Year == date.Year && d.DATE.Month == date.Month &&
                d.VOUCHERNO.Equals(no) && d.NO != id && d.UNITCODE == unitCode && d.REF == string.Empty &&
                d.STATUS == "A"
                ).Count();
        }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.GoodIssuePrefix + unitCode, _context);
            return PMSConstants.GoodIssuePrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        public override TGI CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TGI record = base.CopyFromWebFormData(formData, newRecord);
            record.TGIITEM.Clear();

            _newTGIItems = new List<TGIITEM>();
            _newTGIItems.CopyFrom<TGIITEM>(formData, "TGIITEM");
            _newTGIItems.ForEach(d =>
            {
                record.TGIITEM.Add(d);
            });

            return record;
        }

        public bool Approve(IFormCollection formDataCollection, bool reverse, string userName)
        {
            string no = formDataCollection["NO"];
            return Approve(no,reverse, userName);
        }

        public bool Approve(string no, bool reverse, string userName)
        {
            var issue = GetSingle(no);

            foreach (var item in issue.TGIITEM)
            {
                var stock = _serviceStock.GetStock(issue.LOCCODE, item.MATERIALID, issue.DATE);
                item.STOCK = stock[0];
                if (!reverse) if (item.QTY > 0) item.PRICE = stock[2];
            }

            foreach (var item in issue.TGIITEM)
            {
                _serviceStock.SetStock1(issue.LOCCODE, item.MATERIALID, issue.DATE.Month, issue.DATE.Year, false, item.QTY, item.QTY * item.PRICE, 0);
            }

            this.ApproveValidate(issue);

            issue.STATUS = "A";
            issue.UPDATED = GetServerTime();

            _context.Entry(issue).State = EntityState.Modified;
            _context.SaveChanges();

            var typeList = _serviceJournalType.GetByModul(PMSConstants.GL_Journal_GoodIssueModulCode);
            string journalType = typeList[0].CODE;

            var journal = new TJOURNAL
            {
                NO = issue.VOUCHERNO,
                TYPE = journalType,
                DATE = issue.DATE,
                UNITCODE = issue.UNITCODE,
                NOTE = issue.NOTE,
                STATUS = "P",
                REF = issue.NO,
                CREATEBY = userName,
                CREATED = GetServerTime(),
                UPDATEBY = userName,
                UPDATED = GetServerTime(),
                TJOURNALITEM = new List<TJOURNALITEM>(),
            };

            foreach (var item in issue.TGIITEM)
            {
                if (item.MATERIAL == null) item.MATERIAL = _serviceMaterial.Get(item.MATERIALID);
                var credItem = new TJOURNALITEM
                {
                    ACCOUNTCODE = item.MATERIAL.ACCOUNTCODE,
                    AMOUNT = item.QTY * item.PRICE * -1,
                    NOTE = item.NOTE,
                    BLOCKID = item.BLOCKID,
                };
                journal.TJOURNALITEM.Add(credItem);

                var debtItem = new TJOURNALITEM
                {
                    ACCOUNTCODE = item.ACCOUNTCODE,
                    AMOUNT = item.QTY * item.PRICE,
                    NOTE = item.NOTE,
                    BLOCKID = item.BLOCKID,
                };
                journal.TJOURNALITEM.Add(debtItem);
            }

            TJOURNAL JournalNew = _serviceJournal.SaveInsert(journal, userName);
            string journalId = JournalNew.CODE;

            if (HelperService.GetConfigValue(PMSConstants.CFG_JournalAutoApprove + journal.UNITCODE, _context) != PMSConstants.CFG_JournalAutoApproveTrue)
                _serviceJournal.Approve(journalId, userName);

            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {issue.NO}", _context);

            return true;
        }

        protected override TGI BeforeDelete(TGI record, string userName)
        {
            try
            {
                var tgi = GetSingle(record.NO);
                this.DeleteValidate(tgi);
                _saveDetails = record.TGIITEM.Any();
            }
            catch
            {
                throw;
            }
            return record;
        }

        protected override TGI BeforeSave(TGI record, string userName, bool newRecord)
        {
            _reverse = false;
            record.REVCODE = StandardUtility.IsNull(record.REVCODE, string.Empty);
            record.NOTE = StandardUtility.IsNull(record.NOTE, string.Empty);
            record.REF = StandardUtility.IsNull(record.REF, string.Empty);
            short lineNo = 0;
            foreach (var item in record.TGIITEM)
            {
                item.SEQ = ++lineNo;
                item.CODE = record.NO + "-" + item.SEQ;
                item.NO = record.NO;

                var stock = _serviceStock.GetStock(record.LOCCODE, item.MATERIALID, record.DATE.Date);
                item.STOCK = stock[0];
                if (!_reverse) item.PRICE = stock[2];
            };


            if (newRecord)
            {
                
                record.NO = GenereteNewCode(record.UNITCODE, record.DATE);
                this.InsertValidate(record);
                record.STATUS = "P";
                
            }
            else
            {
                this.UpdateValidate(record);
            }
            record.UPDATED = GetServerTime();
            _saveDetails = (record.TGIITEM.Any());

            return record;
        }

        

        protected override TGI SaveInsertDetailsToDB(TGI record, string userName)
        {
            _context.TGIITEM.AddRange(_newTGIItems);
            return record;
        }

        protected override TGI SaveUpdateDetailsToDB(TGI record, string userName)
        {
            DeleteDetailsFromDB(record, userName);
            return SaveInsertDetailsToDB(record, userName);
        }

        protected override bool DeleteDetailsFromDB(TGI record, string userName)
        {
            _context.TGIITEM.RemoveRange(_context.TGIITEM.Where(d => d.NO.Equals(record.NO)));
            return true; ;
        }

        protected override TGI AfterSave(TGI record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.GoodReceiptPrefix + record.UNITCODE, _context);
            return record;
        }

        private void ReverseValidate(TGI issue)
        {
            if (issue.STATUS != "A")
                throw new Exception("Data belum di approve.");
            if (!string.IsNullOrEmpty(issue.REVCODE))
                throw new Exception("Data sudah di Reverse.");
            _servicePeriod.CheckValidPeriod(issue.UNITCODE, issue.DATE.Date);
        }

        public string Reverse(string id, string userName)
        {
            try
            {
                var issue = GetSingle(id);

                this.ReverseValidate(issue);

                issue.UPDATED = GetServerTime();

                var newGi = new TGI
                {
                    VOUCHERNO = issue.VOUCHERNO + "C",
                    DATE = issue.DATE,
                    UNITCODE = issue.UNITCODE,
                    LOCCODE = issue.LOCCODE,
                    NOTE = issue.NOTE,
                    REF = issue.REF,
                    STATUS = "P",
                    UPDATED = issue.UPDATED,
                    TGIITEM = new List<TGIITEM>(),
                };

                foreach (var item in issue.TGIITEM)
                {
                    var newItem = new TGIITEM
                    {
                        MATERIALID = item.MATERIALID,
                        ACCOUNTCODE = item.ACCOUNTCODE,
                        QTY = item.QTY * -1,
                        PRICE = item.PRICE,
                        NOTE = item.NOTE,
                        BLOCKID = item.BLOCKID,
                    };
                    newGi.TGIITEM.Add(newItem);
                }

                _reverse = true;
                this.SaveInsert(newGi, userName);

                string newCode = this.SaveInsert(newGi, userName).NO.ToString();
                this.Approve(newCode, true, userName);

                issue.REVCODE = newCode;
                _context.Entry<TGI>(issue).State = EntityState.Modified;
                _context.SaveChanges();

                Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Reverse {issue.NO}", _context);
                return newCode;
            }
            catch (Exception ex) { throw ex; }
        }

        public override IEnumerable<TGI> GetList(FilterGoodIssue filter)
        {
            
            var criteria = PredicateBuilder.True<TGI>();

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.NOTE.ToLower().Contains(filter.LowerCasedSearchTerm)||
                        p.REF.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));

                if (!string.IsNullOrWhiteSpace(filter.No))
                    criteria = criteria.And(p => p.NO.Equals(filter.No));

                if (!string.IsNullOrWhiteSpace(filter.VoucherNo))
                    criteria = criteria.And(p => p.VOUCHERNO.Equals(filter.VoucherNo));

                if (!filter.Date.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.DATE.Date == filter.Date.Date);

                if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.DATE.Date >= filter.StartDate.Date && p.DATE.Date <= filter.EndDate.Date);

                var result = _context.TGI.Where(criteria);

                if (filter.PageSize <= 0)
                    return result;
                return result.GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return new List<TGI>(); }

        }

        public override TGI NewRecord(string userName)
        {
            var record = new TGI();
            record.DATE = GetServerTime();
            return record;
        }

    }
}
