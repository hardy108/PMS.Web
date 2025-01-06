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
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class HarvestingResult : EntityFactory<THARVESTRESULT1,THARVESTRESULT1, FilterHarvestResult, PMSContextBase>
    {
        private Period _periodService;
        private AuthenticationServiceBase _authenticationService;
        public HarvestingResult(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "HarvestResult";
            _authenticationService = authenticationService;
            _periodService = new Period(context,_authenticationService,auditContext);
        }

        public void DeleteByDiv(string divId, DateTime date)
        {
            List<THARVESTRESULT1> results =
                (
                from p in _context.THARVESTRESULT1
                .Where(d => d.DIVID.Equals(divId) && d.HARVESTDATE.Date == date.Date)
                select p
                ).ToList();
            foreach (THARVESTRESULT1 p in results)
            {
                p.STATUS = "D";
                p.UPDATED = GetServerTime();

            }
            _context.SaveChanges();
        }

        public void DeleteByPaymentNo(string paymentNo)
        {
            List<THARVESTRESULT1> results =
                (
                from p in _context.THARVESTRESULT1
                .Where(d => d.PAYMENTNO.Equals(paymentNo))
                select p
                ).ToList();

            _context.THARVESTRESULT1.RemoveRange(results);
            _context.SaveChanges();
        }

        public void DeleteGerdanByDiv(string divId, DateTime date)
        {
            List<TGERDANRESULT> results =
                (
                from p in _context.TGERDANRESULT
                .Where(d => d.DIVID.Equals(divId) && d.HARVESTDATE.Date == date.Date)
                select p
                ).ToList();
            foreach (TGERDANRESULT p in results)
            {
                p.STATUS = "D";
                p.UPDATED = GetServerTime();

            }
            _context.SaveChanges();
        }

        public void DeleteGerdanByPaymentNo(string paymentNo)
        {
            //No Action From SP sp_HarvestingResult_DeleteGerdanByPaymentNo
        }

        public override IEnumerable<THARVESTRESULT1> GetList(FilterHarvestResult filter)
        {
            
            var criteria = PredicateBuilder.True<THARVESTRESULT1>();

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.HARVESTCODE.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));

                if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                    criteria = criteria.And(p => p.DIVID.Equals(filter.DivisionID));

                if (!string.IsNullOrWhiteSpace(filter.HarvestCode))
                    criteria = criteria.And(p => p.HARVESTCODE.Equals(filter.HarvestCode));

                if (filter.EmpType != null)
                    criteria = criteria.And(p => p.EMPLOYEETYPE.Equals(filter.EmpType));

                if (!filter.Date.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.HARVESTDATE.Date == filter.Date.Date);

                if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.HARVESTDATE.Date >= filter.StartDate.Date && p.HARVESTDATE.Date <= filter.EndDate.Date);

                var result = _context.THARVESTRESULT1.Where(criteria);
                if (filter.PageSize <= 0)
                    return result;
                return result.GetPaged(filter.PageNo, filter.PageSize).Results;
            }
            catch { return new List<THARVESTRESULT1>(); }
        }

        public List<THARVESTRESULT1> GetGerdanByUnitAndDate(string unitId, DateTime startDate, DateTime endDate)
        {
            var result = new List<THARVESTRESULT1>();
            {
                List<TGERDANRESULT> results =
                               (
                               from p in _context.TGERDANRESULT
                               .Where(d => d.DIVID.StartsWith(unitId) &&
                               d.HARVESTDATE.Date >= startDate.Date && d.HARVESTDATE.Date <= endDate.Date)
                               select p
                               ).ToList();

                THARVESTRESULT1 harvestresult1;
                foreach (TGERDANRESULT item in results)
                {
                    harvestresult1 = new THARVESTRESULT1();
                    harvestresult1.CopyFrom(item);
                    result.Add(harvestresult1);
                }
            }
            return result;
        }

        public List<THARVESTRESULT1> GetDailyCollection(string divId, DateTime date)
        {
            var result = new List<THARVESTRESULT1>();
            List<sp_HarvestingResult_GetDaily_Result> data = _context.sp_HarvestingResult_GetDaily_Result(divId, date.Date).ToList();

            THARVESTRESULT1 harvestresult1;
            foreach (var item in data)
            {
                harvestresult1 = new THARVESTRESULT1();
                harvestresult1.CopyFrom(item);
                result.Add(harvestresult1);
            }
            return result;
        }

        public List<THARVESTRESULT1> GetGerdanDailyCollection(string divId, DateTime date)
        {
            var result = new List<THARVESTRESULT1>();
            List<sp_HarvestingResult_GetGerdanDaily_Result> data = _context.sp_HarvestingResult_GetGerdanDaily(divId, date.Date).ToList();

            THARVESTRESULT1 harvestresult1;
            foreach (var item in data)
            {
                harvestresult1 = new THARVESTRESULT1();
                harvestresult1.CopyFrom(item);
                result.Add(harvestresult1);
            }
            return result;
        }

        public List<THARVESTRESULT1> GetFromHarvestingCollection
        (string unitCode, string basedCalculation, string premiSystem, DateTime startperiod, DateTime startdate, DateTime endDate, int period, bool basisByKg)
        {
            var result = new List<THARVESTRESULT1>();
            List<sp_HarvestingResult_GetFromHarvestingCollection_Result> data = 
                _context.sp_HarvestingResult_GetFromHarvestingCollection_Result(unitCode, basedCalculation, premiSystem, startperiod, startdate,endDate,period,basisByKg).ToList();

            THARVESTRESULT1 harvestresult1;
            foreach (var item in data)
            {
                harvestresult1 = new THARVESTRESULT1();
                harvestresult1.CopyFrom(item);
                result.Add(harvestresult1);
            }
            return result;

        }

        public List<THARVESTRESULT1> GetGerdanFromHarvestingCollection(string unitCode, DateTime startdate, DateTime endDate, bool basisByKg)
        {
            var result = new List<THARVESTRESULT1>();
            List<sp_HarvestingResult_GetGerdanFromHarvestingCollection_Result> data =
                _context.sp_HarvestingResult_GetGerdanFromHarvestingCollection_Result(unitCode, startdate, endDate, basisByKg).ToList();

            THARVESTRESULT1 harvestresult1;
            foreach (var item in data)
            {
                harvestresult1 = new THARVESTRESULT1();
                harvestresult1.CopyFrom(item);
                result.Add(harvestresult1);
            }
            return result;

        }

        public void InsertGerdan(THARVESTBLOCKRESULT harvestingResult)
        {
            TGERDANRESULT data = new TGERDANRESULT();
            data.CopyFrom(harvestingResult);
            _context.TGERDANRESULT.Add(data);
            _context.SaveChanges();
        }
               
    }
}
