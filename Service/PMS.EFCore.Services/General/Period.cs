using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{
    public class Period : EntityFactory<MPERIOD,MPERIOD,FilterPeriod, PMSContextBase>
    {
        AuthenticationServiceBase _authenticationService;
        public Period(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Period";
            _authenticationService = authenticationService;
            
        }

        private void Validate (MPERIOD record, string user)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(record.UNITCODE)) result += "Unit harus diisi." + Environment.NewLine;
            if (record.MONTH == 0) result += "Bulan harus diisi." + Environment.NewLine;
            if (record.YEAR == 0) result += "Tahun harus diisi." + Environment.NewLine;
            if (record.FROM1 == new DateTime()) result += "Tanggal awal period 1 harus diisi." + Environment.NewLine;
            if (record.TO1 == new DateTime()) result += "Tanggal akhir period 1 harus diisi." + Environment.NewLine;
            if (record.FROM2 == new DateTime()) result += "Tanggal awal period 2 harus diisi." + Environment.NewLine;
            if (record.TO2 == new DateTime()) result += "Tanggal akhir period 2 harus diisi." + Environment.NewLine;
            if (_context.MPERIOD.SingleOrDefault(p => p.PERIODCODE.Equals(record.PERIODCODE)) != null)
                result += "Periode sudah pernah diinput." + Environment.NewLine;

            if (result != string.Empty) throw new Exception(result);
        }

        public override MPERIOD NewRecord(string userName)
        {
            short month = Convert.ToInt16(DateTime.Now.Month);
            short year = Convert.ToInt16(DateTime.Now.Year);
            
            MPERIOD record = new MPERIOD
            {
                MONTH = month,
                YEAR = year,
                FROM1 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                FROM2 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                TO1 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                TO2 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)),
            };
            return record;
        }

        protected override MPERIOD GetSingleFromDB(params  object[] keyValues)
        {
            string periodcode = keyValues[0].ToString();
            return _context.MPERIOD.Where(d => d.PERIODCODE.Equals(periodcode)).SingleOrDefault();
        }

        protected override MPERIOD BeforeSave(MPERIOD record, string userName, bool newRecord)
        {
            DateTime now = HelperService.GetServerDateTime(1, _context);
            if (newRecord)
            {
                string NewId = record.UNITCODE + record.MONTH.ToString().PadLeft(2, '0') + record.YEAR;
                record.PERIODCODE = NewId;
                record.CREATEBY = userName;
                record.CREATED = now;
            }
            record.UPDATEBY = userName;
            record.UPDATED = now;
            this.Validate(record, userName);

            return record;
        }

       
        
        public void CheckCloseYearValidPeriod(string unitCode, int year)
        {
            var dateTime = new DateTime(year, 12, 1);
            var period = _context.MPERIOD.Where(d => d.UNITCODE == unitCode && ((d.FROM1 <= dateTime && d.TO1 >= dateTime && d.ACTIVE1 == true) || (d.FROM2 <= dateTime && d.TO2 >= dateTime && d.ACTIVE2 == true))).ToList();
            if (period.Count() == 0)
                throw new Exception("Tanggal harus dalam periode aktif (" + unitCode + ").");
        }

        public void CheckValidPeriod(string unitCode, DateTime datetime)
        {
            var period = _context.MPERIOD.Where(d => d.UNITCODE == unitCode && ((d.FROM1 <= datetime && d.TO1 >= datetime && d.ACTIVE1 == true ) ||(d.FROM2 <= datetime && d.TO2 >= datetime && d.ACTIVE2 == true))).ToList();
            if (period.Count() == 0)
                throw new Exception("Tanggal harus dalam periode aktif (" + unitCode + ").");
        }

        public void CheckMaxPeriod(string unitId, DateTime dateTime)
        {
            var maxDay = 0;
            var sDay = HelperService.GetConfigValue(PMSConstants.CfgTransactionMaxInput + unitId,_context);
            if (!string.IsNullOrEmpty(sDay)) maxDay = StandardUtility.ToInt(sDay);

            if (maxDay > 0)
            {
                var currDate =  HelperService.GetServerDateTime(1,_context);
                var diff = currDate - dateTime;
                if (Math.Floor(diff.TotalDays) > maxDay) throw new Exception("Maksimal input transaksi " + maxDay + " hari yang lalu (" + unitId + ").");
            }
        }

        public Int16 CheckPeriod(string unitCode, DateTime datetime)
        {
            try
            {
                this.CheckValidPeriod(unitCode, datetime);
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public override IEnumerable<MPERIOD> GetList(FilterPeriod filter)
        {            
            var criteria = PredicateBuilder.True<MPERIOD>();
            try
            {
                if (!string.IsNullOrWhiteSpace(filter.UserName))
                {
                    bool allUnits = true;
                    List<string> authorizedUnitIds = new List<string>();

                    //Authorization check
                    //Get Authorization By User Name
                    _authenticationService.GetAuthorizedLocationIdByUserName(filter.UserName, out allUnits);


                    if (!allUnits)
                    {
                        authorizedUnitIds = _authenticationService.GetAuthorizedUnit(filter.UserName, string.Empty)
                            .Select(d => d.UNITCODE)
                            .ToList();
                        criteria = criteria.And(p => authorizedUnitIds.Contains(p.UNITCODE));

                    }
                }

                if (filter.Year != null)
                    criteria = criteria.And(p => p.YEAR.Equals(filter.Year));

                if (filter.Month != null)
                    criteria = criteria.And(p => p.MONTH.Equals(filter.Month));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(d => d.UNITCODE == filter.UnitID);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    criteria = criteria.And(p =>
                        p.REMARK.ToLower().Contains(filter.LowerCasedSearchTerm) || p.PERIODNAME.ToLower().Contains(filter.LowerCasedSearchTerm));

                //criteria = criteria.And(d => (d.MONTH >= filter.StartDate.Date.Month && d.MONTH <= filter.EndDate.Date.Month) &&
                //(d.YEAR >= filter.StartDate.Date.Year && d.YEAR <= filter.EndDate.Date.Year)
                //);

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(d => d.ACTIVE2 == filter.IsActive.Value);

                if (filter.PageSize <= 0)
                    return _context.MPERIOD.Where(criteria);
                return _context.MPERIOD.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return null; }
        }

        public MPERIOD GetSingleBy(object Parameter)
        {
            FilterPeriod filter = Parameter as FilterPeriod;

            if (filter.dateTime.Year == 0001 || filter.dateTime.Year < 1) //by dateTime null
            {
                if (filter.Month != 0 && filter.Year != 0) //by  unitcode, month, year
                {
                    return _context.MPERIOD.Where(p => p.UNITCODE.Equals(filter.UnitCode) && p.MONTH.Equals(filter.Month) && p.YEAR.Equals(filter.Year)).SingleOrDefault();
                }
                else // by current
                {
                    return _context.MPERIOD.Where(p => p.ACTIVE2 == true && p.UNITCODE.Equals(filter.UnitCode)).SingleOrDefault();
                }
            }
            else //by dateTime
            {
                return _context.MPERIOD.Where(p => p.UNITCODE.Equals(filter.UnitCode) && p.MONTH.ToString().Equals(filter.dateTime.Month.ToString()) && p.YEAR.ToString().Equals(filter.dateTime.Year.ToString())).SingleOrDefault();
            }
        }
    }
}