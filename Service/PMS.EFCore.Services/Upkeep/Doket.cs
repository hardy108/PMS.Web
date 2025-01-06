using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.Utilities;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Upkeep
{
    public class Doket:EntityFactory<VDOKET, VDOKET, GeneralFilter, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        public Doket(PMSContextBase context,AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "Doket";
            _authenticationService = authenticationService;
            
        }

        protected override VDOKET GetSingleFromDB(params  object[] keyValues)
        {
            string doketId = keyValues[0].ToString();
            return _context.VDOKET.SingleOrDefault(d => d.DOK_ID.Equals(doketId));
        }
    }
}
