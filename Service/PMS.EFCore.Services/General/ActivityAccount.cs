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
using AM.EFCore.Services;

namespace PMS.EFCore.Services.General
{
    public class ActivityAccount : EntityFactory<MACTIVITYACCOUNT, MACTIVITYACCOUNT,GeneralFilter, PMSContextBase>
    {
        public ActivityAccount(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "ActivityAccount";
        }

        protected override MACTIVITYACCOUNT GetSingleFromDB(params  object[] keyValues)
        {
            string id = keyValues[0].ToString();
            var actAcc = _context.MACTIVITYACCOUNT.SingleOrDefault(d => d.ACTIVITYID.Equals(id));
            if(actAcc != null)
            {
                var criteria = PredicateBuilder.True<MACCOUNT>();
                criteria = criteria.Or(p => p.CODE == actAcc.GAMATDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.GAHKDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.NMATDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.NHKDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.LCMATDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.LCHKDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.TBMMATDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.TBMHKDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.TMMATDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.TMHKDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.HKCRED);
                criteria = criteria.Or(p => p.CODE == actAcc.HKCREDASST);
                criteria = criteria.Or(p => p.CODE == actAcc.CEHKDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.CEMATDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.RAHKDEBT);
                criteria = criteria.Or(p => p.CODE == actAcc.RAMATDEBT);

                var account = _context.MACCOUNT.Where(criteria).ToList();

                actAcc.ACTIVITY = _context.MACTIVITY.Where(a => a.ACTIVITYID.Equals(id)).SingleOrDefault();
                actAcc.CEHKDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.CEHKDEBT)).SingleOrDefault();
                actAcc.CEHKDEBT = account.Where(a => a.CODE.Equals(actAcc.CEHKDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.CEMATDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.CEMATDEBT)).SingleOrDefault();
                actAcc.CEMATDEBT = account.Where(a => a.CODE.Equals(actAcc.CEMATDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.GAHKDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.GAHKDEBT)).SingleOrDefault();
                actAcc.GAHKDEBT = account.Where(a => a.CODE.Equals(actAcc.GAHKDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.GAMATDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.GAMATDEBT)).SingleOrDefault();
                actAcc.GAMATDEBT = account.Where(a => a.CODE.Equals(actAcc.GAMATDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.HKCREDNavigation = account.Where(a => a.CODE.Equals(actAcc.HKCRED)).SingleOrDefault();
                actAcc.HKCRED = account.Where(a => a.CODE.Equals(actAcc.HKCRED)).Select(a => a.CODE).SingleOrDefault();
                //actAcc.HKCREDASSTNavigation = account.Where(a => a.CODE.Equals(actAcc.HKCREDASST)).SingleOrDefault();
                actAcc.HKCREDASST = account.Where(a => a.CODE.Equals(actAcc.HKCREDASST)).Select(a => a.CODE).SingleOrDefault();
                actAcc.LCHKDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.LCHKDEBT)).SingleOrDefault();
                actAcc.LCHKDEBT = account.Where(a => a.CODE.Equals(actAcc.LCHKDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.LCMATDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.LCMATDEBT)).SingleOrDefault();
                actAcc.LCMATDEBT = account.Where(a => a.CODE.Equals(actAcc.LCMATDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.NHKDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.NHKDEBT)).SingleOrDefault();
                actAcc.NHKDEBT = account.Where(a => a.CODE.Equals(actAcc.NHKDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.NMATDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.NMATDEBT)).SingleOrDefault();
                actAcc.NMATDEBT = account.Where(a => a.CODE.Equals(actAcc.NMATDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.RAHKDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.RAHKDEBT)).SingleOrDefault();
                actAcc.RAHKDEBT = account.Where(a => a.CODE.Equals(actAcc.RAHKDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.RAMATDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.RAMATDEBT)).SingleOrDefault();
                actAcc.RAMATDEBT = account.Where(a => a.CODE.Equals(actAcc.RAMATDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.TBMHKDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.TBMHKDEBT)).SingleOrDefault();
                actAcc.TBMHKDEBT = account.Where(a => a.CODE.Equals(actAcc.TBMHKDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.TBMMATDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.TBMMATDEBT)).SingleOrDefault();
                actAcc.TBMMATDEBT = account.Where(a => a.CODE.Equals(actAcc.TBMMATDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.TMHKDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.TMHKDEBT)).SingleOrDefault();
                actAcc.TMHKDEBT = account.Where(a => a.CODE.Equals(actAcc.TMHKDEBT)).Select(a => a.CODE).SingleOrDefault();
                actAcc.TMMATDEBTNavigation = account.Where(a => a.CODE.Equals(actAcc.TMMATDEBT)).SingleOrDefault();
                actAcc.TMMATDEBT = account.Where(a => a.CODE.Equals(actAcc.TMMATDEBT)).Select(a => a.CODE).SingleOrDefault();
            }
            return actAcc;
        }

        public override IEnumerable<MACTIVITYACCOUNT> GetList(GeneralFilter filter)
        {
            
            var criteria = PredicateBuilder.True<MACTIVITYACCOUNT>();

            if (!string.IsNullOrWhiteSpace(filter.Id))
                criteria = criteria.And(p => p.ACTIVITYID.Equals(filter.Id));
            if (filter.Ids.Any())
                criteria = criteria.And(p => filter.Ids.Contains(p.ACTIVITYID));

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                criteria = criteria.And(d => d.ACTIVITYID.Contains(filter.Keyword));

            if (filter.PageSize <= 0)
                return _context.MACTIVITYACCOUNT.Where(criteria);
            return _context.MACTIVITYACCOUNT.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;
        }

        

    }
}
