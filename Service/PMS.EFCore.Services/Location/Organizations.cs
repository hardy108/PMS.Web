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
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Location
{
    public class Organizations : EntityFactory<MORGANIZATION,MORGANIZATION, FilterMorganization, PMSContextBase>
    {
        public Organizations(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Organizations";
        }

        protected override MORGANIZATION GetSingleFromDB(params  object[] keyValues)
        {
            return base.GetSingle(keyValues);
        }

        //public DataTable Find (string code, string name)
        //{
        //    IEnumerable<MORGANIZATION> query = _context.MORGANIZATION.AsEnumerable();
        //       DataTable dt = query.CopyToDataTable();
        //    return dt;

        //}

        public override IEnumerable<MORGANIZATION> GetList(FilterMorganization filter)
        {
            
            var criteria = PredicateBuilder.True<MORGANIZATION>();

            try
            {
                //Added By Hardi 2020-04-06 - Start
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    criteria = criteria.And(p =>
                        p.ID.ToLower().Contains(filter.LowerCasedSearchTerm)
                    );
                }
                //Added By Hardi 2020-04-06 - End

                if (filter.IsActive.HasValue)
                    criteria = criteria.And(p => p.ACTIVE == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Id))
                    criteria = criteria.And(p => p.ID.StartsWith(filter.Id));

                if (!string.IsNullOrWhiteSpace(filter.Type))
                    criteria = criteria.And(p => p.ID.Equals(filter.Type));

                if (filter.PageSize <= 0)
                    return _context.MORGANIZATION
                        .Where(criteria);

                return _context.MORGANIZATION
                    .Where(criteria)
                    .GetPaged(filter.PageNo, filter.PageSize).Results;

            }

            catch { return new List<MORGANIZATION>(); }
        }

        public VORGANIZATION GetSingleV(string Id)
        {
            //string Id = keyValues[0].ToString();
            return _context.VORGANIZATION.SingleOrDefault(d => d.ID.Equals(Id));
        }
    }
}
