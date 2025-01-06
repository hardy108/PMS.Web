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
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using PMS.EFCore.Model.Extentions;
using PMS.EFCore.Services.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.GL
{
    public class Journal : EntityFactory<TJOURNAL,TJOURNAL,FilterJournal, PMSContextBase>
    {
        private Period _servicePeriod;
        private JournalType _serviceJournalType;
        private Account _serviceAccount;
        private AuthenticationServiceBase _authenticationService;

        private List<TJOURNALITEM> _newJournalItems = new List<TJOURNALITEM>();

        public Journal(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Journal";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(_context,_authenticationService,auditContext);
            _serviceJournalType = new JournalType(_context,_authenticationService,auditContext);
            _serviceAccount = new Account(_context,_authenticationService,auditContext);
        }

        private void ApproveValidate(TJOURNAL journal)
        {          
            if (string.IsNullOrEmpty(journal.REF))
                if (CheckNo(journal.CODE, journal.NO, journal.DATE, journal.UNITCODE) != 0)
                    throw new Exception("No Voucher untuk periode ini sudah ada.");
            if (journal.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (journal.STATUS == "C")
                throw new Exception("Data sudah di Cancel.");
            this.Validate(journal);

        }

        private void CancelValidate(TJOURNAL journal)
        {
            if (journal.STATUS != "A")
                throw new Exception("Data belum di approve.");

            if (!string.IsNullOrEmpty(journal.REF))
                throw new Exception("Tidak dapat dicancel " + journal.NO + "."
                    + Environment.NewLine + "Silahkan cancel melalui " + journal.REF + ".");

            _servicePeriod.CheckValidPeriod(journal.UNITCODE, journal.DATE.Date);
        }

        private void DeleteValidate(TJOURNAL journal)
        {
            if (journal.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (journal.STATUS == "C")
                throw new Exception("Data sudah di cancel.");

            if (!string.IsNullOrEmpty(journal.REF))
                throw new Exception("Tidak dapat dihapus " + journal.NO + "."
                    + Environment.NewLine + "Silahkan hapus melalui " + journal.REF + ".");

            _servicePeriod.CheckValidPeriod(journal.UNITCODE, journal.DATE.Date);
        }

        private void UpdateValidate(TJOURNAL journal)
        {
            this.Validate(journal);

            if (!string.IsNullOrEmpty(journal.REF))
                throw new Exception("Journal tidak dapat diupdate" + journal.NO + "."
                    + Environment.NewLine + "Silahkan update dari " + journal.REF+ ".");
        }

        private string FieldsValidation(TJOURNAL journal)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(journal.CODE)) result += "Kode harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(journal.NO)) result += "No harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(journal.TYPE)) result += "Tipe harus diisi." + Environment.NewLine;
            if (journal.DATE == new DateTime()) result += "Tanggal harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(journal.UNITCODE)) result += "Bisnis unit harus diisi." + Environment.NewLine;
            if (journal.STATUS == string.Empty) result += "Status harus diisi." + Environment.NewLine;
            
            //Journal Detail
            foreach (var item in journal.TJOURNALITEM)
            {
                if (string.IsNullOrEmpty(item.CODE)) result += "Kode harus diisi." + Environment.NewLine;
                if (item.SEQ == 0) result += "Sequence harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.ACCOUNTCODE)) result += "Account harus diisi." + Environment.NewLine;
                if (item.AMOUNT== 0) result += "Debit/Credit harus diisi." + Environment.NewLine;
            }
            
            return result;
        }

        private void Validate(TJOURNAL journal)
        {
            string result = this.FieldsValidation(journal);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            if (journal.TJOURNALITEM.Count == 0)
                throw new Exception("Detail belum diisi. ");

            if (journal.Debits() == 0 && journal.Credits() == 0)
                throw new Exception("Journal harus ada nilainya. ");

            if (journal.Debits() + journal.Credits() != 0)
                throw new Exception("Debit dan Credit tidak sama. ");

            MJOURNALTYPE type = _serviceJournalType.GetSingle(journal.TYPE.ToString());
            //MJOURNALTYPE type = _context.MJOURNALTYPE.Find(journal.TYPE);
            if (type.MODUL == PMSConstants.GL_Journal_CashIn || type.MODUL == PMSConstants.GL_Journal_CashOut)
                if (string.IsNullOrEmpty(journal.PAYEE))
                    throw new Exception("Penerima atau pembayar harus diisi. ");

            foreach (var item in journal.TJOURNALITEM)
            {
                var account = _context.MACCOUNT.Find(item.ACCOUNTCODE);
                if (account != null)
                    if (account.CO && string.IsNullOrEmpty(item.BLOCKID))
                        throw new Exception("Cost Object/Blok untuk account " + item.ACCOUNTCODE + " harus diisi.");
            }

            _servicePeriod.CheckValidPeriod(journal.UNITCODE, journal.DATE.Date);

        }

        public int CheckNo(string id, string no, DateTime date, string unitCode)
        {
            return _context.TJOURNAL.Where
                (
                d => d.DATE.Year == date.Year && d.DATE.Month == date.Month
                && d.NO == no && d.CODE != id && d.UNITCODE == unitCode && d.REF == string.Empty && d.STATUS == "A"
                ).Count();
        }

        public override TJOURNAL CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TJOURNAL record = base.CopyFromWebFormData(formData, newRecord);
            record.TJOURNALITEM.Clear();

            _newJournalItems = new List<TJOURNALITEM>();
            _newJournalItems.CopyFrom<TJOURNALITEM>(formData, "TJOURNALITEM");
            _newJournalItems.ForEach(d =>
            {
                record.TJOURNALITEM.Add(d);
            });
          
            return record;
        }

        public bool Approve(string code,string userName)
        {
            var record = GetSingle(code);
            this.ApproveValidate(record);

            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            record.STATUS = "A";

            var local = _context.Set<TJOURNAL>()
                .Local
                .FirstOrDefault(entry => entry.CODE.Equals(code));

            if (local != null)
            { _context.Entry(local).State = EntityState.Detached; }

            _context.Entry<TJOURNAL>(record).State = EntityState.Modified;
            _context.SaveChanges();

            foreach (var item in record.TJOURNALITEM)
                _serviceAccount.SetBalance(record.UNITCODE, item.ACCOUNTCODE, item.BLOCKID, record.DATE.Month,
                                                record.DATE.Year, item.AMOUNT);

            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {record.CODE}", _context);

            return true;

        }
        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string code = formDataCollection["CODE"];
            return Approve(code, userName);
        }

        public bool Cancel(IFormCollection formDataCollection, string userName)
        {
            string code = formDataCollection["CODE"];
            var record = GetSingle(code);
            this.CancelValidate(record);

            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();
            record.STATUS = "C";

            _context.Entry<TJOURNAL>(record).State = EntityState.Modified;
            _context.SaveChanges();

            foreach (var item in record.TJOURNALITEM)
                _serviceAccount.SetBalance(record.UNITCODE, item.ACCOUNTCODE, item.BLOCKID, record.DATE.Month,
                                                record.DATE.Year, item.AMOUNT);

            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Cancel {record.CODE}", _context);

            return true;
        }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.JournalCodePrefix + unitCode,_context);
            return PMSConstants.JournalCodePrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        protected override TJOURNAL BeforeSave(TJOURNAL record, string userName, bool newRecord)
        {
            record.NOTE = StandardUtility.IsNull(record.NOTE, string.Empty);
            record.REF = StandardUtility.IsNull(record.REF, string.Empty);
            record.PAYEE = StandardUtility.IsNull(record.PAYEE, string.Empty);
            record.CHEQUE = StandardUtility.IsNull(record.CHEQUE, string.Empty);

            short lineNo = 0;
            if (newRecord)
                record.CODE = GenereteNewCode(record.UNITCODE, record.DATE);

            foreach(var item in record.TJOURNALITEM)
            {
                item.CODE = record.CODE;
                item.SEQ = ++lineNo;
                item.NOTE= StandardUtility.IsNull(item.NOTE, string.Empty);
            }

            DateTime now = GetServerTime();
            record.STATUS = "P";
            if (newRecord)
            {
                this.Validate(record);
                record.CREATED = now;
                record.CREATEBY = userName;
            }
            else
                this.UpdateValidate(record);
            record.UPDATED = now;
            record.UPDATEBY = userName;
            

            _saveDetails = (record.TJOURNALITEM.Any());

            return record;

        }

        protected override TJOURNAL SaveInsertDetailsToDB(TJOURNAL record, string userName)
        {
            _context.TJOURNALITEM.AddRange(record.TJOURNALITEM);
            return record;
        }
        
        protected override TJOURNAL AfterSave(TJOURNAL record, string userName, bool newRecord)
        {
            if (newRecord)
            { 
                HelperService.IncreaseRunningNumber(PMSConstants.JournalCodePrefix + record.UNITCODE, _context);

                if (HelperService.GetConfigValue(PMSConstants.CFG_JournalAutoApprove + record.UNITCODE, _context) == PMSConstants.CFG_JournalAutoApproveTrue)
                {
                    this.Approve(record.CODE, userName);
                }
            }
            return record;
        }

        

        protected override TJOURNAL SaveUpdateDetailsToDB(TJOURNAL record, string userName)
        {
            DeleteDetailsFromDB(record, userName);
            return SaveInsertDetailsToDB(record, userName);
        }

        protected override TJOURNAL BeforeDelete(TJOURNAL record, string userName)
        {
            record = GetSingle(record.CODE);
            this.DeleteValidate(record);

            _saveDetails = (record.TJOURNALITEM.Any());
            return record;
        }

        protected override bool DeleteDetailsFromDB(TJOURNAL record, string userName)
        {
            _context.TJOURNALITEM.RemoveRange(_context.TJOURNALITEM.Where(d => d.CODE.Equals(record.CODE)));
            return true; ;
        }

        protected override TJOURNAL GetSingleFromDB(params  object[] keyValues)
        {
            string journalCode = keyValues[0].ToString();
            var record = _context.TJOURNAL
                .Include(d => d.TJOURNALITEM)
                .SingleOrDefault(d => d.CODE.Equals(journalCode));

            var JournalItems =
            (
            from a in record.TJOURNALITEM
            join b in _context.MACCOUNT on a.ACCOUNTCODE equals b.CODE
            join c in _context.VBLOCK on a.BLOCKID equals c.BLOCKID into ac
            from ac1 in ac.DefaultIfEmpty()
            select new { JournalItem = a, AccountName = b.NAME, BlockCode = (ac1 == null) ? string.Empty : ac1.CODE }
            ).ToList();
            
            JournalItems.ForEach(d => {
                 d.JournalItem.ACCOUNTNAME = d.JournalItem.ACCOUNTCODE + " - " + d.AccountName;
                 d.JournalItem.BLOCKCODE = d.BlockCode;
             });

            return record;
        }

        public override IEnumerable<TJOURNAL> GetList(FilterJournal filter)
        {
            
            var criteria = PredicateBuilder.True<TJOURNAL>();
            if (!string.IsNullOrWhiteSpace(filter.UnitID))
                criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));

            criteria = criteria.And(p => p.DATE.Date >= filter.StartDate.Date && p.DATE.Date <= filter.EndDate.Date);

            if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(p => !p.STATUS.Equals(PMSConstants.TransactionStatusDeleted));
            else
                criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.CODE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.NO.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.NOTE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
                    p.REF.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }



            if (!string.IsNullOrWhiteSpace(filter.JournalType))
                criteria = criteria.And(p => p.TYPE.Equals(filter.JournalType));

            if (!string.IsNullOrWhiteSpace(filter.Ref))
                criteria = criteria.And(p => p.REF.Equals(filter.Ref));

            if (!string.IsNullOrWhiteSpace(filter.Modul))
            {
                var journalTypes = _context.MJOURNALTYPE.Where(d => d.MODUL.ToLower().Contains(filter.Modul.ToLower())).Select(d => d.CODE).ToList();
                criteria = criteria.And(d => journalTypes.Contains(d.TYPE));
            }

            var result = _context.TJOURNAL.Where(criteria);

            if (filter.PageSize <= 0)
                return result;
            return result.GetPaged(filter.PageNo, filter.PageSize).Results;

        }

        public override TJOURNAL NewRecord(string userName)
        {
            var record = new TJOURNAL();
            record.STATUS = "P";
            record.DATE = DateTime.Today;
            return record;
        }

    }
}
