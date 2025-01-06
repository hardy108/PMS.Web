using System;
using System.Collections;
using System.Collections.Generic;

using System.Text;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Helper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;
using Newtonsoft.Json;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.Payroll;
using PMS.EFCore.Services.Attendances;
using PMS.EFCore.Services.Organization;
using PMS.EFCore.Services.Logistic;
using PMS.Shared.Utilities;
using AM.EFCore.Services;
using PMS.EFCore.Services.GL;

namespace PMS.EFCore.Services.Payroll
{
    public class ProcessMonthly : EntityFactory<TPAYMENT, TPAYMENT, GeneralFilter, PMSContextBase>
    {
        private Period _servicePeriod;
        private Attendance _serviceAttendance;
        private JournalType _serviceJournalType;

        private PremiNonPanen _servicePremiNonPanen;
        private SalaryItem _salaryitemService;
        private Journal _serviceJournal;

        private DateTime startDate;
        private DateTime endDate;
        private AuthenticationServiceBase _authenticationService;

        public ProcessMonthly(PMSContextBase context, AuthenticationServiceBase authenticationService,AuditContext auditContext) : base(context,auditContext)
        {
            _serviceName = "ProcessMonthly";
            _authenticationService = authenticationService;
            _servicePeriod = new Period(_context, authenticationService,auditContext);
            _serviceAttendance = new Attendance(_context, _authenticationService,auditContext);
            _serviceJournalType = new JournalType(_context, _authenticationService,auditContext);

            _servicePremiNonPanen = new PremiNonPanen(_context, _authenticationService,auditContext);
            _salaryitemService = new SalaryItem(_context, _authenticationService,auditContext);
            _serviceJournal = new Journal(_context, _authenticationService,auditContext);
        }

        public override TPAYMENT NewRecord(string userName)
        {
            var record = new TPAYMENT();
            record.DOCDATE = GetServerTime();
            record.STATUS = PMSConstants.TransactionStatusProcess;
            return record;
        }

        public override TPAYMENT CopyFromWebFormData(IFormCollection formData, bool newRecord)
        {
            TPAYMENT record = base.CopyFromWebFormData(formData, newRecord);
            return record;
        }

        public TPAYMENT Process(string docNo, string unitId, DateTime date, string userName)
        {

            bool IsNew = false;
            var unit = _context.MUNIT.Where(d => d.UNITCODE.Equals(unitId)).FirstOrDefault();
            if (unit == null)
                throw new Exception("Unit tidak ada");

            var period = _context.MPERIOD.Where(d => d.UNITCODE.Equals(unitId)
            && d.YEAR == Convert.ToInt16(date.Year) && d.MONTH == Convert.ToInt16(date.Month)).FirstOrDefault();
            var scheme = _context.MPAYMENTSCHEME.Where(d => d.UNITCODE.Equals(unitId)).FirstOrDefault();

            _servicePeriod.CheckValidPeriod(unit.UNITCODE, date);

            TPAYMENT record;

            //New
            if (string.IsNullOrEmpty(docNo))
            {
                IsNew = true;
                record = new TPAYMENT();
                record.UNITCODE = unit.UNITCODE;
                record.STATUS = PMSConstants.TransactionStatusProcess;
                record.DOCNO = this.GenerateNewNumber(record.UNITCODE);
                record.DOCDATE = date.Date;
                record.STARTDATE = period.FROM1;
                record.ENDDATE = period.TO2;
                record.PERIOD = SetPeriod(record.UNITCODE, record.DOCDATE.Date);

                if (record.PERIOD == 1)
                { startDate = period.FROM1; endDate = period.TO1; }
                else
                { startDate = period.FROM2; endDate = period.TO2; }

                record.STATUS = PMSConstants.TransactionStatusProcess;
                record.DOCNO = GenerateNewNumber(unit.UNITCODE);
                record.KTU = unit.UNITKTU;
                record.MANAGER = unit.UNITMGR;
                record.CREATEBY = userName;
                record.CREATED = GetServerTime();

                this.InsertValidate(record);
            }

            //Edit
            else
            {
                record = new TPAYMENT();
                record = _context.TPAYMENT.Where(d => d.DOCNO.Equals(docNo)).FirstOrDefault();
                UpdateValidate(record);
            }

            record.UPDATEBY = userName;
            record.UPDATED = GetServerTime();

            CalculatePayroll(record, period, scheme);


            SaveInsertOrUpdate(record, userName);

            TPAYMENTSCHEME paymentscheme = new TPAYMENTSCHEME();
            paymentscheme.CopyFrom(scheme);
            paymentscheme.PAYMENTNO = record.DOCNO;
            record.VTPAYMENTSCHEME.Add(paymentscheme);

            if (IsNew)
            {
                HelperService.IncreaseRunningNumber(PMSConstants.PaymentCodePrefix + record.UNITCODE, _context);
                SaveInsertDetailsToDB(record, userName);
            }
            else
                SaveUpdateDetailsToDB(record, userName);

            return _context.TPAYMENT.Where(d => d.DOCNO.Equals(record.DOCNO)).FirstOrDefault();
        }

        protected override TPAYMENT SaveInsertDetailsToDB(TPAYMENT record, string userName)
        {
            _context.TPAYMENTDETAIL.AddRange(record.TPAYMENTDETAIL);
            _context.TPAYMENTITEM.AddRange(record.TPAYMENTITEM);
            _context.TPAYMENTATTR.AddRange(record.TPAYMENTATTR);
            _context.TPAYMENTATTREMP.AddRange(record.TPAYMENTATTREMP);

            _context.TPREMIMANDOR.AddRange(record.TPREMIMANDOR);
            _context.TPREMIMANDOR1.AddRange(record.TPREMIMANDOR1);
            _context.TPREMICHECKER.AddRange(record.TPREMICHECKER);

            foreach (var row in record.TSALARYITEM)
            {
                row.PAYMENTNO = record.DOCNO;
                row.STATUS = PMSConstants.TransactionStatusApproved;
                row.UPDATED = GetServerTime();
                row.NOTE = "";
                _salaryitemService.SaveInsert(row, userName);
            }
            _context.TPAYMENTTAX.AddRange(record.TPAYMENTTAX);

            //_context.TINCMANDORDIV.AddRange(record.TINCMANDORDIV);
            //_context.TINCMANDOR.AddRange(record.TINCMANDOR);

            //Update TPREMINONPANEN ; DOCNO
            _context.TPREMINONPANEN.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) &&
            d.DIVID.StartsWith(record.UNITCODE) && d.PREMIDATE.Date >= record.STARTDATE.Date && d.PREMIDATE.Date <= record.ENDDATE.Date
            ).ToList()
            .ForEach(r => { r.PAYMENTNO = record.DOCNO; r.UPDATEBY = userName; r.UPDATED = GetServerTime(); });

