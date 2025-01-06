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
using PMS.EFCore.Services.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Logistic
{
    public class LoadingResult:EntityFactory<TLOADINGRESULT,TLOADINGRESULT,FilterLoading, PMSContextBase>
    {
        private Period _periodService;
        private AuthenticationServiceBase _authenticationService;
        public LoadingResult(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "LoadingResult";
            _authenticationService = authenticationService;
            _periodService = new Period(context,_authenticationService,auditContext);
        }

        public void DeleteByDiv(string divId, DateTime date)
        {
            List<TLOADINGRESULT> results =
            (
            from p in _context.TLOADINGRESULT
            .Where(d => d.DIVID.Equals(divId) && d.LOADINGDATE.Date == date.Date)
            select p
            ).ToList();
            foreach (TLOADINGRESULT p in results)
            {
                p.STATUS = "D";
                p.UPDATED = GetServerTime();
            }
            _context.SaveChanges();
        }

        public void DeleteByUnitDate(string unitCode, DateTime date)
        {
            List<TLOADINGRESULT> results =
            (
            from p in _context.TLOADINGRESULT
            .Where(d => d.UNITCODE.Equals(unitCode) && d.LOADINGDATE.Date == date.Date)
            select p
            ).ToList();

            _context.TLOADINGRESULT.RemoveRange(results);
            _context.SaveChanges();
        }

        public void DeleteByPaymentNo(string paymentNo)
        {
            List<TLOADINGRESULT> results =
            (
            from p in _context.TLOADINGRESULT
            .Where(d => d.PAYMENTNO.Equals(paymentNo))
            select p
            ).ToList();

            _context.TLOADINGRESULT.RemoveRange(results);
            _context.SaveChanges();

        }

        public override IEnumerable<TLOADINGRESULT> GetList(FilterLoading filter)
        {
            
            var criteria = PredicateBuilder.True<TLOADINGRESULT>();

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.LOADINGCODE.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.RecordStatus))
                    criteria = criteria.And(p => p.STATUS.Equals(filter.RecordStatus));

                if (!string.IsNullOrWhiteSpace(filter.UnitID))
                    criteria = criteria.And(p => p.UNITCODE.Equals(filter.UnitID));

                if (!string.IsNullOrWhiteSpace(filter.DivisionID))
                    criteria = criteria.And(p => p.DIVID.Equals(filter.DivisionID));

                if (!string.IsNullOrWhiteSpace(filter.LoadingCode))
                    criteria = criteria.And(p => p.LOADINGCODE.Equals(filter.LoadingCode));

                if (filter.EmpType != null)
                    criteria = criteria.And(p => p.EMPLOYEETYPE.Equals(filter.EmpType));

                if (filter.ProductId != null)
                    criteria = criteria.And(p => p.PRODUCTID.Equals(filter.ProductId));

                if (filter.LoadingType != null)
                    criteria = criteria.And(p => p.LOADINGTYPE.Equals(filter.LoadingType));

                if (filter.SPBDataType != null)
                    criteria = criteria.And(p => p.SPBDATATYPE.Equals(filter.SPBDataType));

                if (!string.IsNullOrWhiteSpace(filter.NoSPB))
                    criteria = criteria.And(p => p.NOSPB.Contains(filter.NoSPB));

                if (!string.IsNullOrWhiteSpace(filter.VehicleId))
                    criteria = criteria.And(p => p.VEHICLEID.Contains(filter.VehicleId));

                if (!string.IsNullOrWhiteSpace(filter.VehicleTypeId))
                    criteria = criteria.And(p => p.VEHICLETYPEID.Contains(filter.VehicleTypeId));

                if (filter.LoadingPaymentType != null)
                    criteria = criteria.And(p => p.LOADINGPAYMENTTYPE.Equals(filter.LoadingPaymentType));

                if (!string.IsNullOrWhiteSpace(filter.ActivityId))
                    criteria = criteria.And(p => p.ACTIVITYID.Contains(filter.ActivityId));

                if (!filter.Eflag !=null)
                    criteria = criteria.And(p => p.EFLAG.Equals(filter.Eflag));

                if (!filter.Date.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.LOADINGDATE.Date == filter.Date.Date);

                if (!filter.StartDate.Equals(DateTime.MinValue) && !filter.EndDate.Equals(DateTime.MinValue))
                    criteria = criteria.And(p => p.LOADINGDATE.Date >= filter.StartDate.Date && p.LOADINGDATE.Date <= filter.EndDate.Date);

                var result = _context.TLOADINGRESULT.Where(criteria);
                if (filter.PageSize <= 0)
                    return result;
                return result.GetPaged(filter.PageNo, filter.PageSize).Results;
            }

            catch { return new List<TLOADINGRESULT>(); }
        }

        public void InsertAll(IEnumerable<TLOADINGRESULT> loading, string unitId, DateTime date, string by)
        {
            _periodService.CheckValidPeriod(unitId, date.Date);
            this.DeleteByUnitDate(unitId, date.Date);
            foreach (TLOADINGRESULT operating in loading)
            {
                _context.Entry<TLOADINGRESULT>(operating).State = EntityState.Added;
                _context.SaveChanges();
            }

        }

        public List<TLOADINGRESULT> GetDailyCollection(string divId, DateTime date)
        {
            var result = new List<TLOADINGRESULT>();
            List<sp_LoadingResult_GetDaily_Result> data = _context.sp_LoadingResult_GetDaily_Result(divId, date.Date).ToList();

            TLOADINGRESULT loadingresult;
            foreach (var item in data)
            {
                loadingresult = new TLOADINGRESULT();
                loadingresult.CopyFrom(item);
                result.Add(loadingresult);
            }
            return result;

        }

        public void UpdatePaymentNo(string unitCode, DateTime startDate, DateTime endDate, string paymentNo, string by, DateTime dateTime)
        {
            List<TLOADINGRESULT> results =
                (
                from p in _context.TLOADINGRESULT
                .Where(d => d.UNITCODE.Equals(unitCode) && d.LOADINGDATE.Date >= startDate.Date && d.LOADINGDATE.Date <= endDate.Date)
                select p
                ).ToList();
            foreach (TLOADINGRESULT p in results)
            {
                p.PAYMENTNO = paymentNo;
                p.UPDATED = GetServerTime();

            }
            _context.SaveChanges();
        }

    }
}
