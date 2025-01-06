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
using System.Globalization;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Attendances
{
    public class AttendanceGroupItem : EntityFactory<MATTENDANCEGROUPITEM, MATTENDANCEGROUPITEM,GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public AttendanceGroupItem(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "AttendanceGroupItem";
            _authenticationService = authenticationService;
            
        }

        protected override MATTENDANCEGROUPITEM GetSingleFromDB(params  object[] keyValues)
        {
            string Id = keyValues[0].ToString();
            return _context.MATTENDANCEGROUPITEM.SingleOrDefault(d => d.ID.Equals(Id));
        }

        public override IEnumerable<MATTENDANCEGROUPITEM> GetList(GeneralFilter filter)
        {
            try
            {
                return _context.MATTENDANCEGROUPITEM.ToList();
            }
            catch { return new List<MATTENDANCEGROUPITEM>(); }
        }
    }
}
