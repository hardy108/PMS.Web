using AM.EFCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PMS.EFCore.Helper;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.GL;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PMS.EFCore.Services.Payroll
{
    public class Loan : EntityFactory<TLOANEMP, TLOANEMP, GeneralFilter, PMSContextBase>
    {

        private Period _servicePeriod;
        private Material _serviceMaterial;
        private Account _serviceAccount;

        private List<TLOANEMPITEM> _newTLOANItem = new List<TLOANEMPITEM>();
        private AuthenticationServiceBase _authenticationService;
        public Loan(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Loan";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(context, _authenticationService, auditContext);
            _serviceMaterial = new Material(context, _authenticationService, auditContext);
            _serviceAccount = new Account(context, _authenticationService, auditContext);
        }

        protected override TLOANEMP GetSingleFromDB(params object[] keyValues)
        {
            TLOANEMP record =
            _context.TLOANEMP
            //.Include(d => d.TLOANEMPITEM)
            .Include(d => d.MEMPLOYEE)
            .SingleOrDefault(d => d.TRANID.Equals(keyValues[0]));

            //if (record != null)
            //{
            //    if (record.TLOANEMPITEM != null && record.TLOANEMPITEM.Any())
            //    {
            //        var materialIds = record.TLOANEMPITEM.Select(d => d.MATERIALID).Distinct().ToList();
            //        record.TLOANEMPITEM.Join(
            //            _serviceMaterial.GetList(new FilterMaterial { Ids = materialIds }),
            //            a => a.MATERIALID,
            //            b => b.MATERIALID,
            //            (a, b) => new { a, MATERIALNAME = b.MATERIALNAME }).ToList().ForEach(d =>
            //              {
            //                  d.a.MATNAME = d.MATERIALNAME;

            //              });

            //        var accountIds = record.TLOANEMPITEM.Select(d => d.ACCOUNTCODE).Distinct().ToList();
            //        record.TLOANEMPITEM.Join(
            //            _serviceAccount.GetList(new FilterAccount { Ids = accountIds }),
            //            a => a.ACCOUNTCODE,
            //            b => b.CODE,
            //            (a, b) => new { a, ACCOUNTNAME = b.NAME }).ToList().ForEach(d =>
            //            {
            //                d.a.ACCNAME = d.ACCOUNTNAME;

            //            });
            // }

            //}
            return record;
        }

        private void ApproveValidate(TLOANEMP loan)
        {
            if (loan.STATUS == "A")
                throw new Exception("Data sudah di approve.");

            if (loan.TENOR >= 999)
                throw new Exception("Tenor Tidak bisa lebih dari 999");

            this.Validate(loan);
        }

        protected override TLOANEMP BeforeDelete(TLOANEMP record, string userName)
        {
            try
            {
                var loan = GetSingle(record.TRANID);
                this.DeleteValidate(loan);
                //_saveDetails = loan.TLOANEMPITEM.Any();
            }
            catch
            {
                throw;
            }
            return record;
        }

        protected override TLOANEMP BeforeSave(TLOANEMP record, string userName, bool newRecord)
        {
            DateTime now = GetServerTime();
            record.NOTE = StandardUtility.IsNull(record.NOTE, string.Empty);
            if (newRecord)
            {
                record.TRANID = GenereteNewCode(record.UNITID, record.LOANDATE);
                this.InsertValidate(record);
                record.CREATED = now;
                record.CREATEDBY = userName;
                record.STATUS = "P";
            }
            else
                UpdateValidate(record);


            record.UPDATED = now;
            record.UPDATEBY = userName;
            

           // _saveDetails = (record.TLOANEMPITEM.Any() );
            return record;          
        }

        private void InsertValidate(TLOANEMP loan)
        { this.Validate(loan); }

        private string GenereteNewCode(string unitCode, DateTime dateTime)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.LoanPrefix + unitCode, _context);
            return PMSConstants.LoanPrefix + unitCode + dateTime.ToString("yyyyMMdd")
                + lastNumber.ToString().PadLeft(4, '0');
        }

        private void UpdateValidate(TLOANEMP loan)
        {
            this.Validate(loan);
            if (loan.STATUS == "A")
                throw new Exception("Data sudah di approve.");
        }

     

        private void DeleteValidate(TLOANEMP loan)
        {
            if (loan.STATUS == "A")
                throw new Exception("Data sudah di approve.");

            _servicePeriod.CheckValidPeriod(loan.UNITID, loan.LOANDATE.Date);
        }


        public override TLOANEMP CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TLOANEMP record = base.CopyFromWebFormData(formData, newRecord);
            return record;
        }



        //public override TLOANEMP CopyFromWebFormData(IFormCollection formData, bool newRecord)
        //{
        //    TLOANEMP record = base.CopyFromWebFormData(formData, newRecord);
        //    record.TLOANEMPITEM.Clear();

        //    _newTLOANItem = new List<TLOANEMPITEM>();
        //    _newTLOANItem.CopyFrom<TLOANEMPITEM>(formData, "TLOANEMPITEM");
        //    _newTLOANItem.ForEach(d =>
        //    {
        //        record.TLOANEMPITEM.Add(d);
        //    });

        //    return record;
        //}

        protected override TLOANEMP AfterSave(TLOANEMP record, string userName, bool newRecord)
        {
            if (newRecord)
                HelperService.IncreaseRunningNumber(PMSConstants.LoanPrefix + record.UNITID, _context);
            return record;
        }



        private void Validate(TLOANEMP loan)
        {
            string result = this.FieldsValidation(loan);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            if (loan.TENOR >= 999)
                throw new Exception("Tenor Tidak bisa lebih dari 999");

            //if (loan.TLOANEMPITEM.Count == 0)
            //    throw new Exception("Detail belum diisi. ");

            _servicePeriod.CheckValidPeriod(loan.UNITID, loan.LOANDATE);

        }

        private string FieldsValidation(TLOANEMP loan)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(loan.TRANID)) result += "Tran Id harus diisi." + Environment.NewLine;
            if (loan.LOANDATE == new DateTime()) result += "Tanggal harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(loan.UNITID)) result += "Bisnis Area harus diisi." + Environment.NewLine;
            if (string.IsNullOrEmpty(loan.EMPID)) result += "Karyawan harus diisi." + Environment.NewLine;
            if (loan.STATUS == string.Empty) result += "Status harus diisi." + Environment.NewLine;
            if (loan.TENOR == 0) result += "Tenor tidak boleh nol." + Environment.NewLine;
            if (loan.TOTAL <= 0) result += "Total harus lebih besar dari nol." + Environment.NewLine;

            //TLoanEmp Item
            //foreach (var item in loan.TLOANEMPITEM)
            //{
            //    if (string.IsNullOrEmpty(item.TRANID)) result += "Tran Id harus diisi." + Environment.NewLine;
            //    if (item.SEQ == 0) result += "Sequence harus diisi." + Environment.NewLine;
            //    if (string.IsNullOrEmpty(item.MATERIALID)) result += "Material harus diisi." + Environment.NewLine;
            //    if (string.IsNullOrEmpty(item.ACCOUNTCODE)) result += "Account pengguna harus diisi." + Environment.NewLine;
            //    if (string.IsNullOrEmpty(item.BLOCKID)) result += "Block harus diisi." + Environment.NewLine;
            //    if (item.QTY == 0) result += "Quantity tidak boleh nol." + Environment.NewLine;
            //    if (item.PRICE <= 0) result += "Harga harus lebih besar dari nol." + Environment.NewLine;
            //}

            return result;
        }





        

        //protected override TLOANEMP SaveInsertDetailsToDB(TLOANEMP record, string userName)
        //{
        //    if (_newTLOANItem != null)
        //        _context.TLOANEMPITEM.AddRange(_newTLOANItem);
        //    return record;
        //}

        //protected override TLOANEMP SaveUpdateDetailsToDB(TLOANEMP record, string userName)
        //{
        //    DeleteDetailsFromDB(record, userName);
        //    return SaveInsertDetailsToDB(record, userName);
        //}

        //protected override bool DeleteDetailsFromDB(TLOANEMP record, string userName)
        //{
        //    _context.TLOANEMPITEM.RemoveRange(_context.TLOANEMPITEM.Where(d => d.TRANID.Equals(record.TRANID)));            
        //    return true; ;
        //}


        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["TRANID"];
            var loan = GetSingle(no);
            this.ApproveValidate(loan);


            decimal installement = 0;
            decimal tot_installement = 0;

            for (int i = 1; i <= loan.TENOR; i++)
            {
                if (loan.TOTAL > 0)
                {
                    if (i != loan.TENOR)
                    {
                        installement = decimal.Round(loan.TOTAL / loan.TENOR, 0);
                        tot_installement += installement;
                    }
                    else
                        installement = loan.TOTAL - tot_installement;

                var loanTenor = new TLOANEMPTENOR
                    {
                        ID = loan.TRANID + "-" + i.ToString(),
                        TENORDATE = loan.LOANDATE.AddMonths(i - 1),
                        UNITID = loan.UNITID,
                        EMPID = loan.EMPID,
                        TYPE ="LOAN EMP",
                        AMOUNT = loan.TOTAL,
                        TENOR = i,
                        INSTALLMENT = installement,
                        //INSTALLMENT = loan.TOTAL/loan.TENOR,
                        REF = loan.TRANID,
                        NOTE = loan.NOTE,
                        PAYMENTNO = string.Empty,
                        STATUS = "A",
                        CREATED = GetServerTime(),
                        CREATEDBY = loan.CREATEDBY,
                        UPDATED = GetServerTime(),
                        UPDATEBY = loan.UPDATEBY,
                    };
                    _context.Entry(loanTenor).State = EntityState.Added;
                    _context.SaveChanges();
                }
            }

            loan.STATUS = "A";
            loan.UPDATEBY = userName;
            loan.UPDATED = GetServerTime();
            _context.Entry(loan).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        public bool CancelApprove(IFormCollection formDataCollection, string userName)
        {
            string no = formDataCollection["TRANID"];
            string canceledcomment = formDataCollection["CANCELEDCOMMENT"];
            var loan = GetSingle(no);
            this.CancelValidate(loan);

            var record = _context.TLOANEMPTENOR.Where (d => d.REF.Equals(loan.TRANID) && d.STATUS.Equals("A") && d.PAYMENTNO == string.Empty).ToList();           

            if (record != null)
            {
                foreach (var data in record)
                    data.STATUS = "C";
            }
            _context.TLOANEMPTENOR.UpdateRange(record);
            _context.SaveChanges();

            loan.STATUS = "C";
            loan.UPDATEBY = userName;
            loan.UPDATED = GetServerTime();
            _context.Entry(loan).State = EntityState.Modified;
            _context.SaveChanges();
            return true;
        }

        private void CancelValidate(TLOANEMP loan)
        {
            if (loan.STATUS != "A")
                throw new Exception("Data belum di approve.");
            if (loan.STATUS == "C")
                throw new Exception("Data sudah di cancel.");

            TLOANEMPTENOR record = _context.TLOANEMPTENOR
            .FirstOrDefault(d => d.REF.Equals(loan.TRANID) && d.STATUS.Equals("A") && d.PAYMENTNO != String.Empty );

            if (record != null)
            {
                throw new Exception("Payroll Sudah Di Approve");
            }
            _servicePeriod.CheckValidPeriod(loan.UNITID, loan.LOANDATE);
        }

        public override IEnumerable<TLOANEMP> GetList(GeneralFilter filter)
        {

            var criteria = PredicateBuilder.True<TLOANEMP>();
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

            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => d.STATUS.Equals(filter.RecordStatus));

            if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                criteria = criteria.And(p => p.LOANDATE.Date >= filter.StartDate.Date && p.LOANDATE.Date <= filter.EndDate.Date);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.UNITID.Contains(filter.Keyword)
                        || d.UNITID.Contains(filter.Keyword)
                        || d.NOTE.Contains(filter.Keyword)
                        || d.TRANID.Contains(filter.Keyword)
                    );

            if (filter.PageSize <= 0)
                return _context.TLOANEMP.Include(d=> d.MEMPLOYEE).Where(criteria);

            return _context.TLOANEMP.Include(d=> d.MEMPLOYEE).Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }     

        public IEnumerable<TLOANEMP> GetAll(object filterParameter)
        {
            try
            {
                return _context.TLOANEMP.ToList();
            }
            catch { return new List<TLOANEMP>(); }
        }

        public override TLOANEMP NewRecord(string userName)
        {
            var record = new TLOANEMP();
            record.LOANDATE = GetServerTime();
            return record;
        }

    }

}