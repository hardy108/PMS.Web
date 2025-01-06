using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using PMS.EFCore.Services.General;
using AM.EFCore.Services;
using PMS.Shared.Models;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using GeoJSON.Net.Feature;

namespace PMS.EFCore.Services.Logistic
{
    public class EMTracking : EntityFactory<EM_TTRACKING, EM_TTRACKING, GeneralFilter, PMSContextBase>
    {



        private AuthenticationServiceBase _authenticationService;
        public EMTracking(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "EM-Tracking";
            _authenticationService = authenticationService;
        }

        public override IEnumerable<EM_TTRACKING> GetList(GeneralFilter filter)
        {

            var criteria = BuildQueryFilter(filter);

            if (filter.PageSize <= 0)
                return _context.EM_TTRACKING.Where(criteria).OrderBy(d => d.id).ThenBy(d => d.seq);
            return _context.EM_TTRACKING.Where(criteria).OrderBy(d => d.id).ThenBy(d => d.seq).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        protected override EM_TTRACKING BeforeSave(EM_TTRACKING record, string userName, bool newRecord)
        {
            record.createUser = userName;
            return record;
        }

        public Feature GenerateGeoJsonLineStringByTrackingId(string Id)
        {
            GeneralFilter filter = new GeneralFilter();
            filter.Id = Id;
            var points = GetList(filter).ToList();
            if (StandardUtility.IsEmptyList(points))
                return null;
            string result = string.Empty;
            List<IPosition> coordinates = new List<IPosition>();

            var props = new Dictionary<string, object>();
            int seq = 0;
            points.ForEach(d => {
                coordinates.Add(new GeoJSON.Net.Geometry.Position(d.latitude, d.longitude));
                seq++;
                props.Add(seq.ToString(), (d.date.HasValue ? d.date.Value.ToString("dd-MMM-yy HH:mm:ss") : string.Empty));
            });
            LineString lineString = new LineString(coordinates);

            Feature feature = new Feature(lineString);
            return feature;
        }

        public TrackingPoints GetTrackingPoints(string Id)
        {
            GeneralFilter filter = new GeneralFilter();
            filter.Id = Id;
            var points = GetList(filter).ToList();
            if (StandardUtility.IsEmptyList(points))
                return null;

            TrackingPoints trackingPoints = new TrackingPoints();

            List<decimal[]> coordinates = new List<decimal[]>();
            List<string> dates = new List<string>();
            
            points.ForEach(d => {            
                decimal lt = 0, ln = 0;
                decimal.TryParse(d.latitude, out lt);
                decimal.TryParse(d.longitude, out ln);
                decimal[] point = { lt,ln  };
                coordinates.Add(point);
                dates.Add(d.date.HasValue ? d.date.Value.ToString("dd-MMM-yy HH:mm:ss"):string.Empty);
            });
            trackingPoints.Dates = dates;
            trackingPoints.Coordinates = coordinates;
            return trackingPoints;
        }
        
        public IEnumerable<TrackingInfo> GetListTrackingInfo(GeneralFilter filter)
        {

            var criteria = BuildQueryFilter(filter);

            var list = _context.EM_TTRACKING.Where(criteria).GroupBy(d => d.id).Select(d => new TrackingInfo
            {
                Id = d.Key,
                Username = d.Max(s => s.createUser),
                StartTime = d.Min(s => s.date.Value),
                EndTime = d.Max(s => s.date.Value),
                FullName = d.Max(s => s.createUser)
            });

            var result = new List<TrackingInfo>();
            if (filter.PageSize <= 0)
                result = list.ToList();
            else
                result = list.GetPaged(filter.PageNo, filter.PageSize).Results.ToList();
            return result;
        }

        public TrackingInfo GetTrackingInfo(string Id)
        {
            return _context.EM_TTRACKING.Where(d => d.id.Equals(Id)).GroupBy(d => d.id).Select(d => new TrackingInfo
            {
                Id = d.Key,
                Username = d.Max(s => s.createUser),
                StartTime = d.Min(s => s.date.Value),
                EndTime = d.Max(s => s.date.Value),
                FullName = d.Max(s => s.createUser)
            }).FirstOrDefault();
        }

        

        public long GetListCountTrackingInfo(GeneralFilter filter) => _context.EM_TTRACKING.Where(BuildQueryFilter(filter)).Select(d => d.id).Distinct().Count();

        private System.Linq.Expressions.Expression<Func<EM_TTRACKING,bool>> BuildQueryFilter(GeneralFilter filter)
        {
            var criteria = PredicateBuilder.True<EM_TTRACKING>();

            if (string.IsNullOrWhiteSpace(filter.RecordStatus))
                criteria = criteria.And(d => !d.status.Equals(PMSConstants.TransactionStatusDeleted));
            else
                criteria = criteria.And(d => d.status.Equals(filter.RecordStatus));

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(d => d.id.Equals(filter.Id));

            //DateTime nullDate = new DateTime();
            //if (filter.StartDate != nullDate || filter.EndDate != nullDate)
            //{
            //    if (filter.EndDate == nullDate)
            //        criteria = criteria.And(d => d.date.Value.Date == filter.StartDate.Date);
            //    else if (filter.StartDate == nullDate)
            //        criteria = criteria.And(d => d.date.Value.Date == filter.EndDate.Date);
            //    else
            //        criteria = criteria.And(d => d.date.Value.Date >= filter.StartDate.Date && d.date.Value.Date <= filter.EndDate.Date);
            //}


            //if (!string.IsNullOrWhiteSpace(filter.Keyword))
            //    criteria = criteria.And(d => d.title.ToLower().Contains(filter.LowerCasedSearchTerm) || d.createUser.ToLower().Contains(filter.LowerCasedSearchTerm));
            return criteria;
        }
        
    }


    public class TrackingPoints
    {
        public List<decimal[]> Coordinates { get; set; }
        public List<string> Dates { get; set; }

    }


    public class TrackingInfo
    {
        public string Id { get; set; }        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public string Username { get; set; }
        public string FullName { get; set; }

        public DateTime Date => StartTime.Date;
        public string StartTime_In_Text => StartTime.ToString("dd-MMM-yyyy HH:mm:ss");
        public string EndTime_In_Text => EndTime.ToString("dd-MMM-yyyy HH:mm:ss");
    }
}
