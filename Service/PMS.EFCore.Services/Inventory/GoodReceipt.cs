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
    public class GoodReceipt : EntityFactory<TGR,TGR,FilterGoodReceipt, PMSContextBase>
    {

        private Period _servicePeriod;
        private JournalType _serviceJournalType;
        private Journal _serviceJournal;
        private Stock _serviceStock;
        private Material _materialService;
        private AuthenticationServiceBase _authenticationService;
        private List<TGRITEM> _newTGRItems = new List<TGRITEM>();

        public GoodReceipt(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "GoodReceipt";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(_context, _authenticationService,auditContext);
            _serviceJournalType = new JournalType(_context,_authenticationService,auditContext);
            _serviceJournal = new Journal(_context, _authenticationService,auditContext);
            _serviceStock = new Stock(_context,_authenticationService,auditContext);
            _materialService = new Material(context,_authenticationService,auditContext);
        }

        private void Validate(TGR receipt )
        {
            string result = this.FieldsValidation(receipt);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            if (receipt.TGRITEM.Count == 0)
                throw new Exception("Detail belum diisi. ");

            if (!string.IsNullOrEmpty(receipt.EXPCODE) && receipt.EXPVALUE == 0)
                throw new Exception("Nilai ekspedisi belum diisi. ");

            _servicePeriod.CheckMaxPeriod(receipt.UNITCODE, receipt.DATE);
        }

        private string FieldsValidation(TGR receipt)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(receipt.NO)) result += "No harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(receipt.VOUCHERNO)) result += "Voucher No harus diisi." + Environment.NewLine;
            if (receipt.DATE == new DateTime()) result += "Tanggal harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(receipt.UNITCODE)) result += "Bisnis Area harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(receipt.LOCCODE)) result += "Lokasi harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(receipt.VENDORCODE)) result += "Vendor harus diisi." + Environment.NewLine;
            if (receipt.STATUS == string.Empty) result += "Status harus diisi." + Environment.NewLine;

            //TGR Item
            foreach (var item in receipt.TGRITEM)
            {
                if (string.IsNullOrEmpty(item.CODE)) result += "Kode harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.NO)) result += "No harus diisi." + Environment.NewLine;
                if (item.SEQ == 0) result += "Sequence harus diisi." + Environment.NewLine;
                if (string.IsNullOrEmpty(item.MATERIALID)) result += "Material harus diisi." + Environment.NewLine;
                if (item.QTY == 0) result += "Jumlah harus diisi." + Environment.NewLine;
                if (item.PRICE <= 0) result += "Harga harus lebih besar dari nol." + Environment.NewLine;
            }
            return result;
        }

        private void ApproveValidate(TGR receipt)
        {
            if (string.IsNullOrEmpty(receipt.REF))
                if (this.CheckNo(receipt.NO, receipt.VOUCHERNO, receipt.DATE, receipt.UNITCODE) != 0)
                    throw new Exception("No Voucher untuk periode ini sudah ada.");
            if (receipt.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (!string.IsNullOrEmpty(receipt.REVCODE))
                throw new Exception("Data sudah di Reverse.");

            this.Validate(receipt);
        }

        private void InsertValidate(TGR receipt)
        { this.Validate(receipt); }

        private void UpdateValidate(TGR receipt)
        {
            this.Validate(receipt);

            if (receipt.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (!string.IsNullOrEmpty(receipt.REVCODE))
                throw new Exception("Data sudah di Reverse.");
            if (!string.IsNullOrEmpty(receipt.REF))
                throw new Exception("Penerimaan tidak dapat diupdate" + receipt.NO+ "."
                    + Environment.NewLine + "Silahkan update dari " + receipt.REF+ ".");
        }

        private void DeleteValidate(TGR receipt)
        {
            if (receipt.STATUS == "A")
                throw new Exception("Data sudah di approve.");
            if (!string.IsNullOrEmpty(receipt.REVCODE))
                throw new Exception("Data sudah di Reverse.");
            if (!string.IsNullOrEmpty(receipt.REF))
                throw new Exception("Tidak dapat dihapus " + receipt.NO + "."
                    + Environment.NewLine + "Silahkan hapus melalui " + receipt.REF + ".");

            _servicePeriod.CheckValidPeriod(receipt.UNITCODE, receipt.DATE.Date);
        }


        protected override TGR GetSingleFromDB(params object[] keyValues)
        {

            string Id = keyValues[0].ToString();
            TGR record = _context.TGR
                .Include(d => d.TGRITEM)
                .Include(d => d.VENDORCODENavigation)
                .Include(d => d.EXPCODENavigation)
                .Where(i => i.NO.Equals(Id)).SingleOrDefault();

            if (record != null)
            {
              
                foreach (var mat in record.TGRITEM)
                {
                    mat.MATERIAL = _materialService.GetSingle(mat.MATERIALID);
                }
                

            }

            return record;
        }

        private int CheckNo(string id, string no, DateTime date, string unitCode)
        {
            return _context.TGR.Where
                (
                d => d.DATE.Year == date.Year && d.DATE.Month == date.Month &&
                d.VOUCHERNO.Equals(no) && d.NO != id && d.UNITCODE == unitCode && d.REF == string.Empty &&
                d.STATUS=="A"
                ).Count();
        }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.GoodReceiptPrefix + unitCode,_context);
            return PMSConstants.GoodReceiptPrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        public override TGR CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TGR record = base.CopyFromWebFormData(formData, newRecord);
            record.TGRITEM.Clear();

            _newTGRItems = new List<TGRITEM>();
            _newTGRItems.CopyFrom<TGRITEM>(formData, "TGRITEM");
            _newTGRItems.ForEach(d =>
            {
                record.TGRITEM.Add(d);
            });

            return record;
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["NO"];
            return Approve(no, userName);
            

        }

        public bool Approve(string no, string userName)
        {
            var receipt = GetSingle(no);
            this.ApproveValidate(receipt);

            receipt.STATUS = "A";
            receipt.UPDATED = GetServerTime();

            foreach (var item in receipt.TGRITEM)
            {
                _serviceStock.SetStock1(receipt.LOCCODE, item.MATERIALID, receipt.DATE.Month, receipt.DATE.Year, true, item.QTY, item.PRICE * item.QTY, item.EXPVALUE);
            }

            _context.Entry(receipt).State = EntityState.Modified;
            _context.SaveChanges();

            var typeList = _serviceJournalType.GetByModul(PMSConstants.GL_Journal_GoodReceiptModulCode);
            string journalType = typeList[0].CODE;

            var journal = new TJOURNAL
            {
                NO = receipt.VOUCHERNO,
                TYPE = journalType,
                DATE = receipt.DATE,
                UNITCODE = receipt.UNITCODE,
                NOTE = receipt.NOTE,
                STATUS = "P",
                REF = receipt.NO,
                CREATEBY = userName,
                CREATED = GetServerTime(),
                UPDATEBY = userName,
                UPDATED = GetServerTime(),
                TJOURNALITEM = new List<TJOURNALITEM>(),
            };

            if (receipt.VENDORCODENavigation == null) receipt.VENDORCODENavigation = _context.MCARD.Where(d => d.CODE.Equals(receipt.VENDORCODE)).SingleOrDefault();
            var vendorItem = new TJOURNALITEM
            {
                ACCOUNTCODE = receipt.VENDORCODENavigation.ACCOUNTCODE,
                AMOUNT = receipt.TotalPrice() * -1,
            };
            journal.TJOURNALITEM.Add(vendorItem);


            if (receipt.EXPVALUE != 0)
            {
                if (receipt.EXPCODENavigation == null) receipt.EXPCODENavigation = _context.MCARD.Where(d => d.CODE.Equals(receipt.EXPCODE)).SingleOrDefault();
                var expeditionItem = new TJOURNALITEM
                {
                    ACCOUNTCODE = receipt.EXPCODENavigation.ACCOUNTCODE,
                    AMOUNT = receipt.EXPVALUE * -1,
                };
                journal.TJOURNALITEM.Add(expeditionItem);
            }

            foreach (var item in receipt.TGRITEM)
            {

                if (item.MATERIAL == null) item.MATERIAL = _context.MMATERIAL.Where(d => d.MATERIALID.Equals(item.MATERIALID)).SingleOrDefault();
                var matItem = new TJOURNALITEM
                {
                    ACCOUNTCODE = item.MATERIAL.ACCOUNTCODE,
                    NOTE = item.NOTE,
                    AMOUNT = (item.PRICE * item.QTY) + item.EXPVALUE,
                };
                journal.TJOURNALITEM.Add(matItem);
            }

            TJOURNAL JournalNew = _serviceJournal.SaveInsert(journal, userName);
            string journalId = JournalNew.CODE;

            if (HelperService.GetConfigValue(PMSConstants.CFG_JournalAutoApprove + journal.UNITCODE, _context) != PMSConstants.CFG_JournalAutoApproveTrue)
                _serviceJournal.Approve(journalId, userName);

            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {receipt.NO}", _context);

            return true;
        }
        private void CalculateExpeditionPrice(TGR receipt)
        {
            decimal expPrice = receipt.EXPVALUE;
            decimal totalPrice = receipt.TotalPrice();
            decimal expRest = expPrice;

            int i = 1;
            foreach (var item in receipt.TGRITEM)
            {
                if (expPrice == 0)
                    item.EXPVALUE = 0;
                else
                {
                    decimal price = i == receipt.TGRITEM.Count ? expRest : Math.Round(((item.PRICE * item.QTY) / totalPrice) * expPrice, 0);
                    item.EXPVALUE = price;
                    expRest -= price;
                    i++;
                }
            }
        }

        protected override TGR BeforeSave(TGR record, string userName, bool newRecord)
        {
            record.NOTE = StandardUtility.IsNull(record.NOTE, string.Empty);
            record.REF = StandardUtility.IsNull(record.REF, string.Empty);
            record.EXPVALUE = StandardUtility.IsNull(record.EXPVALUE, 0);
            record.REVCODE = StandardUtility.IsNull(record.REVCODE, string.Empty);

            if (newRecord)
                record.NO = GenereteNewCode(record.UNITCODE, record.DATE);

            short lineNo = 0;
            foreach (var item in record.TGRITEM)
            {
                item.SEQ = ++lineNo;
                item.CODE = record.NO + "-" + item.SEQ;
                item.NO = record.NO;               
            };

            this.CalculateExpeditionPrice(record);
            if (newRecord)
            {
                this.InsertValidate(record);
                record.STATUS = "P";
            }
            else
                UpdateValidate(record);

            
            record.UPDATED = GetServerTime();

            _saveDetails = (record.TGRITEM.Any());

            return record;            
        }

        

        protected override TGR SaveInsertDetailsToDB(TGR record, string userName)
        {
            _context.TGRITEM.AddRange(record.TGRITEM);
            return record;
        }

        protected override TGR SaveUpdateDetailsToDB(TGR record, string userName)
        {
            DeleteDetailsFromDB(record, userName);
            return SaveInsertDetailsToDB(record, userName);
        }

        protected override bool DeleteDetailsFromDB(TGR record, string userName)
        {
            _context.TGRITEM.RemoveRange(_context.TGRITEM.Where(d => d.NO.Equals(record.NO)));
            return true; ;
        }

        protected override TGR AfterSave(TGR record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.GoodReceiptPrefix + record.UNITCODE, _context);
            return record;
        }

        protected override TGR BeforeDelete(TGR record, string userName)
        {
            record = GetSingle(record.NO);
            this.DeleteValidate(record);

            _saveDetails = (record.TGRITEM.Any());
            return record;
        }

        private void ReverseValidate(TGR receipt)
        {
            if (receipt.STATUS != "A")
                throw new Exception("Data belum di approve.");
            if (!string.IsNullOrEmpty(receipt.REVCODE))
                throw new Exception("Data sudah di Reverse.");
             _servicePeriod.CheckValidPeriod(receipt.UNITCODE, receipt.DATE.Date);

            var q = (from r in receipt.TGRITEM
                     group r by new { r.MATERIALID, }
                         into grp
                     select new { grp.Key.MATERIALID, Quantity = grp.Select(x => x.QTY).Sum() }).ToList();

            foreach (var item in q)
            {
                decimal[] stock = _serviceStock.GetStock(receipt.LOCCODE, item.MATERIALID, receipt.DATE);
                if (item.Quantity > stock[0])
                    throw new Exception("Jumlah item " + item.MATERIALID + "(" + item.Quantity + ") melebihi stok(" + stock[0] + ").");
            }
        }

        public string Reverse(string id, string userName)
        {
            try
            {
                var receipt = GetSingle(id);

                this.ReverseValidate(receipt);

                receipt.UPDATED = GetServerTime();

                var newGr = new TGR
                {
                    VOUCHERNO = receipt.VOUCHERNO +"C",
                    DATE = receipt.DATE,
                    UNITCODE = receipt.UNITCODE,
                    LOCCODE = receipt.LOCCODE,
                    VENDORCODE = receipt.VENDORCODE,
                    EXPCODE = receipt.EXPCODE,
                    EXPVALUE = receipt.EXPVALUE * -1,
                    REF = receipt.REF,
                    NOTE = receipt.NOTE,
                    STATUS = "P",
                    UPDATED = receipt.UPDATED,
                    TGRITEM = new List<TGRITEM>(),
                };

                foreach (var item in receipt.TGRITEM)
                {
                    var newItem = new TGRITEM
                    {
                        MATERIALID = item.MATERIALID,
                        QTY = item.QTY * -1,
                        PRICE = item.PRICE,
                        EXPVALUE = item.EXPVALUE,
                        NOTE = item.NOTE,
                    };
                    newGr.TGRITEM.Add(newItem);
                }

                var entry = _context.Entry<TGR>(newGr);
                entry.State = EntityState.Added;
                _context.SaveChanges();

                string newCode = entry.Entity.NO;
                this.Approve(newCode, userName);

                receipt.REVCODE = newCode;
                _context.Entry<TGR>(receipt).State = EntityState.Modified;
                _context.SaveChanges();

                Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Reverse {receipt.NO}", _context);
                return newCode;
            }
            catch
            {
                throw;
            }
        }

        public override IEnumerable<TGR> GetList(FilterGoodReceipt filter)
        {
            
            var criteria = PredicateBuilder.True<TGR>();

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.NOTE.ToLower().Contains(filter.LowerCasedSearchTerm) ||
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

                if (!string.IsNullOrWhiteSpace(filter.VendorCode))
                    criteria = criteria.And(p => p.VENDORCODE.Equals(filter.VendorCode));

                if (!string.IsNullOrWhiteSpace(filter.ExpCode))
                    criteria = criteria.And(p => p.EXPCODE.Equals(filter.ExpCode));
                
                if(!filter.Date.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.DATE.Date ==  filter.Date.Date);

                if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.DATE.Date >= filter.StartDate.Date && p.DATE.Date <= filter.EndDate.Date);

                var result = _context.TGR.Where(criteria);

                if (filter.PageSize <= 0)
                    return result;
                return result.GetPaged(filter.PageNo, filter.PageSize).Results;

            }
            catch { return new List<TGR>(); }

        }

    }
}