            //Update TSALARYITEM ; DOCNO
            _context.TSALARYITEM.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) &&
            d.UNITID.Equals(record.UNITCODE) && d.DATE.Date >= record.STARTDATE.Date && d.DATE.Date <= record.ENDDATE.Date
            ).ToList()
            .ForEach(r => { r.PAYMENTNO = record.DOCNO; r.UPDATED = GetServerTime(); });

            //Update THARVESTRESULT1 ; DOCNO
            _context.THARVESTRESULT1.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) &&
            d.UNITCODE.Equals(record.UNITCODE) && d.HARVESTDATE.Date >= record.STARTDATE.Date && d.HARVESTDATE.Date <= record.ENDDATE.Date
            ).ToList()
            .ForEach(r => { r.PAYMENTNO = record.DOCNO; r.UPDATED = GetServerTime(); });

            //Update TGERDANRESULT ; DOCNO
            _context.TGERDANRESULT.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) &&
            d.UNITCODE.Equals(record.UNITCODE) && d.HARVESTDATE.Date >= record.STARTDATE.Date && d.HARVESTDATE.Date <= record.ENDDATE.Date
            ).ToList()
            .ForEach(r => { r.PAYMENTNO = record.DOCNO; r.UPDATED = GetServerTime(); });

            //Update TLOADINGRESULT ; DOCNO
            _context.TLOADINGRESULT.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) &&
            d.UNITCODE.Equals(record.UNITCODE) && d.LOADINGDATE.Date >= record.STARTDATE.Date && d.LOADINGDATE.Date <= record.ENDDATE.Date
            ).ToList()
            .ForEach(r => { r.PAYMENTNO = record.DOCNO; r.UPDATED = GetServerTime(); });

            //Update TOPERATINGRESULT ; DOCNO
            _context.TOPERATINGRESULT.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) &&
            d.UNITCODE.Equals(record.UNITCODE) && d.LOADINGDATE.Date >= record.STARTDATE.Date && d.LOADINGDATE.Date <= record.ENDDATE.Date
            ).ToList()
            .ForEach(r => { r.PAYMENTNO = record.DOCNO; r.UPDATED = GetServerTime(); });

            _context.TPAYMENTSCHEME.AddRange(record.VTPAYMENTSCHEME);

            _context.SaveChanges();
            return record;
        }

        protected override TPAYMENT SaveUpdateDetailsToDB(TPAYMENT record, string userName)
        {
            DeleteDetailsFromDB(record, userName);
            return SaveInsertDetailsToDB(record, userName);
        }

        protected override bool DeleteDetailsFromDB(TPAYMENT record, string userName)
        {
            _context.TPAYMENTITEM.RemoveRange(_context.TPAYMENTITEM.Where(d => d.DOCNO.Equals(record.DOCNO)));
            _context.TPAYMENTSCHEME.RemoveRange(_context.TPAYMENTSCHEME.Where(d => d.PAYMENTNO.Equals(record.DOCNO)));
            _context.TPREMIMANDOR.RemoveRange(_context.TPREMIMANDOR.Where(d => d.PAYMENTNO.Equals(record.DOCNO)));
            _context.TPREMIMANDOR1.RemoveRange(_context.TPREMIMANDOR1.Where(d => d.PAYMENTNO.Equals(record.DOCNO)));
            _context.TPREMICHECKER.RemoveRange(_context.TPREMICHECKER.Where(d => d.PAYMENTNO.Equals(record.DOCNO)));
            _context.TPAYMENTDETAIL.RemoveRange(_context.TPAYMENTDETAIL.Where(d => d.DOCNO.Equals(record.DOCNO)));

            _context.TPAYMENTATTR.RemoveRange(_context.TPAYMENTATTR.Where(d => d.DOCNO.Equals(record.DOCNO)));
            _context.TPAYMENTATTREMP.RemoveRange(_context.TPAYMENTATTREMP.Where(d => d.DOCNO.Equals(record.DOCNO)));

            _salaryitemService.DeleteByPaymentNo(record.DOCNO, record.DOCDATE.Date);

            _context.TPAYMENTTAX.RemoveRange(_context.TPAYMENTTAX.Where(d => d.DOCNO.Equals(record.DOCNO)));

            //_context.TINCMANDORDIV.RemoveRange(_context.TINCMANDORDIV.Where(d => d.DOCNO.Equals(record.DOCNO)));
            //_context.TINCMANDOR.RemoveRange(_context.TINCMANDOR.Where(d => d.DOCNO.Equals(record.DOCNO)));

            _context.SaveChanges();

            _auditContext.SaveAuditTrail(userName, record.DOCNO, record.UPDATED.ToString("MM/dd/yyyy HH:mm:ss"));

            return true; ;
        }

        public override IEnumerable<TPAYMENT> GetList(GeneralFilter filter)
        {
            var criteria = PredicateBuilder.True<TPAYMENT>();

            if (!string.IsNullOrWhiteSpace(filter.UnitID))
            {
                criteria = criteria.And(d => d.UNITCODE.Equals(filter.UnitID));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                criteria = criteria.And(p =>
                    p.DOCNO.ToLower().Contains(filter.LowerCasedSearchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.DOCNO == filter.Id);

            if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

            if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                criteria = criteria.And(p => p.DOCDATE.Date >= filter.StartDate.Date && p.DOCDATE.Date <= filter.EndDate.Date);

            if (filter.PageSize <= 0)
                return _context.TPAYMENT.Where(criteria).ToList();
            return _context.TPAYMENT.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }   

        protected override TPAYMENT GetSingleFromDB(params object[] keyValues)
        {
            TPAYMENT record =
                _context.TPAYMENT
                .FirstOrDefault(d => d.DOCNO.Replace(@"/", "").Equals(keyValues[0]) || d.DOCNO.Equals(keyValues[0]));
            return record;
        }

        protected override TPAYMENT BeforeDelete(TPAYMENT record, string userName)
        {
            try
            {
                var Loading = GetSingle(record.DOCNO);
                this.DeleteValidate(Loading);
                record.TPAYMENTDETAIL.Add(_context.TPAYMENTDETAIL.Where(d => d.DOCNO.Equals(Loading.DOCNO)).FirstOrDefault());
                _saveDetails = record.TPAYMENTDETAIL.Any();
            }
            catch
            {
                throw;
            }
            return record;
        }

        public void CalculatePayroll(TPAYMENT payment, MPERIOD period, MPAYMENTSCHEME scheme)
        {
            var result = string.Empty;

            payment.InitEditMonthly();

            if (scheme == null)
                throw new Exception("Perhitungan upah belum dibuat.");

            int dayPerMonth = PMSConstants.PayrollDaysPerMonth;
            var perMonth = HelperService.GetConfigValue(PMSConstants.CfgPayrollDayPerMonth + payment.UNITCODE, _context);
            if (!string.IsNullOrEmpty(perMonth)) dayPerMonth = Convert.ToInt16(perMonth);

            DateTime startDate; DateTime endDate; DateTime endDatePeriod1; decimal ricePrice;
            DateTime startPeriod = period.FROM1;
            if (payment.PERIOD == 1)
            {
                startDate = period.FROM1;
                endDate = period.TO1;
                endDatePeriod1 = period.TO1;
                ricePrice = 0;
            }
            else
            {
                startDate = period.FROM2;
                endDate = period.TO2;
                endDatePeriod1 = period.TO1;
                ricePrice = period.RICEPRICE2;
            }

            List<MSALARYTYPE> salaryTypes = _context.MSALARYTYPE.Select(row => row).ToList();
            MJAMSOSTEK jamsostek = _context.MJAMSOSTEK.Where(d => d.UNITCODE.Equals(payment.UNITCODE)).FirstOrDefault();

            bool hkCalcByFingerPrint = false;
            if (HelperService.GetConfigValue(PMSConstants.CfgAttendanceCalcByFingerprint + payment.UNITCODE, _context) == PMSConstants.CfgAttendanceCalcByFingerprintTrue)
                hkCalcByFingerPrint = true;

            bool bhlNatura = false;
            if (HelperService.GetConfigValue(PMSConstants.CfgPayrollNaturaBhl + payment.UNITCODE, _context) == PMSConstants.CfgPayrollNaturaBhlTrue)
                bhlNatura = true;

            payment.THARVESTRESULT1.Clear();
            payment.THARVESTRESULT1.AddRange(_context.THARVESTRESULT1.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) && d.UNITCODE.Equals(payment.UNITCODE)
                && d.HARVESTDATE.Date >= payment.STARTDATE.Date && d.HARVESTDATE.Date <= payment.ENDDATE.Date).ToList());

            payment.TGERDANRESULT.Clear();
            payment.TGERDANRESULT.AddRange(_context.TGERDANRESULT.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) && d.UNITCODE.Equals(payment.UNITCODE)
                && d.HARVESTDATE.Date >= payment.STARTDATE.Date && d.HARVESTDATE.Date <= payment.ENDDATE.Date).ToList());

            payment.TLOADINGRESULT.Clear();
            payment.TLOADINGRESULT.AddRange(_context.TLOADINGRESULT.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) && d.UNITCODE.Equals(payment.UNITCODE)
                && d.LOADINGDATE.Date >= payment.STARTDATE.Date && d.LOADINGDATE.Date <= payment.ENDDATE.Date).ToList());

            payment.TOPERATINGRESULT.Clear();
            payment.TOPERATINGRESULT.AddRange(_context.TOPERATINGRESULT.Where(d => d.STATUS.Equals(PMSConstants.TransactionStatusApproved) && d.UNITCODE.Equals(payment.UNITCODE)
                && d.LOADINGDATE.Date >= payment.STARTDATE.Date && d.LOADINGDATE.Date <= payment.ENDDATE.Date).ToList());

            payment.TPAYMENTDETAIL.Clear();
            _context.sp_TPAYMENTDETAIL_GetFromEmployee_Result(payment.UNITCODE, payment.STARTDATE.Date, payment.ENDDATE.Date).ToList()
                .ForEach(d => payment.TPAYMENTDETAIL.Add(d));

            //Temp Process 
            payment.VTPAYMENTDETAIL.Clear();
            payment.VTPAYMENTDETAIL.AddRange(_context.sp_PaymentDetail_GetFromEmployee(payment.UNITCODE, payment.STARTDATE.Date, payment.ENDDATE.Date));

            List<TCALENDAR> holidays = _context.TCALENDAR.Where(d => d.UNITCODE.Equals(payment.UNITCODE) && d.DTDATE.Date >= startPeriod.Date && d.DTDATE.Date <= endDate.Date).ToList();
            List<sp_Attendance_GetGroupingByUnitAndDate_Result> skuAttendances = _context.sp_Attendance_GetGroupingByUnitAndDate(payment.UNITCODE, payment.STARTDATE.Date, payment.ENDDATE.Date, hkCalcByFingerPrint).ToList();
            List<sp_Attendance_GetGroupingByUnitAndDate_Result> bhlAttendances = _context.sp_Attendance_GetGroupingByUnitAndDate(payment.UNITCODE, payment.STARTDATE.Date, payment.ENDDATE.Date, hkCalcByFingerPrint).ToList();
            List<TSALARYTYPEMAP> salaryMaps = _context.sp_TSALARYTYPEMAP_GetActive(payment.UNITCODE).ToList();
            List<TMANDORFINE> mandorFines = _context.sp_TMANDORFINE_GetGroup(payment.UNITCODE, payment.STARTDATE.Date, payment.ENDDATE.Date).ToList();

            CalculateAttendanceAndHk(startPeriod, endDate, payment, holidays, skuAttendances, bhlAttendances);
            CalculateBasicWages(payment, dayPerMonth);
            CalculatePremiHadir(payment);
            //CalculatePremiHadirPanen(startPeriod, endDate, scheme, payment, holidays);
            CalculatePremiNonPanen(startPeriod, startDate, endDate, payment);
            CalculateOvertime(startPeriod, endDate, payment, scheme, ricePrice, hkCalcByFingerPrint);
            CalculateJamsostek(payment, scheme, salaryTypes, jamsostek);

            if (HelperService.GetConfigValue(PMSConstants.CfgPayrollNaturaVersionNumber + payment.UNITCODE, _context) == "2")
            {
                if (HelperService.GetConfigValue(PMSConstants.CfgPayrollNaturaVersion + payment.UNITCODE, _context) == "2")
                    CalculateRiceBw2(payment, scheme, period, holidays, ricePrice, hkCalcByFingerPrint);
                else
                    CalculateRice2(payment, scheme, ricePrice, bhlNatura);
            }
            else
            {
                if (HelperService.GetConfigValue(PMSConstants.CfgPayrollNaturaVersion + payment.UNITCODE, _context) == "2")
                    CalculateRiceBw(payment, scheme, period, holidays, ricePrice, hkCalcByFingerPrint);
                else
                    CalculateRice(payment, scheme, ricePrice, bhlNatura);
            }

            CalculatePremiPanenMandor(payment, scheme, startPeriod, endDatePeriod1, mandorFines);
            //CalculateInsentifPengawas(payment);
            CalculatePremi(payment, salaryTypes, salaryMaps);
            SumSalaryItem(payment, period, salaryTypes);
            CalculateTaxYtd(payment, jamsostek);

            foreach (var d in payment.TPAYMENTDETAIL)
            {
                if (payment.PERIOD == 2)
                {
                    d.TOTALSALARY = d.BASICWAGES + d.PREMIPANEN + d.PREMINONPANEN + d.PREMIHADIR - d.PENALTY
                        + d.NATURAINCOME + d.OVERTIME + d.TAXINCENTIVE + d.INCENTIVE - d.PERIOD1 - d.TAX - d.JAMSOSTEKDEDUCTION
                        - d.KOPERASI - d.DEBIT - d.NATURADEDUCTION - d.PREMIPANENKONTAN;
                }

                if (payment.PERIOD == 1)
                {
                    if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                        d.TOTALSALARY = d.PERIOD1;

                    if (d.EMPTYPE.ToUpper().StartsWith("BHL"))
                    {
                        d.TOTALSALARY = d.BASICWAGES + d.PREMIPANEN + d.PREMINONPANEN - d.PENALTY
                            + d.NATURAINCOME + d.OVERTIME + d.TAXINCENTIVE + d.INCENTIVE - d.PERIOD1 - d.TAX
                            - d.JAMSOSTEKDEDUCTION - d.KOPERASI - d.DEBIT - d.NATURADEDUCTION - d.PREMIPANENKONTAN;
                    }
                }
            }

            //payment.Details.RemoveAll(d => d.TotalSalary <= 0 );

            var itemGroup = (from i in payment.TPAYMENTITEM
                             group i by new { i.TYPEID, i.EMPID, }
                            into g
                             select new { g.Key.TYPEID, g.Key.EMPID, AMOUNT = g.Select(x => x.AMOUNT).Sum() }).ToList();
            payment.TPAYMENTITEM.Clear();
            foreach (var item in itemGroup)
            {
                payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = item.TYPEID, EMPID = item.EMPID, AMOUNT = item.AMOUNT });
            }

            //var qTotalItem = (from m in payment.Items
            //                  join n in salaryTypes
            //                      on m.EMPTYPE equals n.Id
            //                  select new { m.EMPID, m.EMPTYPE, n.Deduct, AMOUNT = n.Deduct ? -m.AMOUNT : m.AMOUNT }).ToList();

            //var qItemTotal = from i in qTotalItem select i.AMOUNT;
            //var itemTotal = qItemTotal.Sum();

            var qTotal = from d in payment.TPAYMENTDETAIL select d.TOTALSALARY;
            var total = qTotal.Sum();

            //if (Math.Abs(itemTotal - total) > 1000) throw new Exception("Total Salary");

            payment.TOTAL = total;

        }

        private static void CalculateAttendanceAndHk(DateTime startPeriod, DateTime endDate, TPAYMENT payment,
    IEnumerable<TCALENDAR> holidays, List<sp_Attendance_GetGroupingByUnitAndDate_Result> skuAttendances, List<sp_Attendance_GetGroupingByUnitAndDate_Result> bhlAttendances)
        {
            int totalDay = 0;
            int totalHoliday = 0;
            int totalSunday = 0;
            if (payment.PERIOD == 2)
            {
                var h = from itm in holidays where itm.HOLIDAY select itm;
                var s = from itm in holidays where itm.SUNDAY select itm;
                totalDay = (endDate - startPeriod).Days + 1;
                totalHoliday = h.Count();
                totalSunday = s.Count();
            }

            foreach (var d in payment.TPAYMENTDETAIL)
            {
                d.DAYS = (Int16)totalDay;
                d.HOLIDAY = (Int16)totalHoliday;
                d.SUNDAY = (Int16)totalSunday;

                if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                {
                    var present = skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.PRESENT == 1).Select(itm => itm.HK);
                    d.PRESENT = (Int16)present.Sum();

                    if (payment.PERIOD == 2)
                    {
                        //d.HKC = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "C").Select(itm => itm.HK).AsEnumerable().Sum());
                        d.HKC = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "C").AsEnumerable().Sum(x => x.HK));

                        d.HKH1 = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "H1").AsEnumerable().Sum(x => x.HK));

                        d.HKH2 = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "H2").AsEnumerable().Sum(x => x.HK));

                        d.HKP1 = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "P1").AsEnumerable().Sum(x => x.HK));

                        d.HKP2 = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "P2").AsEnumerable().Sum(x => x.HK));

                        d.HKP3 = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "P3").AsEnumerable().Sum(x => x.HK));

                        d.HKP4 = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "P4").AsEnumerable().Sum(x => x.HK));

                        d.HKS1 = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "S1").AsEnumerable().Sum(x => x.HK));

                        d.HKS2 = Convert.ToInt16(skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "S2").AsEnumerable().Sum(x => x.HK));

                        d.MANGKIR = d.DAYS - (d.PRESENT + d.HKC + d.HKH1 + d.HKH2 + d.HKP1 + d.HKP2 + d.HKP3 + d.HKP4 + d.HKS1 + d.HKS2 + d.HOLIDAY + d.SUNDAY);
                        if (d.MANGKIR < 0) d.MANGKIR = 0;
                    }

                    var hk = skuAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "K").Select(itm => itm.HK);
                    hk.Sum();
                    d.HK = Convert.ToDecimal(hk.Sum()) + d.HKC + d.HKH1 + d.HKH2 + d.HKP2 + d.HKP3 + d.HKP4 + d.HKS1 + d.HKS2;
                }
                if (d.EMPTYPE.ToUpper().StartsWith("BHL"))
                {
                    var hk = bhlAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.ABSENTCODE == "K").Select(itm => itm.HK);
                    d.HK = Convert.ToDecimal(hk.Sum());

                    var present = bhlAttendances.Where(itm => itm.EMPLOYEEID == d.EMPID && itm.PRESENT == 1).Select(itm => itm.HK);
                    d.PRESENT = (Int16)present.Sum();
                }
            }
        }

        private void CalculateBasicWages(TPAYMENT payment, int dayPerMonth)
        {
            foreach (var d in payment.TPAYMENTDETAIL)
            {
                //if (!d.EMPTYPE.ToUpper().StartsWith("SKU"))
                //    d.PERIOD1 = 0;

                decimal basicWages = d.BASICWAGES;
                decimal basicAmount = 0;
                decimal brutoAmount = 0;

                if (d.EMPTYPE.ToUpper().StartsWith("BHL"))
                {
                    basicAmount = d.HK * basicWages;
                    brutoAmount = d.HK * basicWages;
                }
                else if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                {
                    if (payment.PERIOD == 2)
                    {
                        brutoAmount = basicWages;

                        //Resign
                        var emp = _context.MEMPLOYEE.Where(r => r.EMPID.Equals(d.EMPID)).FirstOrDefault();
                        if (emp.STATUS != "A")
                        {
                            basicAmount = d.HK * (basicWages / dayPerMonth);
                        }
                        else
                        {
                            decimal hke = d.DAYS - d.SUNDAY - d.HOLIDAY;
                            var hkPaid = Convert.ToDecimal(d.HK);
                            if (hkPaid > hke) hkPaid = hke;
                            basicAmount = basicWages * (hkPaid / hke);
                        }
                    }
                    else
                    {
                        basicAmount = 0;
                        brutoAmount = 0;
                    }
                }

                d.BASICWAGESBRUTO = brutoAmount;
                d.BASICWAGES = basicAmount;
                if (basicAmount > 0)
                {
                    payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeBasicWages, EMPID = d.EMPID, AMOUNT = basicAmount });
                }

                //if (payment.PERIOD == 2)
                //{
                //    if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                //        d.KOPERASI += d.SPSI;
                //    else
                //    {
                //        d.KOPERASI = 0;
                //        d.SPSI = 0;
                //    }

                //    if (d.EMPTYPE.StartsWith("SKU"))
                //    {
                //        decimal basicWages = d.BASICWAGES;
                //        if (d.Resign)
                //        {
                //            d.BASICWAGES = d.HK * (basicWages / dayPerMonth);
                //        }
                //        else
                //        {
                //            decimal hke = d.Days - d.Sunday - d.Holiday;
                //            var hkPaid = d.HK;
                //            if (hkPaid > hke) hkPaid = hke;
                //            d.BASICWAGES = basicWages * (hkPaid / hke);
                //        }

                //        d.BASICWAGESBRUTO = basicWages;
                //    }
                //}
                //else
                //{
                //    d.KOPERASI = 0;
                //    d.SPSI = 0;
                //    if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                //    {
                //        d.BASICWAGES = 0;
                //        d.BASICWAGESBRUTO = 0;
                //    }
                //}
            }
        }

        private void CalculatePremiHadir(TPAYMENT payment)
        {
            if (payment.PERIOD == 2)
            {
                //bool isLebaran =
                //    PMSServices.Helper.GetConfigValue(PMSConstants.CfgPayrollPremiHadirActivated + payment.UNITCODE)
                //    == "IED";

                //var qHadir = (from i in payment.THARVESTRESULT1
                //              where i.HasilPanen >= i.NewBasis1 && i.Base1 > 0
                //              && i.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah
                //              group i by new { i.EMPLOYEEID }
                //             into g
                //              select new { g.Key.EMPLOYEEID, Hadir = g.Select(x => x.HARVESTDATE).Distinct().Count() }).ToList();

                var qHadir = (from i in payment.THARVESTRESULT1
                              where i.BASE1 > 0 && i.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah
                              group i by new { i.EMPLOYEEID }
                             into g
                              select new { g.Key.EMPLOYEEID, Hadir = g.Select(x => x.HARVESTDATE).Distinct().Count() }).ToList();

                var qNormal = (from i in payment.THARVESTRESULT1
                               where i.HASILPANEN >= i.NEWBASIS1
                               && i.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah
                               && i.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                               group i by new { i.EMPLOYEEID }
                             into g
                               select new { g.Key.EMPLOYEEID, Hadir = g.Select(x => x.HARVESTDATE).Distinct().Count() }).ToList();

                var qKontan = (from i in payment.THARVESTRESULT1
                               where i.HASILPANEN >= i.NEWORIBASIS1
                               && i.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah
                               && i.HARVESTPAYMENTTYPE == PMSConstants.PayTypeKontanan
                               group i by new { i.EMPLOYEEID }
                             into g
                               select new { g.Key.EMPLOYEEID, Hadir = g.Select(x => x.HARVESTDATE).Distinct().Count() }).ToList();

                foreach (var item in qHadir.ToList())
                {
                    //var hadirCount = item.Hadir;
                    //if (hadirCount > 25) hadirCount = 25;

                    //var q = from itm in payment.Details where itm.EMPLOYEEID == item.EMPLOYEEID select itm;
                    //PaymentDetail detail = q.FirstOrDefault();
                    //if (q.Count() > 0)
                    //{
                    //    if (hadirCount >= 20 && hadirCount <= 22)
                    //    {
                    //        detail.PREMIHADIR = 10000m * hadirCount;
                    //        //if (isLebaran) detail.PREMIHADIR += 1500000m;
                    //    }
                    //    else if (item.Hadir >= 23)
                    //    {
                    //        detail.PREMIHADIR = 20000m * hadirCount;
                    //        //if (isLebaran) detail.PREMIHADIR += 3000000m;
                    //    }
                    //}

                    var hadirCount = item.Hadir;

                    var normalCount = 0;
                    var qBasisNormal = from itm in qNormal where itm.EMPLOYEEID == item.EMPLOYEEID select itm;
                    if (qBasisNormal.Count() > 0) normalCount = qBasisNormal.FirstOrDefault().Hadir;

                    var kontanCount = 0;
                    var qBasisKontan = from itm in qKontan where itm.EMPLOYEEID == item.EMPLOYEEID select itm;
                    if (qBasisKontan.Count() > 0) kontanCount = qBasisKontan.FirstOrDefault().Hadir;

                    var premiHadirCount = normalCount + kontanCount;
                    if (premiHadirCount > hadirCount) premiHadirCount = hadirCount;

                    var q = from itm in payment.TPAYMENTDETAIL where itm.EMPID == item.EMPLOYEEID select itm;
                    TPAYMENTDETAIL detail = q.FirstOrDefault();
                    if (q.Count() > 0)
                    {
                        decimal premiAmount = 0;
                        if (premiHadirCount >= 20 && premiHadirCount <= 22)
                        {
                            premiAmount = 10000m * premiHadirCount;
                            //if (isLebaran) detail.PREMIHADIR += 1500000m;
                        }
                        else if (premiHadirCount >= 23)
                        {
                            premiAmount = 20000m * premiHadirCount;
                            //if (isLebaran) detail.PREMIHADIR += 3000000m;
                        }

                        if (premiAmount > 0)
                        {
                            detail.PREMIHADIR += premiAmount;
                            payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodePremiHadirPanen, EMPID = detail.EMPID, AMOUNT = premiAmount });
                        }
                    }
                }
            }
        }

        private void CalculatePremiNonPanen(DateTime startPeriod, DateTime startDate, DateTime endDate, TPAYMENT payment)
        {
            List<sp_PaymentDetail_GetPremiNonPanen_Result> premiNonPanen = _context.sp_PaymentDetail_GetPremiNonPanen(payment.UNITCODE, startPeriod, startDate.Date, endDate.Date).ToList();
            foreach (var r in premiNonPanen)
            {
                var q = from itm in payment.TPAYMENTDETAIL where itm.EMPID == r.EMPLOYEEID select itm;
                TPAYMENTDETAIL detail = q.FirstOrDefault();

                //if (q.Count() == 0)
                //    throw new Exception("Karyawan dengan kode " + r["EMPLOYEEID"] + " tidak terdaftar/tidak aktif.");

                var premiType = r.TYPE;
                var premiAmount = Convert.ToDecimal(r.PREMIAMOUNT);

                if (detail.EMPTYPE.ToUpper() == "BHL")
                {
                    detail.PREMINONPANEN += Convert.ToDecimal(premiAmount);
                    if (premiType == "BOR")
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeBorongan, EMPID = detail.EMPID, AMOUNT = premiAmount });
                    else
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodePremiNonPanen, EMPID = detail.EMPID, AMOUNT = premiAmount });
                }

                if (payment.PERIOD == 2)
                {
                    if (detail.EMPTYPE.ToUpper().StartsWith("SKU"))
                    {
                        detail.PREMINONPANEN += premiAmount;
                        if (premiType == "BOR")
                            payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeBorongan, EMPID = detail.EMPID, AMOUNT = premiAmount });
                        else
                            payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodePremiNonPanen, EMPID = detail.EMPID, AMOUNT = premiAmount });
                    }

                    //detail.INCENTIVE = Convert.ToDecimal(r["PREMIHK"]);
                }
            }
        }

        private void CalculateOvertime(DateTime startPeriod, DateTime endDate, TPAYMENT payment, MPAYMENTSCHEME scheme, decimal ricePrice, bool useFingerprint)
        {
            List<sp_PaymentDetail_GetOvertime_Result> overtimes = _context.sp_PaymentDetail_GetOvertime(payment.UNITCODE, startPeriod.Date, endDate.Date, useFingerprint).ToList();
            if (payment.PERIOD == 2)
            {
                foreach (var r in overtimes)
                {
                    var q = from itm in payment.TPAYMENTDETAIL where itm.EMPID == r.EMPLOYEEID select itm;
                    TPAYMENTDETAIL detail = q.FirstOrDefault();
                    if (detail != null)
                    {
                        detail.OVERTIMEHOUR =
                            (Convert.ToDecimal(r.OT150) * 1.5M) + (Convert.ToDecimal(r.OT200) * 2)
                            + (Convert.ToDecimal(r.OT300) * 3) + (Convert.ToDecimal(r.OT400) * 4);//*Test
                        //detail.OVERTIME = detail.OvertimeHour 

                        decimal basicWages = detail.BASICWAGES;
                        if (detail.EMPTYPE.ToUpper().StartsWith("SKUB")) basicWages = detail.BASICWAGESBRUTO;
                        else if (detail.EMPTYPE.ToUpper().StartsWith("SKUH")) basicWages = detail.BASICWAGESBRUTO;
                        else if (detail.EMPTYPE.ToUpper().StartsWith("BHL")) basicWages = scheme.PROVINCEWAGES;

                        if (!scheme.OVERTIMERICEADD) ricePrice = 0;

                        var ovtAmount = ((basicWages + (15 * ricePrice)) / 173) * detail.OVERTIMEHOUR;//*Test  //-*Parameter
                        //var ovtAmount = (basicWages / 173) * detail.OvertimeHour;//*Test  //-*Parameter
                        if (ovtAmount > 0)
                        {
                            detail.OVERTIME = ovtAmount;
                            payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeOvertime, EMPID = detail.EMPID, AMOUNT = ovtAmount });
                        }

                        var debAmount = Convert.ToDecimal(r.DEBIT);
                        if (debAmount > 0)
                        {
                            detail.DEBIT = debAmount;
                            payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeDebit, EMPID = detail.EMPID, AMOUNT = debAmount });
                        }
                    }
                }
            }
        }

        private void CalculateJamsostek(TPAYMENT payment, MPAYMENTSCHEME scheme, List<MSALARYTYPE> salaryTypes, MJAMSOSTEK jamsostek)
        {
            if (payment.PERIOD == 2 && jamsostek != null)
            {
                //bool bhlJamsostek = false;
                //if (PMSServices.Helper.GetConfigValue(PMSConstants.CfgPayrollJamsostekBhl + payment.UNITCODE) == PMSConstants.CfgPayrollJamsostekBhlTrue)
                //    bhlJamsostek = true;

                foreach (var d in payment.VTPAYMENTDETAIL)
                {
                    decimal jkkAmount = 0;
                    decimal jkmAmount = 0;
                    decimal jhtEmpAmount = 0;
                    decimal jhtCompAmount = 0;
                    decimal kesEmpAmount = 0;
                    decimal kesCompAmount = 0;
                    decimal jpEmpAmount = 0;
                    decimal jpCompAmount = 0;

                    //Not Resign
                    var emp = _context.MEMPLOYEE.Where(r => r.EMPID.Equals(d.EMPID)).FirstOrDefault();
                    if (emp.STATUS == "A")
                    //if (d.EMPTYPE.ToUpper().StartsWith("SKU")
                    //    || (d.EMPTYPE == "BHL" && bhlJamsostek))
                    {
                        //decimal basicWages = 0;
                        //if (d.EMPTYPE == "SKUB") basicWages = d.BASICWAGESBRUTO;
                        //else if (d.EMPTYPE == "SKUH") basicWages = d.BASICWAGESBRUTO;
                        //else if (d.EMPTYPE == "BHL") basicWages = scheme.ProvinceWages;

                        decimal basicWages = d.BPJSBASE;
                        decimal bpjsKesBase = d.BPJSKESBASE;

                        payment.TPAYMENTATTREMP.Add(new TPAYMENTATTREMP
                        {
                            EMPID = d.EMPID,
                            TYPEID = PMSConstants.SalaryAttributeEmployeeBpjsBase,
                            NVALUE = d.BPJSBASE,
                            TVALUE = ""
                        });

                        payment.TPAYMENTATTREMP.Add(new TPAYMENTATTREMP
                        {
                            EMPID = d.EMPID,
                            TYPEID = PMSConstants.SalaryAttributeEmployeeBpjsKesehatanBase,
                            NVALUE = d.BPJSKESBASE,
                            TVALUE = ""
                        });

                        if (Convert.ToBoolean(d.BPJSJKK))
                        {
                            jkkAmount = basicWages * (jamsostek.JKK / 100);
                            jkmAmount = basicWages * (jamsostek.JKM / 100);
                        }

                        if (Convert.ToBoolean(d.BPJSJHT))
                        {
                            jhtEmpAmount = basicWages * (jamsostek.EMPLOYEE / 100);
                            jhtCompAmount = basicWages * (jamsostek.ENTERPRISE / 100);

                            var jpBasic = basicWages;
                            if (jpBasic > jamsostek.JPMAX) jpBasic = jamsostek.JPMAX;
                            jpEmpAmount = jpBasic * (jamsostek.JPEMP / 100);
                            jpCompAmount = jpBasic * (jamsostek.JPCOMP / 100);
                        }

                        if (d.BPJSKES)
                        {
                            kesEmpAmount = bpjsKesBase * (jamsostek.KESEMP / 100);
                            kesCompAmount = bpjsKesBase * (jamsostek.KESCOMP / 100);
                        }
                    }
                    //else if (d.EMPTYPE == "BHLP")
                    //{
                    //    decimal basicWages = scheme.ProvinceWages;

                    //    jkkAmount = basicWages * (jamsostek.JKK / 100);
                    //    jkmAmount = basicWages * (jamsostek.JKM / 100);
                    //}

                    if (jkkAmount > 0)
                    {
                        var type = (from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeJamsosJkk select itm).FirstOrDefault();
                        if (type != null) payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = jkkAmount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJkkId, EMPID = d.EMPID, AMOUNT = jkkAmount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJkkEmp, EMPID = d.EMPID, AMOUNT = jkkAmount });
                    }

                    if (jkmAmount > 0)
                    {
                        var type = (from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeJamsosJkm select itm).FirstOrDefault();
                        if (type != null) payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = jkmAmount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJkmId, EMPID = d.EMPID, AMOUNT = jkmAmount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJkmEmp, EMPID = d.EMPID, AMOUNT = jkmAmount });
                    }

                    if (jhtEmpAmount > 0)
                    {
                        var type = (from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeJamsosJhtEmp select itm).FirstOrDefault();
                        if (type != null) payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = jhtEmpAmount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJhtEmpId, EMPID = d.EMPID, AMOUNT = jhtEmpAmount });
                    }

                    if (jhtCompAmount > 0)
                    {
                        var type = (from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeJamsosJhtComp select itm).FirstOrDefault();
                        if (type != null) payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = jhtCompAmount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJhtCompId, EMPID = d.EMPID, AMOUNT = jhtCompAmount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJhtCompDcId, EMPID = d.EMPID, AMOUNT = jhtCompAmount });
                    }

                    if (kesEmpAmount > 0)
                    {
                        var type = (from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeJamsosKesEmp select itm).FirstOrDefault();
                        if (type != null) payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = kesEmpAmount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosKesEmpId, EMPID = d.EMPID, AMOUNT = kesEmpAmount });
                    }

                    if (kesCompAmount > 0)
                    {
                        var type = (from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeJamsosKesComp select itm).FirstOrDefault();
                        if (type != null) payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = kesCompAmount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosKesCompId, EMPID = d.EMPID, AMOUNT = kesCompAmount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosKesCompDcId, EMPID = d.EMPID, AMOUNT = kesCompAmount });
                    }

                    if (jpEmpAmount > 0)
                    {
                        var type = (from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeJamsosJpEmp select itm).FirstOrDefault();
                        if (type != null) payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = jpEmpAmount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJpEmpId, EMPID = d.EMPID, AMOUNT = jpEmpAmount });
                    }

                    if (jpCompAmount > 0)
                    {
                        var type = (from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeJamsosJpComp select itm).FirstOrDefault();
                        if (type != null) payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = jpCompAmount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJpCompId, EMPID = d.EMPID, AMOUNT = jpCompAmount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeJamsosJpCompDcId, EMPID = d.EMPID, AMOUNT = jpCompAmount });
                    }

                    d.JAMSOSTEKINCENTIVE = jkkAmount + jkmAmount + jhtCompAmount + kesCompAmount + jpCompAmount;
                    d.JAMSOSTEKDEDUCTION = jhtEmpAmount + kesEmpAmount + jpEmpAmount;

                    //Copy value To TPAYMENTDETAIL
                    foreach (var row in payment.TPAYMENTDETAIL.Where(r => r.EMPID.Equals(d.EMPID)))
                    {
                        row.JAMSOSTEKINCENTIVE = d.JAMSOSTEKINCENTIVE;
                        row.JAMSOSTEKDEDUCTION = d.JAMSOSTEKDEDUCTION;
                    }

                }
            }
        }

        private void CalculateRice(TPAYMENT payment, MPAYMENTSCHEME scheme, decimal ricePrice, bool bhlEnable)
        {
            if (payment.PERIOD == 2)
            {
                var familyStatus = _context.MSTATUS.Where(d => d.ACTIVE.Equals(true));

                foreach (var d in payment.TPAYMENTDETAIL)
                {
                    //if (d.NaturaCalculate)
                    //{
                    var s = from itm in familyStatus where itm.STATUSID == d.STATUSID select itm;
                    MSTATUS fs = s.FirstOrDefault();

                    decimal riceEmployee = 0;
                    decimal riceWife = 0;
                    decimal riceChild = 0;

                    decimal naturaIncome = 0;
                    decimal naturaIncomeEmployee = 0;
                    decimal natura = 0;

                    if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                    {
                        //if (d.EMPTYPE.ToUpper() == "SKUH")
                        //{
                        decimal dayOut = 0;
                        if (d.DAYS - d.SUNDAY - d.HOLIDAY - d.HK > 0)
                            dayOut = d.DAYS - d.SUNDAY - d.HOLIDAY - d.HK;
                        //riceEmployee = dayOut * scheme.RiceEmployee / 25; //-*Parameter
                        //riceWife = dayOut * scheme.RiceWife / 25; //-*Parameter
                        //riceChild = dayOut * scheme.RiceChildren / 25; //-*Parameter

                        decimal hke = d.DAYS - d.SUNDAY - d.HOLIDAY;
                        riceEmployee = dayOut * scheme.RICEEMPLOYEE / hke; //-*Parameter
                        riceWife = dayOut * scheme.RICEWIFE / hke; //-*Parameter
                        riceChild = dayOut * scheme.RICECHILDREN / hke; //-*Parameter
                                                                        //}

                        naturaIncome = ((scheme.RICEEMPLOYEE - riceEmployee)
                                          + (fs.SPOUSE * (scheme.RICEWIFE - riceWife))
                                          + (fs.CHILDREN * (scheme.RICECHILDREN - riceChild)))
                                         * ricePrice;
                        naturaIncomeEmployee = (scheme.RICEEMPLOYEE - riceEmployee) * ricePrice;
                        natura = (scheme.RICEEMPLOYEE - riceEmployee)
                                    + (fs.SPOUSE * (scheme.RICEWIFE - riceWife))
                                    + (fs.CHILDREN * (scheme.RICECHILDREN - riceChild));
                    }
                    //else if (d.EMPTYPE.ToUpper().StartsWith("BHL"))
                    //{
                    //    if (bhlEnable)
                    //    {
                    //        d.NATURA = scheme.RiceEmployee / 25 * d.HK;
                    //        d.NATURAINCOME = d.NATURA * ricePrice;
                    //        d.NaturaIncomeEmployee = d.NATURA * ricePrice;
                    //    }
                    //}

                    d.RICEPAIDASMONEY = scheme.RICEPAIDASMONEY;
                    var naturaDeduction = d.RICEPAIDASMONEY.ToUpper() == "UANG" ? 0 : naturaIncome;

                    //if (d.NATURAINCOME < 0) d.NATURAINCOME = 0;
                    //if (d.NATURA < 0) d.NATURA = 0;

                    if (naturaIncome > 0)
                    {
                        d.NATURAINCOME += naturaIncome;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeNaturaIncome, EMPID = d.EMPID, AMOUNT = naturaIncome });
                    }

                    if (naturaDeduction > 0)
                    {
                        d.NATURADEDUCTION += naturaDeduction;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeNaturaDeduction, EMPID = d.EMPID, AMOUNT = naturaDeduction });
                    }

                    if (naturaIncomeEmployee > 0) d.NATURAINCOMEEMPLOYEE += naturaIncomeEmployee;
                    if (natura > 0) d.NATURA += natura;

                    //}
                }
            }
        }

        private void CalculateRiceBw(TPAYMENT payment, MPAYMENTSCHEME scheme, MPERIOD period, List<TCALENDAR> holidays, decimal ricePrice, bool hkCalcByFingerPrint)
        {
            if (payment.PERIOD == 2)
            {
                var familyStatus = _context.MSTATUS.Where(d => d.ACTIVE.Equals(true));

                var attendaces = _context.sp_Attendance_GetNaturaSunday(payment.UNITCODE, period.FROM1, period.TO2, hkCalcByFingerPrint);

                var sunday = from itm in holidays where itm.SUNDAY select itm;
                decimal sundayCount = sunday.Count();

                foreach (var d in payment.TPAYMENTDETAIL)
                {
                    //if (d.NaturaCalculate)
                    //{
                    var s = from itm in familyStatus where itm.STATUSID == d.STATUSID select itm;
                    MSTATUS fs = s.FirstOrDefault();

                    decimal riceEmployee = 0;
                    decimal riceWife = 0;
                    decimal riceChild = 0;

                    decimal naturaIncome = 0;
                    decimal naturaIncomeEmployee = 0;
                    decimal natura = 0;

                    if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                    {
                        //if (d.EMPTYPE.ToUpper() == "SKUH")
                        //{
                        decimal sundayMiss = sundayCount;
                        var q = from itm in attendaces where itm.EMPLOYEEID == d.EMPID select itm;
                        var singleOrDefault = q.FirstOrDefault();
                        if (singleOrDefault != null) sundayMiss = sundayCount - Convert.ToDecimal(singleOrDefault.HK);

                        decimal dayOut = d.MANGKIR + d.HKP1 + sundayMiss;
                        riceEmployee = dayOut * scheme.RICEEMPLOYEE / 30;
                        riceWife = dayOut * scheme.RICEWIFE / 30;
                        riceChild = dayOut * scheme.RICECHILDREN / 30;
                        //}

                        natura = (scheme.RICEEMPLOYEE - riceEmployee)
                                   + (fs.SPOUSE * (scheme.RICEWIFE - riceWife))
                                   + (fs.CHILDREN * (scheme.RICECHILDREN - riceChild));
                        naturaIncomeEmployee = (scheme.RICEEMPLOYEE - riceEmployee) * ricePrice;
                        naturaIncome = natura * ricePrice;
                    }
                    else if (d.EMPTYPE.ToUpper().StartsWith("BHL"))
                    {
                        //if (d.JOINTDATE < new DateTime(2016, 1, 1))
                        //{
                        var pos = _context.MPOSITION.Where(x => x.POSITIONID.Equals(d.POSITIONID)).FirstOrDefault();
                        //if (pos.Flag == "6")
                        if (pos.POSFLAG == 6 || d.JOINTDATE < new DateTime(2016, 1, 1))
                        {
                            natura = (scheme.RICEEMPLOYEE / 30) * (d.PRESENT);
                            if (natura > scheme.RICEEMPLOYEE) natura = scheme.RICEEMPLOYEE;

                            naturaIncome = natura * ricePrice;
                            naturaIncomeEmployee = natura * ricePrice;
                        }
                        //}
                    }

                    d.RICEPAIDASMONEY = scheme.RICEPAIDASMONEY;
                    var naturaDeduction = d.RICEPAIDASMONEY.ToUpper() == "UANG" ? 0 : naturaIncome;

                    //if (d.NATURAINCOME < 0) d.NATURAINCOME = 0;
                    //if (d.NATURA < 0) d.NATURA = 0;

                    if (naturaIncome > 0)
                    {
                        d.NATURAINCOME += naturaIncome;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeNaturaIncome, EMPID = d.EMPID, AMOUNT = naturaIncome });
                    }

                    if (naturaDeduction > 0)
                    {
                        d.NATURADEDUCTION += naturaDeduction;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeNaturaDeduction, EMPID = d.EMPID, AMOUNT = naturaDeduction });
                    }

                    if (naturaIncomeEmployee > 0) d.NATURAINCOMEEMPLOYEE += naturaIncomeEmployee;
                    if (natura > 0) d.NATURA += natura;
                    //}
                }
            }
        }

        private void CalculateRice2(TPAYMENT payment, MPAYMENTSCHEME scheme, decimal ricePrice, bool bhlEnable)
        {
            if (payment.PERIOD == 2)
            {
                var familyStatus = _context.MSTATUS.Where(d => d.ACTIVE.Equals(true));

                foreach (var d in payment.VTPAYMENTDETAIL)
                {
                    var standRiceEmployee = scheme.RICEEMPLOYEE;
                    var standRiceWife = scheme.RICEWIFE;
                    var standRiceChild = scheme.RICECHILDREN;

                    if (d.NATURACALC)
                        standRiceWife = 0;
                    else if (d.NATURACALC)
                    {
                        standRiceEmployee = scheme.RICEWIFE;
                        standRiceWife = 0;
                        standRiceChild = 0;
                    }

                    var s = from itm in familyStatus where itm.STATUSID == d.STATUSID select itm;
                    MSTATUS fs = s.FirstOrDefault();

                    decimal riceEmployee = 0;
                    decimal riceWife = 0;
                    decimal riceChild = 0;

                    decimal naturaIncome = 0;
                    decimal naturaIncomeEmployee = 0;
                    decimal natura = 0;

                    if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                    {
                        decimal dayOut = 0;
                        if (d.DAYS - d.SUNDAY - d.HOLIDAY - d.HK > 0)
                            dayOut = d.DAYS - d.SUNDAY - d.HOLIDAY - d.HK;

                        decimal hke = d.DAYS - d.SUNDAY - d.HOLIDAY;
                        riceEmployee = dayOut * standRiceEmployee / hke;
                        riceWife = dayOut * standRiceWife / hke;
                        riceChild = dayOut * standRiceChild / hke;

                        naturaIncome = ((standRiceEmployee - riceEmployee)
                                          + (fs.SPOUSE * (standRiceWife - riceWife))
                                          + (fs.CHILDREN * (standRiceChild - riceChild)))
                                         * ricePrice;
                        naturaIncomeEmployee = (standRiceEmployee - riceEmployee) * ricePrice;
                        natura = (standRiceEmployee - riceEmployee)
                                    + (fs.SPOUSE * (standRiceWife - riceWife))
                                    + (fs.CHILDREN * (standRiceChild - riceChild));
                    }

                    d.RICEPAIDASMONEY = scheme.RICEPAIDASMONEY;
                    var naturaDeduction = d.RICEPAIDASMONEY.ToUpper() == "UANG" ? 0 : naturaIncome;

                    if (naturaIncome > 0)
                    {
                        d.NATURAINCOME += naturaIncome;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeNaturaIncome, EMPID = d.EMPID, AMOUNT = naturaIncome, });
                    }

                    if (naturaDeduction > 0)
                    {
                        d.NATURADEDUCTION += naturaDeduction;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeNaturaDeduction, EMPID = d.EMPID, AMOUNT = naturaDeduction, });
                    }

                    if (naturaIncomeEmployee > 0) d.NATURAINCOMEEMPLOYEE += naturaIncomeEmployee;
                    if (natura > 0) d.NATURA += natura;

                    //Copy value To TPAYMENTDETAIL
                    foreach (var row in payment.TPAYMENTDETAIL.Where(r => r.EMPID.Equals(d.EMPID)))
                    {
                        row.RICEPAIDASMONEY = d.RICEPAIDASMONEY;
                        row.NATURAINCOME = d.NATURAINCOME;
                        row.NATURADEDUCTION = d.NATURADEDUCTION;
                        row.NATURAINCOMEEMPLOYEE = d.NATURAINCOMEEMPLOYEE;
                        row.NATURA = d.NATURA;
                    }
                }
            }
        }

        private void CalculateRiceBw2(TPAYMENT payment, MPAYMENTSCHEME scheme, MPERIOD period, List<TCALENDAR> holidays, decimal ricePrice, bool hkCalcByFingerPrint)
        {
            if (payment.PERIOD == 2)
            {
                var familyStatus = _context.MSTATUS.Where(d => d.ACTIVE.Equals(true));

                var attendaces = _context.sp_Attendance_GetNaturaSunday(payment.UNITCODE, period.FROM1, period.TO2, hkCalcByFingerPrint);

                var sunday = from itm in holidays where itm.SUNDAY select itm;
                decimal sundayCount = sunday.Count();

                foreach (var d in payment.VTPAYMENTDETAIL)
                {
                    var standRiceEmployee = scheme.RICEEMPLOYEE;
                    var standRiceWife = scheme.RICEWIFE;
                    var standRiceChild = scheme.RICECHILDREN;

                    if (d.NATURACALC)
                        standRiceWife = 0;
                    else if (d.NATURACALC)
                    {
                        standRiceEmployee = scheme.RICEWIFE;
                        standRiceWife = 0;
                        standRiceChild = 0;
                    }

                    var s = from itm in familyStatus where itm.STATUSID == d.STATUSID select itm;
                    MSTATUS fs = s.FirstOrDefault();

                    decimal riceEmployee = 0;
                    decimal riceWife = 0;
                    decimal riceChild = 0;

                    decimal naturaIncome = 0;
                    decimal naturaIncomeEmployee = 0;
                    decimal natura = 0;

                    if (d.EMPTYPE.ToUpper().StartsWith("SKU"))
                    {
                        decimal sundayMiss = sundayCount;
                        var q = from itm in attendaces where itm.EMPLOYEEID == d.EMPID select itm;
                        var singleOrDefault = q.FirstOrDefault();
                        if (singleOrDefault != null) sundayMiss = sundayCount - Convert.ToDecimal(singleOrDefault.HK);

                        decimal dayOut = d.MANGKIR + d.HKP1 + sundayMiss;
                        riceEmployee = dayOut * standRiceEmployee / 30;
                        riceWife = dayOut * standRiceWife / 30;
                        riceChild = dayOut * standRiceChild / 30;

                        natura = (standRiceEmployee - riceEmployee)
                                   + (fs.SPOUSE * (standRiceWife - riceWife))
                                   + (fs.CHILDREN * (standRiceChild - riceChild));
                        naturaIncomeEmployee = (standRiceEmployee - riceEmployee) * ricePrice;
                        naturaIncome = natura * ricePrice;
                    }
                    else if (d.EMPTYPE.ToUpper().StartsWith("BHL"))
                    {
                        var pos = _context.MPOSITION.Where(x => x.POSITIONID.Equals(d.POSITIONID)).FirstOrDefault();
                        if (pos.POSFLAG == 6 || d.JOINTDATE < new DateTime(2016, 1, 1))
                        {
                            natura = (standRiceEmployee / 30) * (d.PRESENT);
                            if (natura > standRiceEmployee) natura = standRiceEmployee;

                            naturaIncome = natura * ricePrice;
                            naturaIncomeEmployee = natura * ricePrice;
                        }
                    }

                    d.RICEPAIDASMONEY = scheme.RICEPAIDASMONEY;
                    var naturaDeduction = d.RICEPAIDASMONEY.ToUpper() == "UANG" ? 0 : naturaIncome;

                    if (naturaIncome > 0)
                    {
                        d.NATURAINCOME += naturaIncome;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeNaturaIncome, EMPID = d.EMPID, AMOUNT = naturaIncome });
                    }

                    if (naturaDeduction > 0)
                    {
                        d.NATURADEDUCTION += naturaDeduction;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeNaturaDeduction, EMPID = d.EMPID, AMOUNT = naturaDeduction });
                    }

                    if (naturaIncomeEmployee > 0) d.NATURAINCOMEEMPLOYEE += naturaIncomeEmployee;
                    if (natura > 0) d.NATURA += natura;

                    //Copy value To TPAYMENTDETAIL
                    foreach (var row in payment.TPAYMENTDETAIL.Where(r => r.EMPID.Equals(d.EMPID)))
                    {
                        row.RICEPAIDASMONEY = d.RICEPAIDASMONEY;
                        row.NATURAINCOME = d.NATURAINCOME;
                        row.NATURADEDUCTION = d.NATURADEDUCTION;
                        row.NATURAINCOMEEMPLOYEE = d.NATURAINCOMEEMPLOYEE;
                        row.NATURA = d.NATURA;
                    }
                }
            }
        }

        private void CalculatePremiPanenMandor(TPAYMENT payment, MPAYMENTSCHEME scheme, DateTime startPeriod1, DateTime endPeriod1, List<TMANDORFINE> mandorFines)
        {
            if (payment.PERIOD == 2)
            {
                var mandorBrondolAddMandor1 = HelperService.GetConfigValue(PMSConstants.CfgPayrollPremiMandorBrondolAddMandor1 + payment.UNITCODE, _context) == PMSConstants.CfgPayrollPremiMandorBrondolAddMandor1True;

                var list = new List<THARVESTRESULT1>();
                list.AddRange(payment.THARVESTRESULT1);
                //Ambil Harvest Result PERIOD 1 BHL
                //list.AddRange(PMSServices.THARVESTRESULT1.GetByDateAndEmployeeType(payment.UNITCODE, "BHL", startPeriod1, endPeriod1));

                var q = (from r in list
                         where r.MANDORFLAG == true
                         group r by new { r.HARVESTDATE, r.MANDORID, r.HARVESTTYPE, r.HARVESTPAYMENTTYPE }
                             into grp
                         select new { grp.Key.HARVESTDATE, grp.Key.MANDORID, grp.Key.HARVESTTYPE, grp.Key.HARVESTPAYMENTTYPE, EmployeeCount = grp.Select(x => x.EMPLOYEEID).Distinct().Count() }
                         ).ToList();

                var q2 = (from r in list
                          where r.MANDORFLAG == true
                          group r by new { r.HARVESTDATE, r.MANDORID, r.MANDOR1ID, r.KRANIID, r.HARVESTTYPE, r.HARVESTPAYMENTTYPE, r.EMPLOYEETYPE, r.ACTIVITYID }
                              into grp
                          select new
                          {
                              grp.Key.HARVESTDATE,
                              grp.Key.MANDORID,
                              grp.Key.MANDOR1ID,
                              grp.Key.KRANIID,
                              grp.Key.HARVESTTYPE,
                              grp.Key.HARVESTPAYMENTTYPE,
                              grp.Key.EMPLOYEETYPE,
                              grp.Key.ACTIVITYID,
                              //PremiPegawai = grp.Sum(r => r.NEWINCENTIVE1 + r.NEWINCENTIVE2 + r.NEWINCENTIVE3 - r.FINEAMOUNT + r.HAINCENTIVE + r.ATTINCENTIVE)
                              PremiPegawai = grp.Sum(r => r.NEWINCENTIVE1 + r.NEWINCENTIVE2 + r.NEWINCENTIVE3 + r.HAINCENTIVE + r.ATTINCENTIVE)
                          }).ToList();

                var q4 = from m in q2
                         join n in q
                             on new { m.HARVESTDATE, m.MANDORID, m.HARVESTTYPE, m.HARVESTPAYMENTTYPE }
                             equals new { n.HARVESTDATE, n.MANDORID, n.HARVESTTYPE, n.HARVESTPAYMENTTYPE }
                         select new { m.HARVESTDATE, m.MANDORID, m.MANDOR1ID, m.KRANIID, m.HARVESTTYPE, m.HARVESTPAYMENTTYPE, m.EMPLOYEETYPE, n.EmployeeCount, m.PremiPegawai, m.ACTIVITYID };

                var qc = (from r in list
                          where !string.IsNullOrEmpty(r.CHECKERID) && r.CHECKERFLAG == true
                          group r by new { r.UNITCODE, r.HARVESTDATE, r.CHECKERID, r.ACTIVITYID, r.HARVESTTYPE, r.HARVESTPAYMENTTYPE }
                              into grp
                          select new
                          {
                              grp.Key.UNITCODE,
                              grp.Key.HARVESTDATE,
                              grp.Key.CHECKERID,
                              grp.Key.ACTIVITYID,
                              grp.Key.HARVESTTYPE,
                              grp.Key.HARVESTPAYMENTTYPE,
                              EmployeeCount = grp.Select(x => x.EMPLOYEEID).Distinct().Count(),
                              //PremiPegawai = grp.Sum(r => r.NEWINCENTIVE1 + r.NEWINCENTIVE2 + r.NEWINCENTIVE3 - r.FINEAMOUNT + r.HAINCENTIVE + r.ATTINCENTIVE)
                              PremiPegawai = grp.Sum(r => r.NEWINCENTIVE1 + r.NEWINCENTIVE2 + r.NEWINCENTIVE3 + r.HAINCENTIVE + r.ATTINCENTIVE)
                          }).ToList();

                string mandorBrondolVersion =
                    HelperService.GetConfigValue(PMSConstants.CFG_PayrollPremiMandorBrondolVersion + payment.UNITCODE, _context);

                if (scheme.PREMISYSTEM == PMSConstants.PremiSystemHarian)
                {
                    foreach (var itm in q4)
                    {
                        decimal premiMandorPct = 0;
                        decimal premiKraniPct = 0;
                        if (itm.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah &&
                            itm.EMPLOYEETYPE.ToUpper().StartsWith("SKU"))
                        {
                            premiMandorPct = scheme.MANDORPREMIPCT;
                            premiKraniPct = scheme.KRANIPREMIPCT;
                        }
                        else if (itm.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah &&
                                 itm.EMPLOYEETYPE.ToUpper().StartsWith("BHL"))
                        {
                            premiMandorPct = scheme.MANDORPREMIPCTBHL;
                            premiKraniPct = scheme.KRANIPREMIPCTBHL;
                        }
                        else if (itm.HARVESTTYPE == PMSConstants.HarvestTypeKutipBrondol &&
                                 itm.EMPLOYEETYPE.ToUpper().StartsWith("SKU"))
                        {
                            premiMandorPct = scheme.MANDORBRONDOLPREMIPCT;
                            premiKraniPct = scheme.KRANIBRONDOLSKU;
                        }
                        else if (itm.HARVESTTYPE == PMSConstants.HarvestTypeKutipBrondol &&
                                 itm.EMPLOYEETYPE.ToUpper().StartsWith("BHL"))
                        {
                            premiMandorPct = scheme.MANDORBRONDOLPREMIPCTBHL;
                            premiKraniPct = scheme.KRANIBRONDOLBHL;
                        }

                        string divId = string.Empty;
                        var m = from i in payment.TPAYMENTDETAIL where i.EMPID == itm.MANDORID select i;
                        if (m.Count() > 0) divId = m.FirstOrDefault().COSTCENTER;

                        if (divId == string.Empty)
                        {
                            var k = from i in payment.TPAYMENTDETAIL where i.EMPID == itm.KRANIID select i;
                            if (k.Count() > 0) divId = k.FirstOrDefault().COSTCENTER;
                        }

                        if (divId == string.Empty)
                        {
                            var m1 = from i in payment.TPAYMENTDETAIL where i.EMPID == itm.MANDOR1ID select i;
                            if (m1.Count() > 0) divId = m1.FirstOrDefault().COSTCENTER;
                        }

                        if (!string.IsNullOrEmpty(divId))
                        {
                            var premiMandor = new TPREMIMANDOR
                            {
                                PAYMENTNO = payment.DOCNO,
                                UNITCODE = payment.UNITCODE,
                                DIVID = divId,
                                HARVESTDATE = itm.HARVESTDATE,
                                MANDORID = itm.MANDORID,
                                MANDOR1ID = itm.MANDOR1ID,
                                KRANIID = itm.KRANIID,
                                HARVESTTYPE = itm.HARVESTTYPE,
                                HARVESTPAYMENTTYPE = itm.HARVESTPAYMENTTYPE,
                                EMPTYPE = itm.EMPLOYEETYPE,
                                TOTALEMP = Convert.ToInt16(itm.EmployeeCount),
                                PREMIMANDORPCT = premiMandorPct,
                                PREMIKRANIPCT = premiKraniPct,
                                PREMIEMP = itm.PremiPegawai,
                                ACTIVITYID = itm.ACTIVITYID,
                                EMPCOUNT =
                                    Convert.ToInt16(
                                    itm.EmployeeCount <= scheme.MINEMPLOYEE
                                        ? scheme.MINEMPLOYEE
                                        : itm.EmployeeCount)
                            };

                            if (itm.HARVESTTYPE == PMSConstants.HarvestTypeKutipBrondol && mandorBrondolVersion == "2")
                                premiMandor.PREMIMANDOR = (itm.PremiPegawai - (itm.EmployeeCount * scheme.BHLDAILYWAGES)) *
                                                    (premiMandorPct / 100) / premiMandor.EMPCOUNT;
                            else
                                premiMandor.PREMIMANDOR = itm.PremiPegawai / premiMandor.EMPCOUNT * premiMandorPct / 100;

                            if (premiMandor.PREMIMANDOR < 0) premiMandor.PREMIMANDOR = 0;

                            premiMandor.PREMIKRANI = itm.PremiPegawai / premiMandor.EMPCOUNT * premiKraniPct / 100;

                            if (premiMandor.PREMIEMP <= 0)
                            {
                                premiMandor.PREMIMANDOR = 0;
                                premiMandor.PREMIKRANI = 0;
                            }

                            if (premiMandor.PREMIMANDOR > 0)
                            {
                                var f = from i in mandorFines where i.EMPID == premiMandor.MANDORID && i.DATE.Date == premiMandor.HARVESTDATE.Date select i;
                                if (f.Count() > 0) premiMandor.MANDORFINE = premiMandor.PREMIMANDOR;

                                var exist = _serviceAttendance.CheckAttendance(payment.UNITCODE, premiMandor.MANDORID, premiMandor.HARVESTDATE.Date, "K", string.Empty);
                                if (exist == 0) premiMandor.MANDORFINE = premiMandor.PREMIMANDOR;
                            }

                            if (premiMandor.PREMIKRANI > 0)
                            {
                                var f = from i in mandorFines where i.EMPID == premiMandor.KRANIID && i.DATE.Date == premiMandor.HARVESTDATE.Date select i;
                                if (f.Count() > 0) premiMandor.KRANIFINE = premiMandor.PREMIKRANI;

                                var exist = _serviceAttendance.CheckAttendance(payment.UNITCODE, premiMandor.KRANIID, premiMandor.HARVESTDATE.Date, "K", string.Empty);
                                if (exist == 0) premiMandor.KRANIFINE = premiMandor.PREMIKRANI;
                            }

                            payment.TPREMIMANDOR.Add(premiMandor);
                        }
                    }

                    if (scheme.CHECKERPREMIPCT > 0)
                        foreach (var itm in qc)
                        {
                            if (itm.PremiPegawai > 0)
                            {
                                var premiChecker = new TPREMICHECKER
                                {
                                    UNITID = itm.UNITCODE,
                                    PAYMENTNO = payment.DOCNO,
                                    DATE = itm.HARVESTDATE,
                                    CHECKERID = itm.CHECKERID,
                                    ACTID = itm.ACTIVITYID,
                                    HARVESTTYPE = itm.HARVESTTYPE,
                                    HARVESTPAYMENTTYPE = itm.HARVESTPAYMENTTYPE,
                                    EMPPREMI = itm.PremiPegawai,
                                    //EmployeeCount = itm.EmployeeCount,
                                    PREMIPCT = scheme.CHECKERPREMIPCT,
                                    //Premi = (itm.PremiPegawai / itm.EmployeeCount) * (scheme.CheckerPremiPct / 100)
                                };
                                premiChecker.EMPCOUNT = Convert.ToByte(itm.EmployeeCount <= scheme.MINEMPLOYEE ? scheme.MINEMPLOYEE : itm.EmployeeCount);
                                premiChecker.PREMI = (itm.PremiPegawai / premiChecker.EMPCOUNT) * (scheme.CHECKERPREMIPCT / 100);

                                if (premiChecker.PREMI > 0)
                                {
                                    var f = from i in mandorFines where i.EMPID == premiChecker.CHECKERID && i.DATE.Date == premiChecker.DATE.Date select i;
                                    if (f.Count() > 0) premiChecker.FINE = premiChecker.PREMI;

                                    var exist = _serviceAttendance.CheckAttendance(payment.UNITCODE, premiChecker.CHECKERID, premiChecker.DATE.Date, "K", string.Empty);
                                    if (exist == 0) premiChecker.FINE = premiChecker.PREMI;
                                }

                                if (premiChecker.PREMI > 0) payment.TPREMICHECKER.Add(premiChecker);
                            }
                        }

                    var pm = (from p in payment.TPREMIMANDOR
                              where p.PREMIMANDORPCT > 0 && (p.HARVESTTYPE == 0 || mandorBrondolAddMandor1)
                              group p by new { p.UNITCODE, p.HARVESTDATE, p.MANDOR1ID, p.HARVESTPAYMENTTYPE }
                                  into grp
                              select new
                              {
                                  grp.Key.UNITCODE,
                                  grp.Key.HARVESTDATE,
                                  grp.Key.MANDOR1ID,
                                  grp.Key.HARVESTPAYMENTTYPE,
                                  Premi = grp.Sum(p => p.PREMIMANDOR),
                                  EmployeeCount = grp.Select(p => p.MANDORID).Distinct().Count()
                              }).ToList();

                    foreach (var p in pm)
                    {
                        var premimdr1 = new TPREMIMANDOR1
                        {
                            UNITID = p.UNITCODE,
                            PAYMENTNO = payment.DOCNO,
                            DATE = p.HARVESTDATE,
                            MANDORID = p.MANDOR1ID,
                            HARVESTPAYMENTTYPE = p.HARVESTPAYMENTTYPE,
                            EMPPREMI = p.Premi,
                            EMPCOUNT = Convert.ToInt16(p.EmployeeCount),
                            PREMIPCT = scheme.MANDOR1PREMIPCT,
                            PREMI = (p.Premi / p.EmployeeCount) * (scheme.MANDOR1PREMIPCT / 100),
                        };

                        if (premimdr1.PREMI > 0)
                        {
                            var f = from i in mandorFines where i.EMPID == premimdr1.MANDORID && i.DATE.Date == premimdr1.DATE.Date select i;
                            if (f.Count() > 0) premimdr1.FINE = premimdr1.PREMI;

                            var exist = _serviceAttendance.CheckAttendance(payment.UNITCODE, premimdr1.MANDORID, premimdr1.DATE.Date, "K", string.Empty);
                            if (exist == 0) premimdr1.FINE = premimdr1.PREMI;
                        }

                        if (premimdr1.PREMI > 0) payment.TPREMIMANDOR1.Add(premimdr1);
                    }
                }
            }
        }

        private static void CalculatePremi(TPAYMENT payment, List<MSALARYTYPE> salaryTypes, List<TSALARYTYPEMAP> typeMaps)
        {
            var q1 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiPanen select itm;
            var q2 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiPokokTinggi select itm;
            //var q3 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiHadirPanen select itm;
            var q4 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiHektarePanen select itm;
            var q5 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiKraniPanen select itm;
            var q6 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiMandorPanen select itm;
            var q7 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremimandor1Panen select itm;
            var q8 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiChecker select itm;
            var q9 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiMuat select itm;
            var q10 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiPanenKontanan select itm;
            var q11 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremimandor1PanenKontanan select itm;
            var q12 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiMandorPanenKontanan select itm;
            var q13 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiKraniPanenKontanan select itm;
            var q14 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiCheckerKontanan select itm;
            var q15 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiHektarePanenKontanan select itm;
            var q16 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiPokokTinggiKontanan select itm;
            var q17 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodeInsentifMandorPanen select itm;
            var q18 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiPanenTerbaik select itm;
            var q19 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiPanenGerdan select itm;
            var q20 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiOperatorAngkutTBS select itm;
            var q21 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiMuatKontanan select itm;
            var q22 = from itm in salaryTypes where itm.SYSCODE == PMSConstants.SalaryTypeCodePremiOperatorAngkutTBSKontanan select itm;


            foreach (var d in payment.TPAYMENTDETAIL)
            {
                if (q1.Count() > 0)
                {
                    MSALARYTYPE type = q1.FirstOrDefault();
                    var q = from itm in payment.THARVESTRESULT1
                            where itm.EMPLOYEEID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                            && itm.EFLAG == true
                            select itm.NEWINCENTIVE1 + itm.NEWINCENTIVE2 + itm.NEWINCENTIVE3 - itm.FINEAMOUNT;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q19.Count() > 0)
                {
                    MSALARYTYPE type = q19.FirstOrDefault();
                    var q = from itm in payment.TGERDANRESULT
                            where itm.GEMPID == d.EMPID
                            select itm.NEWINCENTIVE1 + itm.NEWINCENTIVE2 + itm.NEWINCENTIVE3;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q2.Count() > 0)
                {
                    MSALARYTYPE type = q2.FirstOrDefault();
                    var q = from itm in payment.THARVESTRESULT1
                            where itm.EMPLOYEEID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                            && itm.EFLAG == true
                            select itm.INCENTIVEPKKTGI;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                //if (q3.Count() > 0)
                //{
                //    MSALARYTYPE type = q3.FirstOrDefault();
                //    var q = from itm in payment.THARVESTRESULT1
                //            where itm.EMPLOYEEID == d.EMPLOYEEID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                //            && itm.EFLAG == true
                //            select itm.ATTINCENTIVE;
                //    var amount = q.Sum();
                //    if (amount > 0)
                //    {
                //        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.Id, EMPLOYEEID = d.EMPLOYEEID, Date = payment.DOCDATE, AMOUNT = amount, Auto = true });
                //        payment.TSALARYITEM.Add(new TPAYMENTITEM { TYPEID = type.Id, EMPLOYEEID = d.EMPLOYEEID, AMOUNT = amount, });
                //    }
                //}

                if (q4.Count() > 0)
                {
                    MSALARYTYPE type = q4.FirstOrDefault();
                    var q = from itm in payment.THARVESTRESULT1
                            where itm.EMPLOYEEID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                            && itm.EFLAG == true
                            select itm.HAINCENTIVE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q5.Count() > 0)
                {
                    MSALARYTYPE type = q5.FirstOrDefault();
                    var q = from itm in payment.TPREMIMANDOR
                            where itm.KRANIID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                            select itm.PREMIKRANI - itm.KRANIFINE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q6.Count() > 0)
                {
                    MSALARYTYPE type = q6.FirstOrDefault();
                    var q = from itm in payment.TPREMIMANDOR
                            where itm.MANDORID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                            select itm.PREMIMANDOR - itm.MANDORFINE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q7.Count() > 0)
                {
                    MSALARYTYPE type = q7.FirstOrDefault();
                    var q = from itm in payment.TPREMIMANDOR1
                            where itm.MANDORID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                            select itm.PREMI - itm.FINE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q8.Count() > 0)
                {
                    MSALARYTYPE type = q8.FirstOrDefault();
                    var q = from itm in payment.TPREMICHECKER
                            where itm.CHECKERID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeHarian
                            select itm.PREMI - itm.FINE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q9.Count() > 0)
                {
                    MSALARYTYPE type = q9.FirstOrDefault();
                    var q = from itm in payment.TLOADINGRESULT
                            where itm.EMPLOYEEID == d.EMPID
                            select itm.NEWINCENTIVE1 + itm.NEWINCENTIVE2 + itm.NEWINCENTIVE3;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q10.Count() > 0)
                {
                    MSALARYTYPE type = q10.FirstOrDefault();
                    MSALARYTYPE typePanen = q1.FirstOrDefault();
                    var q = from itm in payment.THARVESTRESULT1
                            where itm.EMPLOYEEID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeKontanan
                            && itm.EFLAG == true
                            select itm.NEWINCENTIVE1 + itm.NEWINCENTIVE2 + itm.NEWINCENTIVE3;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = typePanen.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q11.Count() > 0)
                {
                    MSALARYTYPE type = q11.FirstOrDefault();
                    MSALARYTYPE typePanen = q7.FirstOrDefault();
                    var q = from itm in payment.TPREMIMANDOR1
                            where itm.MANDORID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeKontanan
                            select itm.PREMI - itm.FINE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = typePanen.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q12.Count() > 0)
                {
                    MSALARYTYPE type = q12.FirstOrDefault();
                    MSALARYTYPE typePanen = q6.FirstOrDefault();
                    var q = from itm in payment.TPREMIMANDOR
                            where itm.MANDORID == d.EMPID && itm.HARVESTPAYMENTTYPE == (int)PMSConstants.PayTypeKontanan
                            select itm.PREMIMANDOR - itm.MANDORFINE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = typePanen.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q13.Count() > 0)
                {
                    MSALARYTYPE type = q13.FirstOrDefault();
                    MSALARYTYPE typePanen = q5.FirstOrDefault();
                    var q = from itm in payment.TPREMIMANDOR
                            where itm.KRANIID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeKontanan
                            select itm.PREMIKRANI - itm.KRANIFINE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = typePanen.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q14.Count() > 0)
                {
                    MSALARYTYPE type = q14.FirstOrDefault();
                    MSALARYTYPE typePanen = q8.FirstOrDefault();
                    var q = from itm in payment.TPREMICHECKER
                            where itm.CHECKERID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeKontanan
                            select itm.PREMI - itm.FINE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = typePanen.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q15.Count() > 0)
                {
                    MSALARYTYPE type = q15.FirstOrDefault();
                    MSALARYTYPE typePanen = q4.FirstOrDefault();
                    var q = from itm in payment.THARVESTRESULT1
                            where itm.EMPLOYEEID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeKontanan
                            && itm.EFLAG == true
                            select itm.HAINCENTIVE;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = typePanen.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q16.Count() > 0)
                {
                    MSALARYTYPE type = q16.FirstOrDefault();
                    MSALARYTYPE typePanen = q2.FirstOrDefault();
                    var q = from itm in payment.THARVESTRESULT1
                            where itm.EMPLOYEEID == d.EMPID && itm.HARVESTPAYMENTTYPE == PMSConstants.PayTypeKontanan
                            && itm.EFLAG == true
                            select itm.INCENTIVEPKKTGI;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = typePanen.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                //if (q17.Count() > 0)
                //{
                //    MSALARYTYPE type = q17.FirstOrDefault();
                //    var q = from itm in payment.TIN IncentiveMandor
                //            where itm.EMPLOYEEID == d.EMPLOYEEID
                //            select itm.AMOUNT;
                //    var amount = q.Sum();
                //    if (amount > 0)
                //    {
                //        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.Id, EMPLOYEEID = d.EMPLOYEEID, Date = payment.DOCDATE, AMOUNT = amount, Auto = true });
                //        payment.TSALARYITEM.Add(new TPAYMENTITEM { TYPEID = type.Id, EMPLOYEEID = d.EMPLOYEEID, AMOUNT = amount, });
                //    }
                //}

                if (q20.Count() > 0)
                {
                    MSALARYTYPE type = q20.FirstOrDefault();
                    var q = from itm in payment.TOPERATINGRESULT
                            where itm.DRIVERID == d.EMPID
                            select itm.NEWINCENTIVE1;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q21.Count() > 0)
                {
                    MSALARYTYPE type = q21.FirstOrDefault();
                    var q = from itm in payment.TLOADINGRESULT
                            where itm.EMPLOYEEID == d.EMPID && itm.LOADINGPAYMENTTYPE == PMSConstants.PayTypeKontanan
                            select itm.NEWINCENTIVE1;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

                if (q22.Count() > 0)
                {
                    MSALARYTYPE type = q22.FirstOrDefault();
                    var q = from itm in payment.TOPERATINGRESULT
                            where itm.DRIVERID == d.EMPID && itm.LOADINGPAYMENTTYPE == PMSConstants.PayTypeKontanan
                            select itm.NEWINCENTIVE1;
                    var amount = q.Sum();
                    if (amount > 0)
                    {
                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = d.EMPID, DATE = payment.DOCDATE.Date, AMOUNT = amount, AUTO = true });
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = d.EMPID, AMOUNT = amount });
                    }
                }

            }

            if (payment.PERIOD == 2)
            {
                foreach (var type in typeMaps)
                {
                    string empid = string.Empty;
                    if (!string.IsNullOrEmpty(type.EMPID))
                        empid = type.EMPID;

                    foreach (var d in payment.TPAYMENTDETAIL)
                    {
                        if (type.FREQ == PMSConstants.SalaryTypeFreqMonthly)
                        {
                            if (!string.IsNullOrEmpty(type.POSID) && type.AMOUNT > 0)
                                if (d.POSITIONID.Trim() == type.POSID.Trim())
                                {
                                    payment.TSALARYITEM.Add(new TSALARYITEM
                                    {
                                        UNITID = payment.UNITCODE,
                                        TYPEID = type.TYPEID,
                                        EMPID = d.EMPID,
                                        DATE = payment.DOCDATE,
                                        AMOUNT = type.AMOUNT,
                                        MAPREF = type.ID,
                                        AUTO = true
                                    });
                                    payment.TPAYMENTITEM.Add(new TPAYMENTITEM
                                    {
                                        TYPEID = type.TYPEID,
                                        EMPID = d.EMPID,
                                        AMOUNT = type.AMOUNT,
                                    });
                                }

                            if (!string.IsNullOrEmpty(type.EMPID) && type.AMOUNT > 0)
                                if (d.EMPID.Trim() == type.EMPID.Trim())
                                {
                                    payment.TSALARYITEM.Add(new TSALARYITEM
                                    {
                                        UNITID = payment.UNITCODE,
                                        TYPEID = type.TYPEID,
                                        EMPID = d.EMPID,
                                        DATE = payment.DOCDATE,
                                        AMOUNT = type.AMOUNT,
                                        MAPREF = type.ID,
                                        AUTO = true
                                    });
                                    payment.TPAYMENTITEM.Add(new TPAYMENTITEM
                                    {
                                        TYPEID = type.TYPEID,
                                        EMPID = d.EMPID,
                                        AMOUNT = type.AMOUNT,
                                    });
                                }
                        }
                        else if (type.FREQ == PMSConstants.SalaryTypeFreqDaily)
                        {
                            if (d.EMPID.Trim() == empid || d.POSITIONID == type.POSID)
                                if (d.PRESENT > 0)
                                {
                                    payment.TSALARYITEM.Add(new TSALARYITEM
                                    {
                                        UNITID = payment.UNITCODE,
                                        TYPEID = type.TYPEID,
                                        EMPID = d.EMPID,
                                        DATE = payment.DOCDATE,
                                        AMOUNT = type.AMOUNT * d.PRESENT,
                                        MAPREF = type.ID,
                                        AUTO = true
                                    });
                                    payment.TPAYMENTITEM.Add(new TPAYMENTITEM
                                    {
                                        TYPEID = type.TYPEID,
                                        EMPID = d.EMPID,
                                        AMOUNT = type.AMOUNT * d.PRESENT,
                                    });
                                }
                        }
                        else if (type.FREQ == PMSConstants.SalaryTypeFreqMin25ProHke)
                        {
                            if (d.EMPID.Trim() == empid || d.POSITIONID == type.POSID)
                            {
                                var hke = d.DAYS - d.HOLIDAY - d.SUNDAY;
                                decimal amount = 0;
                                if (d.PRESENT >= 25 || d.PRESENT >= hke) amount = type.AMOUNT;
                                else amount = Math.Floor((Convert.ToDecimal(d.PRESENT) / Convert.ToDecimal(hke)) * type.AMOUNT);

                                if (amount > 0)
                                {
                                    payment.TSALARYITEM.Add(new TSALARYITEM
                                    {
                                        UNITID = payment.UNITCODE,
                                        TYPEID = type.TYPEID,
                                        EMPID = d.EMPID,
                                        DATE = payment.DOCDATE,
                                        AMOUNT = amount,
                                        MAPREF = type.ID,
                                        AUTO = true
                                    });
                                    payment.TPAYMENTITEM.Add(new TPAYMENTITEM
                                    {
                                        TYPEID = type.TYPEID,
                                        EMPID = d.EMPID,
                                        AMOUNT = amount,
                                    });
                                }
                            }
                        }
                    }
                }

                //-----------------------------------------------------------------------------------------------------
                //Premi Pemanen Terbaik
                //-----------------------------------------------------------------------------------------------------

                var qEmpResult = (from m in payment.THARVESTRESULT1
                                  join n in payment.TPAYMENTDETAIL
                                  on m.EMPLOYEEID equals n.EMPID
                                  select new { DivisionId = n.COSTCENTER, m.EMPLOYEEID, m.HARVESTTYPE, m.QTYKG }
                                 ).ToList();

                var qSumEmp = (from r in qEmpResult
                               where r.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah
                               group r by new { r.DivisionId, r.EMPLOYEEID, }
                             into grp
                               select new { grp.Key.DivisionId, grp.Key.EMPLOYEEID, Kg = grp.Select(x => x.QTYKG).Sum() }
                         ).ToList();

                var qTopKg = (from r in qSumEmp
                              group r by new { r.DivisionId, }
                             into grp
                              select new { grp.Key.DivisionId, Kg = grp.Select(x => x.Kg).Max() }
                         ).ToList();

                foreach (var item in qTopKg)
                {
                    if (item.Kg > 0)
                    {
                        var qHarvester = from i in qSumEmp
                                         where i.DivisionId == item.DivisionId && i.Kg == item.Kg
                                         select i;

                        if (qHarvester.Count() > 0)
                        {
                            var hvtResult = qHarvester.FirstOrDefault();
                            var qDetail = from i in payment.TPAYMENTDETAIL
                                          where i.EMPID == hvtResult.EMPLOYEEID
                                          select i;

                            if (qDetail.Count() > 0)
                            {
                                var det = qDetail.FirstOrDefault();
                                if (det.PRESENT >= det.DAYS - det.HOLIDAY - det.SUNDAY)
                                {
                                    MSALARYTYPE type = q18.FirstOrDefault();
                                    {
                                        payment.TSALARYITEM.Add(new TSALARYITEM { UNITID = payment.UNITCODE, TYPEID = type.ID, EMPID = det.EMPID, DATE = payment.DOCDATE, AMOUNT = 600000, AUTO = true });
                                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = type.ID, EMPID = det.EMPID, AMOUNT = 600000, });
                                    }
                                }
                            }
                        }
                    }
                }
                //-----------------------------------------------------------------------------------------------------
            }

            //var prm = (from m in payment.TSALARYITEM
            //           join l in salaryTypes
            //               on m.TYPEID equals l.Id
            //           select new { l.PAYCODE, m.EMPLOYEEID, m.AMOUNT }).ToList();

            //if (payment.PERIOD == 2)
            //{
            //    var manPremi = PMSServices.PaymentDetail.GetManualPremi(payment.UNITCODE, period.From2, period.To2);
            //    foreach (DataRow r in manPremi.Rows)
            //    {
            //        var q = from itm in payment.Details where itm.EMPLOYEEID == r["EMPID"].ToString() select itm;
            //        PaymentDetail detail = q.FirstOrDefault();

            //        if (q.Count() == 0)
            //            throw new Exception("Karyawan dengan kode " + r["EMPID"] + " tidak terdaftar/tidak aktif.");

            //        if (Convert.ToDecimal(r["PRMPNN"]) > 0) detail.PREMIPANEN += Convert.ToDecimal(r["PRMPNN"]);
            //        if (Convert.ToDecimal(r["PRMNPN"]) > 0) detail.PREMINONPANEN += Convert.ToDecimal(r["PRMNPN"]);
            //    }
            //}

            //foreach (var d in payment.Details)
            //{
            //    var amPnn = from i in prm
            //                where i.EMPLOYEEID == d.EMPLOYEEID && i.PAYCODE == PMSConstants.SalaryTypePaymentCodePremiPanen
            //                select i.AMOUNT;
            //    var amountPanen = amPnn.Sum();
            //    if (amountPanen > 0) d.PREMIPANEN += amountPanen;

            //    var amNpn = from i in prm
            //                where i.EMPLOYEEID == d.EMPLOYEEID && i.PAYCODE == PMSConstants.SalaryTypePaymentCodePremiNonPanen
            //                select i.AMOUNT;
            //    var amountNonPanen = amNpn.Sum();
            //    if (amountNonPanen > 0) d.PREMINONPANEN += amountNonPanen;
            //}
        }

        private void SumSalaryItem(TPAYMENT payment, MPERIOD period, List<MSALARYTYPE> salaryTypes)
        {
            //var prm = (from m in payment.TSALARYITEM
            //           join l in salaryTypes
            //               on m.TYPEID equals l.Id
            //           select new { l.PAYCODE, l.SYSCODE, m.EMPLOYEEID, m.AMOUNT }).ToList();

            if (payment.PERIOD == 2)
            {
                var manPremi = _context.sp_PaymentDetail_GetManualPremi_Result(payment.UNITCODE, period.FROM2, period.TO2);
                foreach (var r in manPremi)
                {
                    var q = from itm in payment.TPAYMENTDETAIL where itm.EMPID == r.EMPID select itm;
                    TPAYMENTDETAIL detail = q.FirstOrDefault();

                    if (q.Count() == 0)
                        throw new Exception("Karyawan dengan kode " + r.EMPID + " tidak terdaftar/tidak aktif.");

                    if (Convert.ToDecimal(r.PRMPNN) > 0)
                    {
                        var amount = Convert.ToDecimal(r.PRMPNN);
                        //detail.PREMIPANEN += amount;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = r.TYPEID, EMPID = detail.EMPID, AMOUNT = amount });
                    }

                    if (Convert.ToDecimal(r.PRMNPN) > 0)
                    {
                        var amount = Convert.ToDecimal(r.PRMNPN);
                        //detail.PREMINONPANEN += amount;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = r.TYPEID, EMPID = detail.EMPID, AMOUNT = amount });
                    }

                    if (Convert.ToDecimal(r.INC) > 0)
                    {
                        var amount = Convert.ToDecimal(r.INC);
                        //detail.PREMINONPANEN += amount;
                        payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = r.TYPEID, EMPID = detail.EMPID, AMOUNT = amount });
                    }
                }
            }

            var prm = (from m in payment.TSALARYITEM
                       join l in salaryTypes
                           on m.TYPEID equals l.ID
                       select new { l.PAYCODE, l.SYSCODE, m.EMPID, m.AMOUNT }).ToList();

            foreach (var d in payment.TPAYMENTDETAIL)
            {
                var amPnn = from i in prm
                            where i.EMPID == d.EMPID && i.PAYCODE == PMSConstants.SalaryTypePaymentCodePremiPanen
                            select i.AMOUNT;
                var amountPanen = amPnn.Sum();
                if (amountPanen > 0) d.PREMIPANEN += amountPanen;

                var amPnnKtn = from i in prm
                               where i.EMPID == d.EMPID && i.PAYCODE == PMSConstants.SalaryTypePaymentCodePremiPanenKontanan
                               select i.AMOUNT;
                var amountPanenKontan = amPnnKtn.Sum();
                if (amountPanenKontan > 0)
                {
                    //d.PREMIPANEN += amountPanenKontan;
                    d.PREMIPANENKONTAN += amountPanenKontan;
                }

                var amNpn = from i in prm
                            where i.EMPID == d.EMPID && i.PAYCODE == PMSConstants.SalaryTypePaymentCodePremiNonPanen
                            select i.AMOUNT;
                var amountNonPanen = amNpn.Sum();
                if (amountNonPanen > 0) d.PREMINONPANEN += amountNonPanen;

                var amThr = from i in prm
                            where i.EMPID == d.EMPID && i.PAYCODE == PMSConstants.SalaryTypePaymentCodeIncentive
                            select i.AMOUNT;
                var amountThr = amThr.Sum();
                if (amountThr > 0) d.INCENTIVE += amountThr;

                //var amJamsosInc = from i in prm
                //                  where i.EMPLOYEEID == d.EMPLOYEEID && i.PAYCODE == PMSConstants.SalaryTypePaymentCodeJAMSOSTEKINC
                //                  select i.AMOUNT;
                //var amountJamsosInc = amJamsosInc.Sum();
                //if (amountJamsosInc > 0) d.JAMSOSTEKINC += amountJamsosInc;

                //var amJamsosDeduc = from i in prm
                //                    where i.EMPLOYEEID == d.EMPLOYEEID && i.PAYCODE == PMSConstants.SalaryTypePaymentCodeJAMSOSTEKDEDUCT
                //                    select i.AMOUNT;
                //var amountJamsosDeduc = amJamsosDeduc.Sum();
                //if (amountJamsosDeduc > 0) d.JAMSOSTEKDEDUCT += amountJamsosDeduc;
            }
        }

        private void CalculateTaxYtd(TPAYMENT payment, MJAMSOSTEK jamsostek)
        {
            if (payment.PERIOD == 2 && jamsostek != null)
            {
                var taxPaid = HelperService.GetConfigValue(PMSConstants.CfgPayrollTaxPaid + payment.UNITCODE, _context) == PMSConstants.CfgPayrollTaxPaidTrue;

                List<TPAYMENTDETAIL> ytdSalary = _context.sp_PaymentDetail_GetTaxYTD_TPAYMENTDETAILResult(payment.UNITCODE, payment.DOCDATE.Date).ToList();

                foreach (var d in payment.VTPAYMENTDETAIL)
                {
                    if (d.EMPTYPE.ToUpper().StartsWith("SKU")) 
                    {                       
                        var paymentTax = new TPAYMENTTAX { DOCNO = payment.DOCNO, EMPID = d.EMPID };

                        decimal month = payment.DOCDATE.Month;
                        decimal jointMonth = 1;
                        if (d.JOINTDATE != null)
                            if (payment.DOCDATE.Year == ((DateTime)d.JOINTDATE).Year)
                                jointMonth = ((DateTime)d.JOINTDATE).Month;

                        var q = ytdSalary.Where(r => r.EMPID.Equals(d.EMPID)).ToList();
                        TPAYMENTDETAIL ytd = q.Count() == 0 ? new TPAYMENTDETAIL() : q.FirstOrDefault();

                        decimal astekPerusahaan;
                        if (d.BPJSKES)
                            astekPerusahaan = (jamsostek.JKK + jamsostek.JKM + jamsostek.KESCOMP) / 100;
                        else
                            astekPerusahaan = (jamsostek.JKK + jamsostek.JKM) / 100;

                        decimal astekEmployee = (jamsostek.EMPLOYEE + jamsostek.JPEMP) / 100;

                        paymentTax.BASICWAGESBRUTO = d.BASICWAGESBRUTO + ytd.BASICWAGESBRUTO;
                        paymentTax.JAMSOSTEKINC = paymentTax.BASICWAGESBRUTO * astekPerusahaan;
                        paymentTax.JAMSOSTEKDEDUCT = paymentTax.BASICWAGESBRUTO * astekEmployee;

                        paymentTax.BASICWAGES = d.BASICWAGES + ytd.BASICWAGES;
                        paymentTax.OVERTIME = d.OVERTIME + ytd.OVERTIME;
                        paymentTax.NATURA = d.NATURAINCOME + ytd.NATURAINCOME;
                        paymentTax.INCENTIVE = d.INCENTIVE + ytd.INCENTIVE;
                        paymentTax.PREMI = (d.PREMIPANEN + ytd.PREMIPANEN) + (d.PREMINONPANEN + ytd.PREMINONPANEN)
                            + (d.PREMIHADIR + ytd.PREMIHADIR) - (d.PENALTY + ytd.PENALTY);

                        //paymentTax.GROSSINCOME = paymentTax.BASICWAGES + paymentTax.Premi + paymentTax.OVERTIME
                        //    + paymentTax.NATURA + paymentTax.JAMSOSTEKINC;
                        paymentTax.GROSSINCOME = paymentTax.BASICWAGES + paymentTax.PREMI + paymentTax.OVERTIME
                            + paymentTax.NATURA + paymentTax.JAMSOSTEKINC + paymentTax.INCENTIVE;

                        paymentTax.POSITIONCOST = jamsostek.MAXPOSBENEFIT * 12;
                        if ((paymentTax.GROSSINCOME * jamsostek.POSBENEFIT / 100) < paymentTax.POSITIONCOST)
                            paymentTax.POSITIONCOST = paymentTax.GROSSINCOME * jamsostek.POSBENEFIT / 100;

                        paymentTax.NETINCOMEMONTH = paymentTax.GROSSINCOME - paymentTax.POSITIONCOST - paymentTax.JAMSOSTEKDEDUCT;
                        if (month - jointMonth + 1 > 0)
                            paymentTax.NETINCOMEYEAR = paymentTax.NETINCOMEMONTH / (month - jointMonth + 1) * (13 - jointMonth);

                        decimal ptkp = 0;
                        if (d.TAXSTATUS.ToUpper() == "TK" || d.TAXSTATUS.ToUpper() == "TK1" || d.TAXSTATUS.ToUpper() == "TK2"
                            || d.TAXSTATUS.ToUpper() == "TK3" || d.TAXSTATUS.ToUpper() == "WTK")
                            ptkp = jamsostek.PTKPS;
                        else if (d.TAXSTATUS.ToUpper() == "K" || d.TAXSTATUS.ToUpper() == "K0"
                            || d.TAXSTATUS.ToUpper() == "WK")
                            ptkp = jamsostek.PTKPM;
                        else if (d.TAXSTATUS.ToUpper() == "K1") ptkp = jamsostek.PTKPM1;
                        else if (d.TAXSTATUS.ToUpper() == "K2") ptkp = jamsostek.PTKPM2;
                        else if (d.TAXSTATUS.ToUpper() == "K3") ptkp = jamsostek.PTKPM3;

                        paymentTax.NONTAXABLE = ptkp * 12;
                        //paymentTax.TaxableIncome = (paymentTax.NetIncomeYear + paymentTax.INCENTIVE - paymentTax.NonTaxableIncome);
                        paymentTax.TAXABLE = (paymentTax.NETINCOMEYEAR - paymentTax.NONTAXABLE);
                        paymentTax.TAXABLE = Math.Floor(paymentTax.TAXABLE / 1000) * 1000;

                        decimal newTax = 0;
                        decimal[] limit = { 0, 50000000, 250000000, 500000000 };
                        decimal[] tarif = { 0.05M, 0.15M, 0.25M, 0.3M };

                        for (int i = 0; i < limit.Length; i++)
                        {
                            decimal partWages = 0;
                            if (paymentTax.TAXABLE > limit[i])
                                if (i == limit.Length - 1)
                                    partWages = paymentTax.TAXABLE - limit[i];
                                else
                                {
                                    if (paymentTax.TAXABLE > limit[i + 1])
                                        partWages = limit[i + 1] - limit[i];
                                    else
                                        partWages = paymentTax.TAXABLE - limit[i];
                                }

                            if (partWages > 0)
                            {
                                decimal partTax = tarif[i] * partWages;
                                if (d.NONPWP == 1)
                                    partTax = partTax + (partTax * jamsostek.NPWP / 100);
                                newTax += partTax;
                            }
                        }

                        paymentTax.TAXYEAR = newTax;
                        paymentTax.TAXPAID = ytd.TAX;

                        paymentTax.TAXMONTH = (newTax - paymentTax.TAXPAID);
                        paymentTax.TAXMONTH = paymentTax.TAXMONTH > 0 ? Math.Round(paymentTax.TAXMONTH /
                            (13 - month), 0) : Math.Round(paymentTax.TAXMONTH, 0);

                        payment.TPAYMENTTAX.Add(paymentTax);

                        if (taxPaid)
                        {
                            d.TAX = paymentTax.TAXMONTH;
                            payment.TPAYMENTITEM.Add(new TPAYMENTITEM { TYPEID = PMSConstants.SalaryTypeCodeTax, EMPID = d.EMPID, AMOUNT = paymentTax.TAXMONTH });

                            //Copy value To TPAYMENTDETAIL
                            payment.TPAYMENTDETAIL.Where(r => r.EMPID.Equals(d.EMPID)).ToList().ForEach
                                (r => { r.TAX = d.TAX; });
                        }
                    }


                }
            }
        }

        private string GenerateNewNumber(string unitCode)
        {
            int lastNumber = HelperService.GetCurrentDocumentNumber(PMSConstants.PaymentCodePrefix + unitCode, _context);
            lastNumber += 1;
            return PMSConstants.PaymentCodePrefix + "/" + unitCode + "/" + lastNumber.ToString().PadLeft(4, '0');
        }

        private void InsertValidate(TPAYMENT payment)
        {
            this.Validate(payment);

            TPAYMENT paymentNoExist = _context.TPAYMENT.Where(d => d.DOCNO.Equals(payment.DOCNO)).FirstOrDefault();
            if (paymentNoExist != null)
                throw new Exception("Nomor sudah ada.");

            TPAYMENT paymentExist = _context.TPAYMENT.Where(d => d.PERIOD.Equals(payment.PERIOD) && d.DOCDATE.Date.Equals(payment.DOCDATE.Date)
            && d.UNITCODE.Equals(payment.UNITCODE)).FirstOrDefault();
            if (paymentExist != null)
                throw new Exception("Perhitungan sudah pernah digenerate dengan Nomor " + paymentExist.DOCNO);
        }

        private void UpdateValidate(TPAYMENT payment)
        {
            if (payment.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (payment.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            this.Validate(payment);
        }

        private void DeleteValidate(TPAYMENT payment)
        {
            if (payment.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (payment.STATUS == PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");
            _servicePeriod.CheckValidPeriod(payment.UNITCODE, payment.DOCDATE.Date);
        }

        private void Validate(TPAYMENT payment)
        {
            string result = this.FieldsValidation(payment);
            if (!string.IsNullOrEmpty(result))
                throw new Exception(result);

            _servicePeriod.CheckValidPeriod(payment.UNITCODE, payment.DOCDATE.Date);
        }

        private void ApproveValidate(TPAYMENT payment)
        {
            if (payment.STATUS == PMSConstants.TransactionStatusApproved)
                throw new Exception("Data sudah di approve.");
            if (payment.STATUS ==  PMSConstants.TransactionStatusCanceled)
                throw new Exception("Data sudah di cancel.");

            this.Validate(payment);

            var period = _context.MPERIOD.Where(d => d.UNITCODE.Equals(payment.UNITCODE)
                && d.YEAR == Convert.ToInt16(payment.DOCDATE.Year) && d.MONTH == Convert.ToInt16(payment.DOCDATE.Month)).FirstOrDefault();

            DateTime startDate;
            DateTime endDate;
            if (payment.PERIOD == 1)
            {
                startDate = period.FROM1;
                endDate = period.TO1;
            }
            else
            {
                startDate = period.FROM2;
                endDate = period.TO2;
            }

            //int upkeep = PMSServices.Consumption.GetProcessedData(payment.UnitId, startDate, endDate, database);
            //if (upkeep > 0)
            //    throw new Exception("Masih ada upkeep yang belum diapprove.");

            //int harvest = PMSServices.Harvesting.GetProcessedData(payment.UnitId, startDate, endDate, database);
            //if (harvest > 0)
            //    throw new Exception("Masih ada panen yang belum diapprove.");

            var dsTotal = _context.sp_Payment_CheckTotal(payment.DOCNO);
            decimal total = 0; decimal dTotal = 0; decimal iTotal = 0;
            foreach (var row in dsTotal)
            {
                total = Convert.ToDecimal(row.TOTAL);
                dTotal = Convert.ToDecimal(row.TOTAL);
                iTotal = Convert.ToDecimal(row.TOTAL);
            }

            if (Math.Abs(total - dTotal) > 1000 || Math.Abs(dTotal - iTotal) > 1000 || Math.Abs(iTotal - total) > 1000)
                throw new Exception("Total Salary.");
        }

        private string FieldsValidation(TPAYMENT payment)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(payment.DOCNO)) result += "Nomor tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(payment.UNITCODE)) result += "Unit tidak boleh kosong." + Environment.NewLine;
            if (payment.DOCDATE == new DateTime()) result += "Tanggal tidak boleh kosong." + Environment.NewLine;
            if (payment.PERIOD == 0) result += "PERIOD tidak boleh kosong." + Environment.NewLine;
            if (payment.STARTDATE == new DateTime()) result += "Tanggal awal tidak boleh kosong." + Environment.NewLine;
            if (payment.ENDDATE == new DateTime()) result += "Tanggal akhir tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(payment.MANAGER)) result += "Manager tidak boleh kosong." + Environment.NewLine;
            if (string.IsNullOrEmpty(payment.KTU)) result += "KTU tidak boleh kosong." + Environment.NewLine;
            if (payment.STATUS == PMSConstants.TransactionStatusNone) result += "Status tidak boleh kosong." + Environment.NewLine;
            return result;
        }

        private short SetPeriod(string unitId, DateTime date)
        {
            short period;
            var currPeriod = _context.MPERIOD.Where(d => d.UNITCODE.Equals(unitId)
            && d.YEAR == Convert.ToInt16(date.Year) && d.MONTH == Convert.ToInt16(date.Month)).FirstOrDefault();

            if (currPeriod != null)
            {
                if (date.Date >= currPeriod.FROM1 && date.Date <= currPeriod.TO1)
                    period = 1;
                else if (date.Date >= currPeriod.FROM2 && date.Date <= currPeriod.TO2)
                    period = 2;
                else
                    period = 0;
            }
            else
                period = 0;

            return period;
        }

        public bool Approve(IFormCollection formDataCollection, string userName)
        {
            string docNo = formDataCollection["DOCNO"];
            var payment = GetSingle(docNo);

            var autoJurnal = HelperService.GetConfigValue(PMSConstants.CFG_PayrollAutoJournal + payment.UNITCODE,_context) ==
                 PMSConstants.CFG_PayrollAutoJournalTrue;

            var list = new List<sp_PaymentJournal>();

            if (autoJurnal && payment.PERIOD == 2)           
            list = _context.sp_PaymentJournal(payment.DOCNO).ToList();

                try
                {
                    this.ApproveValidate(payment);
                    payment.UPDATEBY = userName;
                    payment.UPDATED = GetServerTime();
                    payment.STATUS = PMSConstants.TransactionStatusApproved;
                    this.SaveUpdateToDB(payment, userName);

                    if (autoJurnal && list.Count > 0)
                    {
                        var typeList = _serviceJournalType.GetByModul(PMSConstants.GL_Journal_GoodReceiptModulCode);
                        var journal = new TJOURNAL
                        {
                            NO = typeList[0].CODE + "99999",
                            TYPE = typeList[0].CODE,
                            DATE = payment.DOCDATE,
                            UNITCODE = payment.UNITCODE,
                            STATUS = PMSConstants.TransactionStatusProcess,
                            REF = payment.DOCNO,
                            CREATEBY = payment.UPDATEBY,
                            CREATED = payment.UPDATED,
                            UPDATEBY = payment.UPDATEBY,
                            UPDATED = payment.UPDATED,
                            TJOURNALITEM = new List<TJOURNALITEM>(),
                        };

                        foreach (var row in list)
                        {
                            var newDebit = new TJOURNALITEM
                            {
                                ACCOUNTCODE = row.CODE,
                                AMOUNT = Convert.ToDecimal(row.AMOUNT),
                                NOTE = row.NOTE,
                                BLOCKID = row.BLOCKID
                            };
                            journal.TJOURNALITEM.Add(newDebit);
                        }

                        if (journal.TJOURNALITEM.Count > 0)
                        {
                            TJOURNAL JournalNew = _serviceJournal.SaveInsert(journal, userName);
                            string journalCode = JournalNew.CODE;

                            if (HelperService.GetConfigValue(PMSConstants.CFG_JournalAutoApprove + journal.UNITCODE, _context) != PMSConstants.CFG_JournalAutoApproveTrue)
                                _serviceJournal.Approve(journalCode, userName);
                        }

                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.ToString());
                }
            Security.Audit.Insert(userName, _serviceName, GetServerTime(), $"Approve {payment.DOCNO}", _context);
            return true;
        }

        public List<PayrollUploadSAP> GetPaymentJournal(int period, string unitCode, DateTime date)
        {
            if (period == 1)
                return null;
            //return _context.sp_sap_Payment_Journal1(unitCode, date,date).ToList();
            else
            {
                //if (to <= new DateTime(2018, 11, 30))
                //    return repository.GetPaymentJournal2(unitCode, from, to);
                //else
                return this.GetPaymentJournal(unitCode, date);
            }
        }

        private List<PayrollUploadSAP> GetPaymentJournal(string unitId, DateTime date)
        {
            var divList = new List<VDIVISI>();
            var newJournals = new List<SapJournal>();

            var payment = _context.TPAYMENT.Where(d => d.PERIOD == 2 && d.UNITCODE.Equals(unitId) && d.DOCDATE.Month.Equals(date.Month) && d.DOCDATE.Year.Equals(date.Year) ).FirstOrDefault();  
            if (payment == null)
                throw new Exception("Payroll belum di proses.");
            else
            {
                if (payment.STATUS != PMSConstants.TransactionStatusApproved)
                    throw new Exception("Payroll belum di approve.");
            }

            bool hkCalcByFingerPrint = false;
            if ( HelperService.GetConfigValue(PMSConstants.CfgAttendanceCalcByFingerprint + payment.UNITCODE,_context) == PMSConstants.CfgAttendanceCalcByFingerprintTrue)
                hkCalcByFingerPrint = true;

            var unit =  _context.MUNIT.Where(d=> d.UNITCODE.Equals(payment.UNITCODE)).FirstOrDefault();
            var keyList =  this.GetSapJournalKey();
            var costCtrList =  this.GetSapCostCenter();
            var positions = _context.MPOSITION.ToList();

            var attList =  _serviceAttendance.GetGroupingByDocType(payment.UNITCODE, payment.STARTDATE, payment.ENDDATE, hkCalcByFingerPrint).ToList();
            var itemList = _context.TSALARYITEM.Where(d=> d.PAYMENTNO.Equals(payment.DOCNO)).ToList();
            var taxList = _context.TPAYMENTTAX.Where(d=> d.DOCNO.Equals(payment.DOCNO)).ToList();
            var harvestList = _context.THARVESTRESULT1.Where(d=> d.UNITCODE.Equals(payment.UNITCODE) && d.HARVESTDATE.Date>= payment.STARTDATE.Date && d.HARVESTDATE.Date >= payment.ENDDATE.Date).ToList();
            var gerdanList = _context.TGERDANRESULT.Where(d => d.UNITCODE.Equals(payment.UNITCODE) && d.HARVESTDATE.Date >= payment.STARTDATE.Date && d.HARVESTDATE.Date >= payment.ENDDATE.Date).ToList();
            var tkbmList = _context.TLOADINGRESULT.Where(d => d.UNITCODE.Equals(payment.UNITCODE) && d.LOADINGDATE.Date >= payment.STARTDATE.Date && d.LOADINGDATE.Date >= payment.ENDDATE.Date).ToList();
            var operatorList = _context.TOPERATINGRESULT.Where(d => d.UNITCODE.Equals(payment.UNITCODE) && d.LOADINGDATE.Date >= payment.STARTDATE.Date && d.LOADINGDATE.Date >= payment.ENDDATE.Date).ToList();

            var mandorList = _context.TPREMIMANDOR.Where(d=> d.PAYMENTNO.Equals(payment.DOCNO)).ToList();
            var mandor1List = _context.TPREMIMANDOR1.Where(d => d.PAYMENTNO.Equals(payment.DOCNO)).ToList();
            var checkerList = _context.TPREMICHECKER.Where(d => d.PAYMENTNO.Equals(payment.DOCNO)).ToList();

            payment.TPAYMENTDETAIL = _context.TPAYMENTDETAIL.Where(d=> d.DOCNO.Equals(payment.DOCNO)).ToList();
            foreach (var det in payment.TPAYMENTDETAIL)
            {
                if (det.KOPERASI != 0) throw new Exception("Jurnal potongan koperasi belum di setup.");
                if (det.SPSI != 0) throw new Exception("Jurnal potongan SPSI belum di setup.");

                var qCheck = from i in attList where i.EMPID.Equals(det.EMPID) select i;

                if (qCheck.Count() == 0)
                {
                    var attNew = new sp_Attendance_GetGroupingByDocType_result
                    {
                        EMPID = det.EMPID,
                        HK = 1,
                        DOCTYPE = "",
                        MO =""
                    };
                    attList.Add(attNew);
                }

                var attEmp = from i in attList.AsEnumerable() where i.EMPID.ToString() == det.EMPID select i;
                decimal hkTotal = 0;
                foreach (var row in attEmp)
                {
                    hkTotal += Convert.ToDecimal(row.HK);
                }

                var divCount = from i in divList.Where(i=> i.DIVID.Equals(det.COSTCENTER)) select i;
                if (divCount.Count() == 0) divList.AddRange(_context.VDIVISI.Where(d=>d.DIVID.Equals(det.COSTCENTER)));

                var division = (from i in divList.Where(i=> i.DIVID.Equals(det.COSTCENTER)) select i).FirstOrDefault();
                var position = (from i in positions.Where(i=> i.POSITIONID.Equals(det.POSITIONID)) select i).FirstOrDefault();

                string sReference = "D" + division.CODE;
                string sMonthYear = payment.DOCDATE.Month.ToString().PadLeft(2, '0') + "." + payment.DOCDATE.Year.ToString();
                string sText = det.EMPTYPE.Substring(0, 3);
                string sNaturaInternalOrder2 = payment.UNITCODE + "-SLHRG";

                string sSapCostCtr = string.Empty;
                var qCostCtr = from i in costCtrList where i[0] == division.CODE && i[1] == det.POSITIONID select i;
                if (qCostCtr.Count() > 0) sSapCostCtr = qCostCtr.FirstOrDefault()[2];

                string sCostCenter = unitId;
                if (!string.IsNullOrEmpty(sSapCostCtr)) sCostCenter += sSapCostCtr;//Divisi Mill
                else if (position.POSFLAG == 1 || position.POSFLAG == 5) sCostCenter += "MDR";//Mandor 1
                else if (position.POSFLAG == 2 || position.POSFLAG == 3) sCostCenter += "MDR";//Mandor
                else if (division.CODE == "20" || division.CODE == "40") sCostCenter += "GA";
                else if (division.CODE == "30") sCostCenter += "SKUR";
                else
                {
                    sCostCenter += det.EMPTYPE.Substring(0, 3);
                    if (det.EMPTYPE.StartsWith("SKU")) sCostCenter += division.CODE;
                }

                #region  Basic Wages
                if (det.BASICWAGES - det.PERIOD1 != 0)
                {
                    var basicKeys = this.GetJournalKey("GAPOK", division.CODE, keyList);

                    decimal amount = det.BASICWAGES - det.PERIOD1;
                    decimal amountUsed = 0;
                    int i = 1;
                    foreach (var row in attEmp)
                    {
                        decimal amountPerItem;
                        if (i != attEmp.Count())
                        {
                            amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                            amountUsed += amountPerItem;
                        }
                        else
                            amountPerItem = amount - amountUsed;

                        var newBasicRow = new SapJournal();
                        newBasicRow.References = sReference;
                        newBasicRow.HeaderText = "PY Gaji Tahap II " + unitId + " " + sMonthYear;
                        newBasicRow.PostingKey1 = basicKeys[1];
                        newBasicRow.Account1 = basicKeys[2];
                        newBasicRow.PostingKey2 = basicKeys[3];
                        newBasicRow.Account2 = basicKeys[4];

                        if (row.DOCTYPE == "ETS")
                        {
                            newBasicRow.InternalOrder1 = row.MO.ToString();
                            newBasicRow.CostCenter = string.Empty;
                        }
                        else
                        {
                            newBasicRow.InternalOrder1 = string.Empty;
                            newBasicRow.CostCenter = sCostCenter;
                        }

                        newBasicRow.Text = sText;
                        newBasicRow.AMOUNT = amountPerItem;
                        newJournals.Add(newBasicRow);
                    }
                }
                #endregion

                #region Overtime
                if (det.OVERTIME != 0)
                {
                    var overtimeKeys = this.GetJournalKey("LEMBUR", division.CODE, keyList);
                    var newOvertimeRow = new SapJournal();
                    newOvertimeRow.References = sReference;
                    newOvertimeRow.HeaderText = "PY Lembur " + unitId + " " + sMonthYear;
                    newOvertimeRow.PostingKey1 = overtimeKeys[1];
                    newOvertimeRow.Account1 = overtimeKeys[2];
                    newOvertimeRow.PostingKey2 = overtimeKeys[3];
                    newOvertimeRow.Account2 = overtimeKeys[4];
                    newOvertimeRow.CostCenter = sCostCenter;
                    newOvertimeRow.Text = sText;
                    newOvertimeRow.AMOUNT = det.OVERTIME;
                    newJournals.Add(newOvertimeRow);
                }
                #endregion

                #region Piutang
                if (det.DEBIT != 0)
                {
                    var piutangKeys = this.GetJournalKey("PIUTANG", division.CODE, keyList);
                    var newPiutangRow = new SapJournal();
                    newPiutangRow.References = sReference;
                    newPiutangRow.HeaderText = "PY Piutang " + unitId + " " + sMonthYear;
                    newPiutangRow.PostingKey1 = piutangKeys[1];
                    newPiutangRow.Account1 = piutangKeys[2];
                    newPiutangRow.PostingKey2 = piutangKeys[3];
                    newPiutangRow.Account2 = piutangKeys[4];
                    newPiutangRow.CostCenter = sCostCenter;
                    newPiutangRow.Text = sText;
                    newPiutangRow.AMOUNT = det.DEBIT;
                    newJournals.Add(newPiutangRow);
                }
                #endregion

                #region  Premi Non Panen
                if (det.PREMINONPANEN + det.PREMIHADIR != 0)
                {
                    var piutangKeys = this.GetJournalKey("PREMI", division.CODE, keyList);
                    var newPiutangRow = new SapJournal();
                    newPiutangRow.References = sReference;
                    newPiutangRow.HeaderText = "PY Premi Non Panen Tahap II " + unitId + " " + sMonthYear;
                    newPiutangRow.PostingKey1 = piutangKeys[1];
                    newPiutangRow.Account1 = piutangKeys[2];
                    newPiutangRow.PostingKey2 = piutangKeys[3];
                    newPiutangRow.Account2 = piutangKeys[4];
                    newPiutangRow.CostCenter = sCostCenter;
                    newPiutangRow.Text = sText;
                    newPiutangRow.AMOUNT = det.PREMINONPANEN + det.PREMIHADIR;
                    newJournals.Add(newPiutangRow);
                }
                #endregion

                #region  THR dan Bonus
                if (det.INCENTIVE != 0)
                {
                    var incentiveKeys = this.GetJournalKey("THRBNS", division.CODE, keyList);
                    var newIncentiveRow = new SapJournal();
                    newIncentiveRow.References = sReference;
                    newIncentiveRow.HeaderText = "PY THR dan Bonus " + unitId + " " + sMonthYear;
                    newIncentiveRow.PostingKey1 = incentiveKeys[1];
                    newIncentiveRow.Account1 = incentiveKeys[2];
                    newIncentiveRow.PostingKey2 = incentiveKeys[3];
                    newIncentiveRow.Account2 = incentiveKeys[4];
                    newIncentiveRow.CostCenter = sCostCenter;
                    newIncentiveRow.Text = sText;
                    newIncentiveRow.AMOUNT = det.INCENTIVE;
                    newJournals.Add(newIncentiveRow);
                }
                #endregion

                #region  Natura Employee
                if (det.NATURAINCOMEEMPLOYEE != 0)
                {
                    string[] piutangKeys;
                    if (det.RICEPAIDASMONEY == "UANG") piutangKeys = this.GetJournalKey("NATURAMONEY", division.CODE, keyList);
                    else piutangKeys = this.GetJournalKey("NATURARICE", division.CODE, keyList);

                    decimal amount = det.NATURAINCOMEEMPLOYEE;
                    decimal amountUsed = 0;
                    int i = 1;
                    foreach (var row in attEmp)
                    {
                        decimal amountPerItem;
                        if (i != attEmp.Count())
                        {
                            amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                            amountUsed += amountPerItem;
                        }
                        else
                            amountPerItem = amount - amountUsed;

                        var newPiutangRow = new SapJournal();
                        newPiutangRow.References = sReference;
                        newPiutangRow.HeaderText = "PY Natura Pegawai (Beras) " + unitId + " " + sMonthYear;
                        newPiutangRow.PostingKey1 = piutangKeys[1];
                        newPiutangRow.Account1 = piutangKeys[2];
                        newPiutangRow.PostingKey2 = piutangKeys[3];
                        newPiutangRow.Account2 = piutangKeys[4];

                        if (row.DOCTYPE.ToString() == "ETS")
                        {
                            newPiutangRow.InternalOrder1 = row.MO.ToString();
                            newPiutangRow.CostCenter = string.Empty;
                        }
                        else
                        {
                            newPiutangRow.InternalOrder1 = string.Empty;
                            newPiutangRow.CostCenter = sCostCenter;
                        }

                        newPiutangRow.Text = sText;
                        newPiutangRow.InternalOrder2 = sNaturaInternalOrder2;
                        newPiutangRow.AMOUNT = amountPerItem;
                        newJournals.Add(newPiutangRow);
                    }
                }
                #endregion

                #region  Natura Keluarga
                if (det.NATURAINCOME - det.NATURAINCOMEEMPLOYEE != 0)
                {
                    string[] piutangKeys;
                    if (det.RICEPAIDASMONEY == "UANG") piutangKeys = this.GetJournalKey("NATURAMONEY", division.CODE, keyList);
                    else piutangKeys = this.GetJournalKey("NATURARICE", division.CODE, keyList);

                    decimal amount = det.NATURAINCOME - det.NATURAINCOMEEMPLOYEE ;
                    decimal amountUsed = 0;
                    int i = 1;
                    foreach (var row in attEmp)
                    {
                        decimal amountPerItem;
                        if (i != attEmp.Count())
                        {
                            amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                            amountUsed += amountPerItem;
                        }
                        else
                            amountPerItem = amount - amountUsed;

                        var newPiutangRow = new SapJournal();
                        newPiutangRow.References = sReference;
                        newPiutangRow.HeaderText = "PY Natura Keluarga (Beras) " + unitId + " " + sMonthYear;
                        newPiutangRow.PostingKey1 = piutangKeys[1];
                        newPiutangRow.Account1 = piutangKeys[2];
                        newPiutangRow.PostingKey2 = piutangKeys[3];
                        newPiutangRow.Account2 = piutangKeys[4];

                        if (row.DOCTYPE.ToString() == "ETS")
                        {
                            newPiutangRow.InternalOrder1 = row.MO.ToString();
                            newPiutangRow.CostCenter = string.Empty;
                        }
                        else
                        {
                            newPiutangRow.InternalOrder1 = string.Empty;
                            newPiutangRow.CostCenter = sCostCenter;
                        }

                        newPiutangRow.Text = sText;
                        newPiutangRow.InternalOrder2 = sNaturaInternalOrder2;
                        newPiutangRow.AMOUNT = amountPerItem;
                        newJournals.Add(newPiutangRow);
                    }
                }
                #endregion

                #region PPH 21
                if (det.TAX != 0)
                {
                    //PPH21 Ditanggung Karyawan
                    string[] taxEmpKeys;
                    if (det.TAX > 0) taxEmpKeys = this.GetJournalKey("PPH21", division.CODE, keyList);
                    else taxEmpKeys = this.GetJournalKey("PPH21-", division.CODE, keyList);

                    decimal amount = det.TAX < 0 ? det.TAX * -1 : det.TAX;
                    decimal amountUsed = 0;
                    int i = 1;
                    foreach (var row in attEmp)
                    {
                        decimal amountPerItem;
                        if (i != attEmp.Count())
                        {
                            amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                            amountUsed += amountPerItem;
                        }
                        else
                            amountPerItem = amount - amountUsed;

                        var newTaxRow = new SapJournal();
                        newTaxRow.References = sReference;
                        newTaxRow.HeaderText = "PY PPH21 " + unitId + " " + sMonthYear;
                        newTaxRow.PostingKey1 = taxEmpKeys[1];
                        newTaxRow.Account1 = taxEmpKeys[2];
                        newTaxRow.PostingKey2 = taxEmpKeys[3];
                        newTaxRow.Account2 = taxEmpKeys[4];

                        if (row.DOCTYPE.ToString() == "ETS")
                        {
                            newTaxRow.InternalOrder1 = row.MO.ToString();
                            newTaxRow.CostCenter = string.Empty;
                        }
                        else
                        {
                            newTaxRow.InternalOrder1 = string.Empty;
                            newTaxRow.CostCenter = sCostCenter;
                        }

                        newTaxRow.Text = sText;
                        newTaxRow.AMOUNT = amountPerItem;
                        newJournals.Add(newTaxRow);
                    }
                }
                else
                {
                    //PPH21 Ditanggung Perusahaan
                    var q = from i in taxList where i.EMPID == det.EMPID select i;
                    if (q.Count() > 0)
                    {
                        var taxCmp = q.FirstOrDefault();
                        if (taxCmp.TAXMONTH != 0)
                        {
                            string[] taxEmpKeys;
                            if (taxCmp.TAXMONTH > 0) taxEmpKeys = this.GetJournalKey("PPH21B", division.CODE, keyList);
                            else taxEmpKeys = this.GetJournalKey("PPH21B-", division.CODE, keyList);

                            decimal amount = taxCmp.TAXMONTH < 0 ? taxCmp.TAXMONTH * -1 : taxCmp.TAXMONTH;
                            decimal amountUsed = 0;
                            int i = 1;
                            foreach (var row in attEmp)
                            {
                                decimal amountPerItem;
                                if (i != attEmp.Count())
                                {
                                    amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                    amountUsed += amountPerItem;
                                }
                                else
                                    amountPerItem = amount - amountUsed;

                                var newTaxRow = new SapJournal();
                                newTaxRow.References = sReference;
                                newTaxRow.HeaderText = "PY PPH21 " + unitId + " " + sMonthYear;
                                newTaxRow.PostingKey1 = taxEmpKeys[1];
                                newTaxRow.Account1 = taxEmpKeys[2];
                                newTaxRow.PostingKey2 = taxEmpKeys[3];
                                newTaxRow.Account2 = taxEmpKeys[4];

                                if (row.DOCTYPE.ToString() == "ETS")
                                {
                                    newTaxRow.InternalOrder1 = row.MO.ToString();
                                    newTaxRow.CostCenter = string.Empty;
                                }
                                else
                                {
                                    newTaxRow.InternalOrder1 = string.Empty;
                                    newTaxRow.CostCenter = sCostCenter;
                                }

                                newTaxRow.Text = sText;
                                newTaxRow.AMOUNT = amountPerItem;
                                newJournals.Add(newTaxRow);
                            }
                        }
                    }
                }
                #endregion

                #region Jamsostek/BPJS
                var items = (from i in itemList where i.EMPID == det.EMPID select i).ToList();
                foreach (var item in items)
                {
                    //Potongan JHT
                    if (item.TYPEID == PMSConstants.SalaryTypeCodeJamsosJhtEmpId && item.AMOUNT != 0)
                    {
                        var jhtEmpKeys = this.GetJournalKey("POTJHT", division.CODE, keyList);

                        decimal amount = item.AMOUNT;
                        decimal amountUsed = 0;
                        int i = 1;
                        foreach (var row in attEmp)
                        {
                            decimal amountPerItem;
                            if (i != attEmp.Count())
                            {
                                amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                amountUsed += amountPerItem;
                            }
                            else
                                amountPerItem = amount - amountUsed;

                            var newJhtEmpRow = new SapJournal();
                            newJhtEmpRow.References = sReference;
                            newJhtEmpRow.HeaderText = "PY Potongan Jamsostek " + unitId + " " + sMonthYear;
                            newJhtEmpRow.PostingKey1 = jhtEmpKeys[1];
                            newJhtEmpRow.Account1 = jhtEmpKeys[2];
                            newJhtEmpRow.PostingKey2 = jhtEmpKeys[3];
                            newJhtEmpRow.Account2 = jhtEmpKeys[4];

                            if (row.DOCTYPE.ToString() == "ETS")
                            {
                                newJhtEmpRow.InternalOrder1 = row.MO.ToString();
                                newJhtEmpRow.CostCenter = string.Empty;
                            }
                            else
                            {
                                newJhtEmpRow.InternalOrder1 = string.Empty;
                                newJhtEmpRow.CostCenter = sCostCenter;
                            }

                            newJhtEmpRow.Text = sText;
                            newJhtEmpRow.AMOUNT = amountPerItem;
                            newJournals.Add(newJhtEmpRow);
                        }
                    }

                    //Potongan BPJS JP
                    if (item.TYPEID == PMSConstants.SalaryTypeCodeJamsosJpEmpId && item.AMOUNT != 0)
                    {
                        var jpEmpKeys = this.GetJournalKey("POTJAMJP", division.CODE, keyList);

                        decimal amount = item.AMOUNT;
                        decimal amountUsed = 0;
                        int i = 1;
                        foreach (var row in attEmp)
                        {
                            decimal amountPerItem;
                            if (i != attEmp.Count())
                            {
                                amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                amountUsed += amountPerItem;
                            }
                            else
                                amountPerItem = amount - amountUsed;

                            var newJpEmpRow = new SapJournal();
                            newJpEmpRow.References = sReference;
                            newJpEmpRow.HeaderText = "PY Potongan BPJS JP " + unitId + " " + sMonthYear;
                            newJpEmpRow.PostingKey1 = jpEmpKeys[1];
                            newJpEmpRow.Account1 = jpEmpKeys[2];
                            newJpEmpRow.PostingKey2 = jpEmpKeys[3];
                            newJpEmpRow.Account2 = jpEmpKeys[4];

                            if (row.DOCTYPE.ToString() == "ETS")
                            {
                                newJpEmpRow.InternalOrder1 = row.MO.ToString();
                                newJpEmpRow.CostCenter = string.Empty;
                            }
                            else
                            {
                                newJpEmpRow.InternalOrder1 = string.Empty;
                                newJpEmpRow.CostCenter = sCostCenter;
                            }

                            newJpEmpRow.Text = sText;
                            newJpEmpRow.AMOUNT = amountPerItem;
                            newJournals.Add(newJpEmpRow);
                        }
                    }

                    //Potongan BPJS Kesehatan
                    if (item.TYPEID == PMSConstants.SalaryTypeCodeJamsosKesEmpId && item.AMOUNT != 0)
                    {
                        var kesEmpKeys = this.GetJournalKey("POTJAMKES", division.CODE, keyList);

                        decimal amount = item.AMOUNT;
                        decimal amountUsed = 0;
                        int i = 1;
                        foreach (var row in attEmp)
                        {
                            decimal amountPerItem;
                            if (i != attEmp.Count())
                            {
                                amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                amountUsed += amountPerItem;
                            }
                            else
                                amountPerItem = amount - amountUsed;

                            var newKesEmpRow = new SapJournal();
                            newKesEmpRow.References = sReference;
                            newKesEmpRow.HeaderText = "PY Potongan BPJS Kesehatan " + unitId + " " + sMonthYear;
                            newKesEmpRow.PostingKey1 = kesEmpKeys[1];
                            newKesEmpRow.Account1 = kesEmpKeys[2];
                            newKesEmpRow.PostingKey2 = kesEmpKeys[3];
                            newKesEmpRow.Account2 = kesEmpKeys[4];

                            if (row.DOCTYPE.ToString() == "ETS")
                            {
                                newKesEmpRow.InternalOrder1 = row.MO.ToString();
                                newKesEmpRow.CostCenter = string.Empty;
                            }
                            else
                            {
                                newKesEmpRow.InternalOrder1 = string.Empty;
                                newKesEmpRow.CostCenter = sCostCenter;
                            }

                            newKesEmpRow.Text = sText;
                            newKesEmpRow.AMOUNT = amountPerItem;
                            newJournals.Add(newKesEmpRow);
                        }
                    }

                    //Tunjangan JKK
                    if (item.TYPEID == PMSConstants.SalaryTypeCodeJamsosJkkId && item.AMOUNT != 0)
                    {
                        var jkkKeys = this.GetJournalKey("TJNJAMTEK", division.CODE, keyList);

                        decimal amount = item.AMOUNT;
                        decimal amountUsed = 0;
                        int i = 1;
                        foreach (var row in attEmp)
                        {
                            decimal amountPerItem;
                            if (i != attEmp.Count())
                            {
                                amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                amountUsed += amountPerItem;
                            }
                            else
                                amountPerItem = amount - amountUsed;

                            var newJkkRow = new SapJournal();
                            newJkkRow.References = sReference;
                            newJkkRow.HeaderText = "PY Tunjangan Jamsostek " + unitId + " " + sMonthYear;
                            newJkkRow.PostingKey1 = jkkKeys[1];
                            newJkkRow.Account1 = jkkKeys[2];
                            newJkkRow.PostingKey2 = jkkKeys[3];
                            newJkkRow.Account2 = jkkKeys[4];

                            if (row.DOCTYPE.ToString() == "ETS")
                            {
                                newJkkRow.InternalOrder1 = row.MO.ToString();
                                newJkkRow.CostCenter = string.Empty;
                            }
                            else
                            {
                                newJkkRow.InternalOrder1 = string.Empty;
                                newJkkRow.CostCenter = sCostCenter;
                            }

                            newJkkRow.Text = sText;
                            newJkkRow.AMOUNT = amountPerItem;
                            newJournals.Add(newJkkRow);
                        }
                    }

                    //Tunjangan JKM
                    if (item.TYPEID == PMSConstants.SalaryTypeCodeJamsosJkmId && item.AMOUNT != 0)
                    {
                        var jkmKeys = this.GetJournalKey("TJNJAMTEK", division.CODE, keyList);

                        decimal amount = item.AMOUNT;
                        decimal amountUsed = 0;
                        int i = 1;
                        foreach (var row in attEmp)
                        {
                            decimal amountPerItem;
                            if (i != attEmp.Count())
                            {
                                amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                amountUsed += amountPerItem;
                            }
                            else
                                amountPerItem = amount - amountUsed;

                            var newJkmRow = new SapJournal();
                            newJkmRow.References = sReference;
                            newJkmRow.HeaderText = "PY Tunjangan Jamsostek " + unitId + " " + sMonthYear;
                            newJkmRow.PostingKey1 = jkmKeys[1];
                            newJkmRow.Account1 = jkmKeys[2];
                            newJkmRow.PostingKey2 = jkmKeys[3];
                            newJkmRow.Account2 = jkmKeys[4];

                            if (row.DOCTYPE.ToString() == "ETS")
                            {
                                newJkmRow.InternalOrder1 = row.MO.ToString();
                                newJkmRow.CostCenter = string.Empty;
                            }
                            else
                            {
                                newJkmRow.InternalOrder1 = string.Empty;
                                newJkmRow.CostCenter = sCostCenter;
                            }

                            newJkmRow.Text = sText;
                            newJkmRow.AMOUNT = amountPerItem;
                            newJournals.Add(newJkmRow);
                        }
                    }

                    //Tunjangan JHT
                    if (item.TYPEID == PMSConstants.SalaryTypeCodeJamsosJhtCompId && item.AMOUNT != 0)
                    {
                        var jkmKeys = this.GetJournalKey("TJNJHT", division.CODE, keyList);

                        decimal amount = item.AMOUNT;
                        decimal amountUsed = 0;
                        int i = 1;
                        foreach (var row in attEmp)
                        {
                            decimal amountPerItem;
                            if (i != attEmp.Count())
                            {
                                amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                amountUsed += amountPerItem;
                            }
                            else
                                amountPerItem = amount - amountUsed;

                            var newJkmRow = new SapJournal();
                            newJkmRow.References = sReference;
                            newJkmRow.HeaderText = "PY Tunjangan JHT " + unitId + " " + sMonthYear;
                            newJkmRow.PostingKey1 = jkmKeys[1];
                            newJkmRow.Account1 = jkmKeys[2];
                            newJkmRow.PostingKey2 = jkmKeys[3];
                            newJkmRow.Account2 = jkmKeys[4];

                            if (row.DOCTYPE.ToString() == "ETS")
                            {
                                newJkmRow.InternalOrder1 = row.MO.ToString();
                                newJkmRow.CostCenter = string.Empty;
                            }
                            else
                            {
                                newJkmRow.InternalOrder1 = string.Empty;
                                newJkmRow.CostCenter = sCostCenter;
                            }

                            newJkmRow.Text = sText;
                            newJkmRow.AMOUNT = amountPerItem;
                            newJournals.Add(newJkmRow);
                        }
                    }

                    //Tunjangan BPJS Kesehatan
                    if (item.TYPEID == PMSConstants.SalaryTypeCodeJamsosKesCompId && item.AMOUNT != 0)
                    {
                        var jkmKeys = this.GetJournalKey("TJNJAMKES", division.CODE, keyList);

                        decimal amount = item.AMOUNT;
                        decimal amountUsed = 0;
                        int i = 1;
                        foreach (var row in attEmp)
                        {
                            decimal amountPerItem;
                            if (i != attEmp.Count())
                            {
                                amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                amountUsed += amountPerItem;
                            }
                            else
                                amountPerItem = amount - amountUsed;

                            var newJkmRow = new SapJournal();
                            newJkmRow.References = sReference;
                            newJkmRow.HeaderText = "PY Tunjangan BPJS Kesehatan " + unitId + " " + sMonthYear;
                            newJkmRow.PostingKey1 = jkmKeys[1];
                            newJkmRow.Account1 = jkmKeys[2];
                            newJkmRow.PostingKey2 = jkmKeys[3];
                            newJkmRow.Account2 = jkmKeys[4];

                            if (row.DOCTYPE.ToString() == "ETS")
                            {
                                newJkmRow.InternalOrder1 = row.MO.ToString();
                                newJkmRow.CostCenter = string.Empty;
                            }
                            else
                            {
                                newJkmRow.InternalOrder1 = string.Empty;
                                newJkmRow.CostCenter = sCostCenter;
                            }

                            newJkmRow.Text = sText;
                            newJkmRow.AMOUNT = amountPerItem;
                            newJournals.Add(newJkmRow);
                        }
                    }

                    //Tunjangan BPJS JP
                    if (item.TYPEID == PMSConstants.SalaryTypeCodeJamsosJpCompId && item.AMOUNT != 0)
                    {
                        var jkmKeys = this.GetJournalKey("TJNJAMJP", division.CODE, keyList);

                        decimal amount = item.AMOUNT;
                        decimal amountUsed = 0;
                        int i = 1;
                        foreach (var row in attEmp)
                        {
                            decimal amountPerItem;
                            if (i != attEmp.Count())
                            {
                                amountPerItem = Decimal.Round(amount * (Convert.ToDecimal(row.HK) / hkTotal), 2);
                                amountUsed += amountPerItem;
                            }
                            else
                                amountPerItem = amount - amountUsed;

                            var newJkmRow = new SapJournal();
                            newJkmRow.References = sReference;
                            newJkmRow.HeaderText = "PY Tunjangan BPJS JP " + unitId + " " + sMonthYear;
                            newJkmRow.PostingKey1 = jkmKeys[1];
                            newJkmRow.Account1 = jkmKeys[2];
                            newJkmRow.PostingKey2 = jkmKeys[3];
                            newJkmRow.Account2 = jkmKeys[4];

                            if (row.DOCTYPE.ToString() == "ETS")
                            {
                                newJkmRow.InternalOrder1 = row.MO.ToString();
                                newJkmRow.CostCenter = string.Empty;
                            }
                            else
                            {
                                newJkmRow.InternalOrder1 = string.Empty;
                                newJkmRow.CostCenter = sCostCenter;
                            }

                            newJkmRow.Text = sText;
                            newJkmRow.AMOUNT = amountPerItem;
                            newJournals.Add(newJkmRow);
                        }
                    }
                }
                #endregion

                #region Premi Panen
                var hvtResults = (from i in harvestList where i.EMPLOYEEID == det.EMPID && i.EFLAG == true select i).ToList();
                foreach (var hvt in hvtResults)
                {
                    var premiAmount = hvt.NEWINCENTIVE1 + hvt.NEWINCENTIVE2 + hvt.NEWINCENTIVE3 - hvt.FINEAMOUNT
                        + hvt.INCENTIVEPKKTGI + hvt.HAINCENTIVE + hvt.ATTINCENTIVE;

                    //Premi Panen Karyawan PB
                    if (hvt.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah && premiAmount != 0)
                    {
                        string[] jkmKeys;
                        if (premiAmount > 0)
                            jkmKeys = this.GetJournalKey("HVPRMPN", division.CODE, keyList);
                        else
                            jkmKeys = this.GetJournalKey("HVPRMPN-", division.CODE, keyList);

                        if (premiAmount < 0) premiAmount = premiAmount * -1;

                        if (hvt.BLOCKID.Substring(0, 4) == unitId)
                        {
                            var newJkmRow = new SapJournal();
                            newJkmRow.References = sReference;
                            newJkmRow.HeaderText = "PY HV Premi PB Karyawan " + unitId + " " + sMonthYear;
                            newJkmRow.PostingKey1 = jkmKeys[1];
                            newJkmRow.Account1 = jkmKeys[2];
                            newJkmRow.PostingKey2 = jkmKeys[3];
                            newJkmRow.Account2 = jkmKeys[4];
                            newJkmRow.CostCenter = hvt.BLOCKID.Replace("-", string.Empty);
                            newJkmRow.Text = sText;
                            newJkmRow.AMOUNT = premiAmount;
                            newJournals.Add(newJkmRow);
                        }
                    }

                    //Premi Panen Karyawan KB
                    if (hvt.HARVESTTYPE ==  PMSConstants.HarvestTypeKutipBrondol && premiAmount != 0)
                    {
                        string[] jkmKeys;
                        if (premiAmount > 0)
                            jkmKeys = this.GetJournalKey("HVPRMKB", division.CODE, keyList);
                        else
                            jkmKeys = this.GetJournalKey("HVPRMKB-", division.CODE, keyList);

                        if (premiAmount < 0) premiAmount = premiAmount * -1;

                        if (hvt.BLOCKID.Substring(0, 4) == unitId)
                        {
                            var newJkmRow = new SapJournal();
                            newJkmRow.References = sReference;
                            newJkmRow.HeaderText = "PY HV Premi KB Karyawan " + unitId + " " + sMonthYear;
                            newJkmRow.PostingKey1 = jkmKeys[1];
                            newJkmRow.Account1 = jkmKeys[2];
                            newJkmRow.PostingKey2 = jkmKeys[3];
                            newJkmRow.Account2 = jkmKeys[4];
                            newJkmRow.CostCenter = hvt.BLOCKID.Replace("-", string.Empty);
                            newJkmRow.Text = sText;
                            newJkmRow.AMOUNT = premiAmount;
                            newJournals.Add(newJkmRow);
                        }
                    }
                }
                #endregion

                #region Premi Gerdan
                var grdResults = (from i in gerdanList where i.GEMPID == det.EMPID select i).ToList();
                foreach (var grd in grdResults)
                {
                    var premiAmount = grd.NEWINCENTIVE1 + grd.NEWINCENTIVE2 + grd.NEWINCENTIVE3;

                    //Premi Panen Gerdan PB
                    if (grd.HARVESTTYPE == PMSConstants.HarvestTypePotongBuah && premiAmount != 0)
                    {
                        string[] jkmKeys;
                        if (premiAmount > 0)
                            jkmKeys = this.GetJournalKey("HVPRMPN", division.CODE, keyList);
                        else
                            jkmKeys = this.GetJournalKey("HVPRMPN-", division.CODE, keyList);

                        if (premiAmount < 0) premiAmount = premiAmount * -1;

                        if (grd.BLOCKID.Substring(0, 4) == unitId)
                        {
                            var newJkmRow = new SapJournal();
                            newJkmRow.References = sReference;
                            newJkmRow.HeaderText = "PY HV Premi PB Gardan " + unitId + " " + sMonthYear;
                            newJkmRow.PostingKey1 = jkmKeys[1];
                            newJkmRow.Account1 = jkmKeys[2];
                            newJkmRow.PostingKey2 = jkmKeys[3];
                            newJkmRow.Account2 = jkmKeys[4];
                            newJkmRow.CostCenter = grd.BLOCKID.Replace("-", string.Empty);
                            newJkmRow.Text = sText;
                            newJkmRow.AMOUNT = premiAmount;
                            newJournals.Add(newJkmRow);
                        }
                    }

                    //Premi Panen Gerdan KB
                    if (grd.HARVESTTYPE == PMSConstants.HarvestTypeKutipBrondol && premiAmount != 0)
                    {
                        string[] jkmKeys;
                        if (premiAmount > 0)
                            jkmKeys = this.GetJournalKey("HVPRMKB", division.CODE, keyList);
                        else
                            jkmKeys = this.GetJournalKey("HVPRMKB-", division.CODE, keyList);

                        if (premiAmount < 0) premiAmount = premiAmount * -1;

                        if (grd.BLOCKID.Substring(0, 4) == unitId)
                        {
                            var newJkmRow = new SapJournal();
                            newJkmRow.References = sReference;
                            newJkmRow.HeaderText = "PY HV Premi KB Gardan " + unitId + " " + sMonthYear;
                            newJkmRow.PostingKey1 = jkmKeys[1];
                            newJkmRow.Account1 = jkmKeys[2];
                            newJkmRow.PostingKey2 = jkmKeys[3];
                            newJkmRow.Account2 = jkmKeys[4];
                            newJkmRow.CostCenter = grd.BLOCKID.Replace("-", string.Empty);
                            newJkmRow.Text = sText;
                            newJkmRow.AMOUNT = premiAmount;
                            newJournals.Add(newJkmRow);
                        }
                    }
                }
                #endregion

                #region Premi TKBM
                var tkbmResults = (from i in tkbmList where i.EMPLOYEEID == det.EMPID && i.EFLAG == true select i).ToList();
                foreach (var tkbm in tkbmResults)
                {
                    var premiAmount = tkbm.NEWINCENTIVE1 + tkbm.NEWINCENTIVE2 + tkbm.NEWINCENTIVE3 + tkbm.NEWINCENTIVE4
                        + tkbm.NEWINCENTIVE5 + tkbm.NEWINCENTIVE6;

                    if (premiAmount != 0)
                    {
                        string[] tkbmKeys;
                        if (premiAmount > 0)
                            tkbmKeys = this.GetJournalKey("HTTKBM", division.CODE, keyList);
                        else
                            tkbmKeys = this.GetJournalKey("HTTKBM-", division.CODE, keyList);

                        if (premiAmount < 0) premiAmount = premiAmount * -1;

                        if (tkbm.BLOCKID.Substring(0, 4) == unitId)
                        {
                            var newTkbmRow = new SapJournal();
                            newTkbmRow.References = sReference;
                            newTkbmRow.HeaderText = "PY HV Premi Bongkar Muat " + unitId + " " + sMonthYear;
                            newTkbmRow.PostingKey1 = tkbmKeys[1];
                            newTkbmRow.Account1 = tkbmKeys[2];
                            newTkbmRow.PostingKey2 = tkbmKeys[3];
                            newTkbmRow.Account2 = tkbmKeys[4];
                            newTkbmRow.CostCenter = tkbm.BLOCKID.Replace("-", string.Empty);
                            newTkbmRow.Text = sText;
                            newTkbmRow.AMOUNT = premiAmount;
                            newJournals.Add(newTkbmRow);
                        }
                    }
                }
                #endregion

                #region Operator
                var oprResults = (from i in operatorList where i.DRIVERID == det.EMPID select i).ToList();
                foreach (var opr in oprResults)
                {
                    var premiAmount = opr.NEWINCENTIVE1 + opr.NEWINCENTIVE2 + opr.NEWINCENTIVE3 + opr.NEWINCENTIVE4
                        + opr.NEWINCENTIVE5 + opr.NEWINCENTIVE6;

                    if (premiAmount != 0)
                    {
                        string[] oprKeys;
                        if (premiAmount > 0)
                            oprKeys = this.GetJournalKey("HTOPR", division.CODE, keyList);
                        else
                            oprKeys = this.GetJournalKey("HTOPR-", division.CODE, keyList);

                        if (premiAmount < 0) premiAmount = premiAmount * -1;

                        if (opr.BLOCKID.Substring(0, 4) == unitId)
                        {
                            var newOprRow = new SapJournal();
                            newOprRow.References = sReference;
                            newOprRow.HeaderText = "PY HV Premi Operator " + unitId + " " + sMonthYear;
                            newOprRow.PostingKey1 = oprKeys[1];
                            newOprRow.Account1 = oprKeys[2];
                            newOprRow.PostingKey2 = oprKeys[3];
                            newOprRow.Account2 = oprKeys[4];
                            newOprRow.CostCenter = opr.BLOCKID.Replace("-", string.Empty);
                            newOprRow.Text = sText;
                            newOprRow.AMOUNT = premiAmount;
                            newJournals.Add(newOprRow);
                        }
                    }
                }
                #endregion

                #region Premi Mandor
                var mdrResults = (from i in mandorList where i.MANDORID == det.EMPID select i).ToList();
                foreach (var mdr in mdrResults)
                {
                    var premiAmount = mdr.PREMIEMP - mdr.MANDORFINE;

                    //Premi Panen Mandor PB
                    if (mdr.HARVESTTYPE == (int)PMSConstants.HarvestTypePotongBuah && premiAmount != 0)
                    {
                        string[] jkmKeys;
                        if (premiAmount > 0)
                            jkmKeys = this.GetJournalKey("HVPRMPNMDR", division.CODE, keyList);
                        else
                            jkmKeys = this.GetJournalKey("HVPRMPNMDR-", division.CODE, keyList);

                        var newJkmRow = new SapJournal();
                        newJkmRow.References = sReference;
                        newJkmRow.HeaderText = "PY HV Premi PB Mandor Tahap II " + unitId + " " + sMonthYear;
                        newJkmRow.PostingKey1 = jkmKeys[1];
                        newJkmRow.Account1 = jkmKeys[2];
                        newJkmRow.PostingKey2 = jkmKeys[3];
                        newJkmRow.Account2 = jkmKeys[4];
                        newJkmRow.CostCenter = sCostCenter;
                        newJkmRow.Text = sText;
                        newJkmRow.AMOUNT = premiAmount;
                        newJournals.Add(newJkmRow);
                    }

                    //Premi Panen Mandor KB
                    if (mdr.HARVESTTYPE == (int)PMSConstants.HarvestTypeKutipBrondol && premiAmount != 0)
                    {
                        string[] jkmKeys;
                        if (premiAmount > 0)
                            jkmKeys = this.GetJournalKey("HVPRMKBMDR", division.CODE, keyList);
                        else
                            jkmKeys = this.GetJournalKey("HVPRMKBMDR-", division.CODE, keyList);

                        var newJkmRow = new SapJournal();
                        newJkmRow.References = sReference;
                        newJkmRow.HeaderText = "PY HV Premi KB Mandor Tahap II " + unitId + " " + sMonthYear;
                        newJkmRow.PostingKey1 = jkmKeys[1];
                        newJkmRow.Account1 = jkmKeys[2];
                        newJkmRow.PostingKey2 = jkmKeys[3];
                        newJkmRow.Account2 = jkmKeys[4];
                        newJkmRow.CostCenter = sCostCenter;
                        newJkmRow.Text = sText;
                        newJkmRow.AMOUNT = premiAmount;
                        newJournals.Add(newJkmRow);
                    }
                }
                #endregion

                #region Premi Krani
                var krnResults = (from i in mandorList where i.KRANIID == det.EMPID select i).ToList();
                foreach (var krn in krnResults)
                {
                    var premiAmount = krn.PREMIKRANI - krn.KRANIFINE;

                    //Premi Panen Krani PB
                    if (krn.HARVESTTYPE == (int)PMSConstants.HarvestTypePotongBuah && premiAmount != 0)
                    {
                        string[] jkmKeys;
                        if (premiAmount > 0)
                            jkmKeys = this.GetJournalKey("HVPRMPNKRN", division.CODE, keyList);
                        else
                            jkmKeys = this.GetJournalKey("HVPRMPNKRN-", division.CODE, keyList);

                        var newJkmRow = new SapJournal();
                        newJkmRow.References = sReference;
                        newJkmRow.HeaderText = "PY HV Premi PB Krani Tahap II " + unitId + " " + sMonthYear;
                        newJkmRow.PostingKey1 = jkmKeys[1];
                        newJkmRow.Account1 = jkmKeys[2];
                        newJkmRow.PostingKey2 = jkmKeys[3];
                        newJkmRow.Account2 = jkmKeys[4];
                        newJkmRow.CostCenter = sCostCenter;
                        newJkmRow.Text = sText;
                        newJkmRow.AMOUNT = premiAmount;
                        newJournals.Add(newJkmRow);
                    }

                    //Premi Panen Krani KB
                    if (krn.HARVESTTYPE == (int)PMSConstants.HarvestTypeKutipBrondol && premiAmount != 0)
                    {
                        string[] jkmKeys;
                        if (premiAmount > 0)
                            jkmKeys = this.GetJournalKey("HVPRMKBKRN", division.CODE, keyList);
                        else
                            jkmKeys = this.GetJournalKey("HVPRMKBKRN-", division.CODE, keyList);

                        var newJkmRow = new SapJournal();
                        newJkmRow.References = sReference;
                        newJkmRow.HeaderText = "PY HV Premi KB Krani Tahap II " + unitId + " " + sMonthYear;
                        newJkmRow.PostingKey1 = jkmKeys[1];
                        newJkmRow.Account1 = jkmKeys[2];
                        newJkmRow.PostingKey2 = jkmKeys[3];
                        newJkmRow.Account2 = jkmKeys[4];
                        newJkmRow.CostCenter = sCostCenter;
                        newJkmRow.Text = sText;
                        newJkmRow.AMOUNT = premiAmount;
                        newJournals.Add(newJkmRow);
                    }
                }
                #endregion

                #region Premi Mandor 1
                var mdr1Results = (from i in mandor1List where i.MANDORID == det.EMPID select i).ToList();
                foreach (var mdr1 in mdr1Results)
                {
                    var premiAmount = mdr1.PREMI - mdr1.FINE;

                    //Premi Panen Mandor 1
                    if (premiAmount != 0)
                    {
                        string[] jkmKeys = this.GetJournalKey("HVPRMMDR1", division.CODE, keyList);
                        var newJkmRow = new SapJournal();
                        newJkmRow.References = sReference;
                        newJkmRow.HeaderText = "PY HV Premi Mandor 1 Tahap II " + unitId + " " + sMonthYear;
                        newJkmRow.PostingKey1 = jkmKeys[1];
                        newJkmRow.Account1 = jkmKeys[2];
                        newJkmRow.PostingKey2 = jkmKeys[3];
                        newJkmRow.Account2 = jkmKeys[4];
                        newJkmRow.CostCenter = sCostCenter;
                        newJkmRow.Text = sText;
                        newJkmRow.AMOUNT = premiAmount;
                        newJournals.Add(newJkmRow);
                    }
                }
                #endregion

                #region Premi Checker
                var chkResults = (from i in checkerList where i.CHECKERID == det.EMPID select i).ToList();
                foreach (var chk in chkResults)
                {
                    var premiAmount = chk.PREMI - chk.FINE;

                    //Premi Panen Checker
                    if (premiAmount != 0)
                    {
                        string[] jkmKeys = this.GetJournalKey("HVPRMPNKRN", division.CODE, keyList);
                        var newJkmRow = new SapJournal();
                        newJkmRow.References = sReference;
                        newJkmRow.HeaderText = "PY HV Premi Checker Tahap II " + unitId + " " + sMonthYear;
                        newJkmRow.PostingKey1 = jkmKeys[1];
                        newJkmRow.Account1 = jkmKeys[2];
                        newJkmRow.PostingKey2 = jkmKeys[3];
                        newJkmRow.Account2 = jkmKeys[4];
                        newJkmRow.CostCenter = sCostCenter;
                        newJkmRow.Text = sText;
                        newJkmRow.AMOUNT = premiAmount;
                        newJournals.Add(newJkmRow);
                    }
                }
                #endregion
            }

            var finalList = (from i in newJournals
                             where i.AMOUNT != 0
                             group i by new { i.References, i.HeaderText, i.PostingKey1, i.PostingKey2, i.Account1, i.Account2, i.CostCenter, i.Text, i.InternalOrder2, i.InternalOrder1 }
                             into g
                             select new
                             { g.Key.References, g.Key.HeaderText, g.Key.PostingKey1, g.Key.PostingKey2, g.Key.Account1, g.Key.Account2, g.Key.CostCenter, g.Key.Text, g.Key.InternalOrder2, g.Key.InternalOrder1, AMOUNT = g.Sum(x => x.AMOUNT) }).ToList();


            List<PayrollUploadSAP> ds = new List<PayrollUploadSAP>();
            foreach (var item in finalList)
            {
                var row = new PayrollUploadSAP {
                    Line_Item = "1",
                    Doc_Date = payment.DOCDATE.ToString("yyyyMMdd"),
                    Doc_Type = "ZI",
                    Company_Code = unit.LEGALID,
                    Posting_Date = payment.DOCDATE.ToString("yyyyMMdd"),
                    Period = "2",
                    Currency = "IDR",
                    Reference = item.References,
                    Doc_Header_Text = item.HeaderText,
                    Posting_Key_1 = item.PostingKey1,
                    Account_1 = item.Account1,
                    Special_GL_1 = string.Empty,
                    Amount_1 = item.AMOUNT.ToString(),
                    Tax_Code_1 = string.Empty,
                    Business_Area_1 = payment.UNITCODE,
                    Internal_Order_1 = item.InternalOrder1,
                    Cost_Center = item.CostCenter,
                    Profit_Center = string.Empty,
                    Assignment_1 = "R2",
                    Text_1 = item.Text,
                    Posting_Key_2 = item.PostingKey2,
                    Account_2 = item.Account2,
                    Special_GL_2 = string.Empty,
                    No_column_name = string.Empty,
                    Amount_2 = item.AMOUNT.ToString(),
                    Tax_Code_2 = string.Empty,
                    Business_Area_2 = payment.UNITCODE,
                    Internal_Order_2 = item.InternalOrder2,
                    Cost_Center_2 = string.Empty,
                    Profit_Center_2 = string.Empty,
                    Assignment_2 = "R2",
                    Text_2 = item.Text
            };
                ds.Add(row);
            }

            return ds;
        }

        public class PayrollUploadSAP
        {
            public string Line_Item { get; set; }
            public string Doc_Date { get; set; }
            public string Doc_Type { get; set; }
            public string Company_Code { get; set; }
            public string Posting_Date { get; set; }
            public string Period { get; set; }
            public string Currency { get; set; }
            public string Reference { get; set; }
            public string Doc_Header_Text { get; set; }
            public string Posting_Key_1 { get; set; }
            public string Account_1 { get; set; }
            public string Special_GL_1 { get; set; }
            public string Amount_1 { get; set; }
            public string Tax_Code_1 { get; set; }
            public string Business_Area_1 { get; set; }
            public string Internal_Order_1 { get; set; }
            public string Cost_Center { get; set; }
            public string Profit_Center { get; set; }
            public string Assignment_1 { get; set; }
            public string Text_1 { get; set; }
            public string Posting_Key_2 { get; set; }
            public string Account_2 { get; set; }
            public string Special_GL_2 { get; set; }
            public string No_column_name { get; set; }
            public string Amount_2 { get; set; }
            public string Tax_Code_2 { get; set; }
            public string Business_Area_2 { get; set; }
            public string Internal_Order_2 { get; set; }
            public string Cost_Center_2 { get; set; }
            public string Profit_Center_2 { get; set; }
            public string Assignment_2 { get; set; }
            public string Text_2 { get; set; }
        }

        public class SapJournal
        {
            public string References { get; set; }
            public string HeaderText { get; set; }
            public string PostingKey1 { get; set; }
            public string Account1 { get; set; }
            public string PostingKey2 { get; set; }
            public string Account2 { get; set; }
            public string CostCenter { get; set; }
            public string Text { get; set; }
            public string InternalOrder1 { get; set; }
            public string InternalOrder2 { get; set; }
            public decimal AMOUNT { get; set; }

            public SapJournal()
            {
                this.References = string.Empty;
                this.HeaderText = string.Empty;
                this.PostingKey1 = string.Empty;
                this.Account1 = string.Empty;
                this.PostingKey2 = string.Empty;
                this.Account2 = string.Empty;
                this.CostCenter = string.Empty;
                this.Text = string.Empty;
                this.InternalOrder1 = string.Empty;
                this.InternalOrder2 = string.Empty;
                this.AMOUNT = 0;
            }
        }

        private string[] GetJournalKey(string name, string divCode, List<string[]> keys)
        {
            var q = from i in keys where i[0] == name select i;
            if (q.Count() == 0) throw new Exception("Posting Key belum terdaftar");

            var key0 = q.FirstOrDefault();
            string key1 = key0[1];
            string key2 = key0[2];
            string key3 = key0[3];
            string key4 = key0[4];

            if (divCode.StartsWith("S")) divCode = "20";
            divCode = divCode.PadLeft(3, '0');

            if (divCode == "00Z") divCode = "BBT";

            key1 = key1.Replace("<COMPANY>", divCode);
            key2 = key2.Replace("<COMPANY>", divCode);
            key3 = key3.Replace("<COMPANY>", divCode);
            key4 = key4.Replace("<COMPANY>", divCode);

            if (key1 == "DIV020" || key1 == "DIV030") key1 = "GAJI-KB";
            else if (key1 == "DIV040") key1 = "GAJI-CE";

            if (key2 == "DIV020" || key2 == "DIV030") key2 = "GAJI-KB";
            else if (key2 == "DIV040") key2 = "GAJI-CE";

            if (key3 == "DIV020" || key3 == "DIV030") key3 = "GAJI-KB";
            else if (key3 == "DIV040") key3 = "GAJI-CE";

            if (key4 == "DIV020" || key4 == "DIV030") key4 = "GAJI-KB";
            else if (key4 == "DIV040") key4 = "GAJI-CE";

            var keyRes = new string[5];
            keyRes[0] = string.Empty;
            keyRes[1] = key1;
            keyRes[2] = key2;
            keyRes[3] = key3;
            keyRes[4] = key4;
            return keyRes;
        }

        public List<string[]> GetSapJournalKey()
        {
            var list = new List<string[]>();
            var table = HelperService.GetSapJournalKey(_context);
            foreach (var row in table)
            {
                var value = new string[5];
                value[0] = row.NAME.ToString();
                value[1] = row.PK1.ToString();
                value[2] = row.ACC1.ToString();
                value[3] = row.PK2.ToString();
                value[4] = row.Acc2.ToString();
                list.Add(value);
            }
            return list;
        }

        public List<string[]> GetSapCostCenter()
        {
            var list = new List<string[]>();
            var table =  HelperService.GetSapCostCenter(_context);
            foreach (var row in table)
            {
                var value = new string[3];
                value[0] = row.DIVID.ToString();
                value[1] = row.POSID.ToString();
                value[2] = row.COSTCTR.ToString();
                list.Add(value);
            }
            return list;
        }

    }
}
